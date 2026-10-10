using System;

// The cold-side physics of Health_System.md's Core Temperature Model (2026-10-07), as plain math with no Unity types so
// SurvivalManager can call it and it can also be run on its own for tuning. Core temperature (°C) is the one stored
// value; Warmth is read off it through the piecewise table below. Each step, core temperature moves by
//   (heat produced + radiant gain − heat lost − placeholder dissipation) ÷ the body's heat capacity.
// Produced: resting watts × MET for the activity, or for the shivering the body adds when it is cold, whichever is more.
// Lost: skin-to-air through clothing and a thin air layer (thinner in wind); wet or sweat-damp clothing insulates
// worse. A campfire is radiant gain, independent of clothing. All numbers are first-pass starting points (see the
// Settings defaults) for Mike to confirm by feel.
public static class BodyHeat
{
    [Serializable]
    public class Settings
    {
        // The body (a 70 kg player).
        public float restWatts = 80f;
        public float heatCapacityWhPerC = 67f;
        public float skinC = 33f;
        public float areaM2 = 1.8f;

        // Heat transfer. Still air through convection and radiation is about 7.8 W/m²K (a 0.13 m²K/W air layer); wind adds
        // forced convection that grows with the square root of the speed.
        public float stillAirH = 7.8f;
        public float windH = 8.3f;
        // 1 clo is 0.155 m²K/W.
        public float resistancePerClo = 0.155f;
        // Wet clothing loses most of its insulation (the doc's ×5 on clothing resistance only); sweat-damp is a mild wet.
        public float wetFactor = 4f;
        public float dampFactor = 1f;

        // Shivering: starts at the bottom of the normal band, reaches full by shivering-ramp degrees lower, fades out
        // between fade-from and end (no cliff at one temperature), and is fuelled by Hunger.
        public float shiverStartC = 36.5f;
        public float shiverRampC = 1f;
        public float shiverFadeFromC = 33f;
        public float shiverEndC = 30f;
        public float shiverPeakMet = 4.9f;
        public float shiverFuelHunger = 25f; // below this much Hunger, shivering weakens

        // Placeholder until the hot side exists: above the normal core temperature the body sheds heat (blood flow and
        // sweat) at this many watts per degree, so hard work in the cold settles a little above 37 °C instead of running away.
        public float normalCoreC = 37f;
        public float dissipationWattsPerC = 400f;

        public float minCoreC = 24f;
        public float maxCoreC = 41f;
    }

    // Warmth 0, 25, 50, 75, 100 ↔ core temperature (straight lines between).
    static readonly float[] WarmthPoints = { 0f, 25f, 50f, 75f, 100f };
    static readonly float[] CorePoints = { 28f, 32f, 35f, 36.5f, 37f };

    public static float CoreToWarmth(float coreC) => Interpolate(CorePoints, WarmthPoints, coreC);
    public static float WarmthToCore(float warmth) => Interpolate(WarmthPoints, CorePoints, warmth);

    static float Interpolate(float[] from, float[] to, float value)
    {
        if (value <= from[0])
            return to[0];
        for (int i = 1; i < from.Length; i++)
            if (value <= from[i])
                return to[i - 1] + (to[i] - to[i - 1]) * ((value - from[i - 1]) / (from[i] - from[i - 1]));
        return to[to.Length - 1];
    }

    public struct Conditions
    {
        public float airC;
        public float windMetersPerSecond; // at the body, after any shelter
        public float clo;
        public float wetness;             // 0-1, soaked by rain
        public float damp;                // 0-1, soaked by sweat
        public float extraResistance;     // bedding and shelter, m²K/W
        public float activityMet;
        public float radiantWatts;        // from a campfire
        public float hunger;              // 0-100, fuels shivering
        public float lossMultiplier;      // difficulty: heat loss scaled in the harmful direction
        public float rewarmMultiplier;    // game-feel boost on the body's own net heat gain; 0 or 1 = none
        public float rewarmBelowC;        // the boost applies only while core temperature is below this
    }

    public struct Result
    {
        public float coreC;
        public float shiver;       // 0-1
        public float totalMet;     // what the body is producing, in multiples of resting
        public float productionWatts;
        public float lossWatts;
        public float netWatts;
    }

    // Seasonal clothing until a clothing system exists: the player dresses for the recent temperature.
    public static float Clo(float smoothedC, float winterClo, float summerClo, float coldC, float warmC)
    {
        float t = warmC <= coldC ? 0f : Math.Min(1f, Math.Max(0f, (smoothedC - coldC) / (warmC - coldC)));
        return winterClo + (summerClo - winterClo) * t;
    }

    public static float AirResistance(Settings s, float windMetersPerSecond) =>
        1f / (s.stillAirH + s.windH * (float)Math.Sqrt(Math.Max(0f, windMetersPerSecond)));

    public static float TotalResistance(Settings s, in Conditions c)
    {
        float clothing = c.clo * s.resistancePerClo / (1f + s.wetFactor * c.wetness + s.dampFactor * c.damp);
        return clothing + AirResistance(s, c.windMetersPerSecond) + c.extraResistance;
    }

    // The shivering that makes heat (the body's, from shiverStartC): feeds the heat balance, the stamina penalty and the
    // Hunger cost, so it is what the resting plateaus rest on.
    public static float ShiverIntensity(Settings s, float coreC, float hunger) =>
        ShiverBetween(s, coreC, hunger, s.shiverStartC, s.shiverStartC - Math.Max(0.01f, s.shiverRampC));

    // The shivering the player is shown and hears (HUD word, Shiver.wav): the same fade-out and Hunger fuel as above, but
    // it starts and reaches full at its own, lower core temperatures (Health_System.md, Shivering Threshold, Mike 2026-10-10).
    // It changes nothing about the heat physics; it only decides when shivering is announced.
    public static float VisibleShiver(Settings s, float coreC, float hunger, float onsetC, float fullC) =>
        ShiverBetween(s, coreC, hunger, onsetC, fullC);

    static float ShiverBetween(Settings s, float coreC, float hunger, float onsetC, float fullC)
    {
        float rise = Clamp01((onsetC - coreC) / Math.Max(0.01f, onsetC - fullC));
        float fade = coreC >= s.shiverFadeFromC ? 1f : Clamp01((coreC - s.shiverEndC) / Math.Max(0.01f, s.shiverFadeFromC - s.shiverEndC));
        float fuel = Clamp01(hunger / Math.Max(0.01f, s.shiverFuelHunger));
        return rise * fade * fuel;
    }

    // Advances core temperature by some game hours, in short steps (shivering and dissipation correct quickly).
    public static Result Advance(Settings s, float coreC, in Conditions c, float hours)
    {
        const float MaxStepHours = 0.05f;
        int steps = Math.Max(1, (int)Math.Ceiling(hours / MaxStepHours));
        float dt = hours / steps;
        float resistance = TotalResistance(s, c);
        Result r = default;
        for (int i = 0; i < steps; i++)
        {
            float shiver = ShiverIntensity(s, coreC, c.hunger);
            float shiverMet = 1f + shiver * (s.shiverPeakMet - 1f);
            float totalMet = Math.Max(c.activityMet, shiverMet);
            float production = s.restWatts * totalMet;

            float dry = s.areaM2 * (s.skinC - c.airC) / resistance;
            float loss = dry > 0f ? dry * c.lossMultiplier : dry; // heat gained from warm air isn't scaled
            float dissipation = Math.Max(0f, coreC - s.normalCoreC) * s.dissipationWattsPerC;
            // Rewarming boost (Health_System.md, Warmth Regeneration, Mike 2026-10-10): only the body's OWN net gain is
            // multiplied, and only while below the ceiling. A net loss is never touched (so cooling and the resting
            // plateaus are unchanged, because a plateau is net zero), and a campfire's radiant watts are added after.
            float body = production - loss - dissipation;
            if (body > 0f && c.rewarmMultiplier > 1f && coreC < c.rewarmBelowC)
                body *= c.rewarmMultiplier;
            float net = body + c.radiantWatts;

            coreC = Math.Min(s.maxCoreC, Math.Max(s.minCoreC, coreC + net / s.heatCapacityWhPerC * dt));
            r = new Result { coreC = coreC, shiver = shiver, totalMet = totalMet, productionWatts = production, lossWatts = loss, netWatts = net };
        }
        return r;
    }

    static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
}
