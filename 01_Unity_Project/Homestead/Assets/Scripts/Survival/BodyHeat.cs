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

        // Above the normal core temperature the body sheds heat (blood flow and sweat) at this many watts per degree, so hard
        // work in the cold settles a little above 37 °C instead of running away. In warm air the sweat glands add to it (the
        // hot side, Weather_System.md's Heat Inputs): the rate rises from the first figure to the second as the air warms from
        // hotSweatFromAirC to hotSweatFullAirC, so the cold side's tuning is untouched. All of it is scaled by how well sweat
        // evaporates (the caller's sweatEfficiency: wet-bulb, wind, Hydration), then capped at maxSweatWatts.
        public float normalCoreC = 37f;
        public float dissipationWattsPerC = 400f;
        public float hotSweatWattsPerC = 1000f;
        public float hotSweatFromAirC = 18f;
        public float hotSweatFullAirC = 26f;
        public float maxSweatWatts = 1000f;
        // Heat stroke: sweating fades rather than stopping (a stop would be a runaway). Full at strokeFromC, down to
        // strokeSweatFloor of itself by strokeFloorAtC.
        public float strokeFromC = 40f;
        public float strokeFloorAtC = 41.5f;
        public float strokeSweatFloor = 0.25f;

        public float minCoreC = 24f;
        public float maxCoreC = 41f;
    }

    // --- Air and sweat (the hot side) ---

    // Saturation vapour pressure in hPa (Magnus).
    public static float SaturationHPa(float tempC) => 6.112f * (float)Math.Exp(17.62f * tempC / (243.12f + tempC));

    // Relative humidity, 0-100, of air at tempC whose dew point is dewC.
    public static float RelativeHumidity(float tempC, float dewC) =>
        Math.Min(100f, Math.Max(1f, 100f * SaturationHPa(Math.Min(dewC, tempC)) / SaturationHPa(tempC)));

    // Wet-bulb temperature (Stull 2011), good for 5-99 % humidity and -20 to 50 °C: inputs are clamped to that.
    public static float WetBulbC(float tempC, float humidity)
    {
        float t = Math.Min(50f, Math.Max(-20f, tempC));
        float rh = Math.Min(99f, Math.Max(5f, humidity));
        return (float)(t * Math.Atan(0.151977 * Math.Sqrt(rh + 8.313659)) + Math.Atan(t + rh) - Math.Atan(rh - 1.676331)
                       + 0.00391838 * Math.Pow(rh, 1.5) * Math.Atan(0.023101 * rh) - 4.686035);
    }

    // Heat index in °F (the NWS "feels like"): the Steadman simple formula, and above about 80 °F the Rothfusz regression with the
    // NWS low-humidity (RH under 13 % at 80-112 °F) and high-humidity (RH over 85 % at 80-87 °F) adjustments. For display only: the body model
    // never reads it. Below 80 °F the air temperature itself is returned.
    public static float HeatIndexF(float tempF, float humidity)
    {
        double t = tempF, rh = Math.Min(100.0, Math.Max(0.0, humidity));
        if (t < 80.0)
            return tempF;

        double hi = 0.5 * (t + 61.0 + (t - 68.0) * 1.2 + rh * 0.094);
        if ((hi + t) / 2.0 >= 80.0)
        {
            hi = -42.379 + 2.04901523 * t + 10.14333127 * rh - 0.22475541 * t * rh - 0.00683783 * t * t - 0.05481717 * rh * rh
                 + 0.00122874 * t * t * rh + 0.00085282 * t * rh * rh - 0.00000199 * t * t * rh * rh;
            if (rh < 13.0 && t <= 112.0)
                hi -= (13.0 - rh) / 4.0 * Math.Sqrt((17.0 - Math.Abs(t - 95.0)) / 17.0);
            else if (rh > 85.0 && t <= 87.0)
                hi += (rh - 85.0) / 10.0 * ((87.0 - t) / 5.0);
        }
        return (float)hi;
    }

    // The NWS band for a heat index in °F; empty under 80.
    public static string HeatIndexBand(float heatIndexF) =>
        heatIndexF >= 130f ? "Extreme Danger" : heatIndexF >= 105f ? "Danger" : heatIndexF >= 90f ? "Extreme Caution" : heatIndexF >= 80f ? "Caution" : "";

    // The fraction of sweat that actually cools (0-1): full at or below fullC of wet-bulb, none at or above zeroC (about 35 °C,
    // the physical survival limit where sweat cools no one), straight between. Wind raises it (never from zero).
    public static float EvaporationFactor(float wetBulbC, float fullC, float zeroC, float wind01, float windBoost)
    {
        float f = Clamp01((zeroC - wetBulbC) / Math.Max(0.01f, zeroC - fullC));
        return Clamp01(f * (1f + windBoost * Clamp01(wind01)));
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
        public float solarWatts;          // heat gained from the sun, after cloud and shade
        public float sweatEfficiency;     // 0-1: evaporation factor × the sweat Hydration can supply (see SweatEfficiency)
        public float waterCoolingWatts;   // heat taken by wading and the damp skin after it (0 = none: everything else is unchanged)
        // Water never chills a body that is already cool: its watts are multiplied by 1 at waterTaperFullC and above, 0 at
        // waterTaperNoneC and below. They come from the caller (not Settings): a field added to a serialized class that a saved
        // scene already holds loads as 0, which silently turned this taper off.
        public float waterTaperFullC;
        public float waterTaperNoneC;
    }

    public struct Result
    {
        public float coreC;
        public float shiver;       // 0-1
        public float totalMet;     // what the body is producing, in multiples of resting
        public float productionWatts;
        public float lossWatts;
        public float netWatts;
        public float sweatWatts;   // heat shed by sweat and blood flow above the normal core temperature
        public float waterWatts;   // water cooling actually taken on the last step (after the taper)
        public float waterTaper;   // the taper factor on the last step, 0-1
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
            float perDegree = s.dissipationWattsPerC + (s.hotSweatWattsPerC - s.dissipationWattsPerC) *
                              Clamp01((c.airC - s.hotSweatFromAirC) / Math.Max(0.01f, s.hotSweatFullAirC - s.hotSweatFromAirC));
            float strokeFade = 1f - (1f - s.strokeSweatFloor) * Clamp01((coreC - s.strokeFromC) / Math.Max(0.01f, s.strokeFloorAtC - s.strokeFromC));
            float dissipation = Math.Min(Math.Max(0f, coreC - s.normalCoreC) * perDegree * Clamp01(c.sweatEfficiency) * strokeFade,
                                         s.maxSweatWatts);
            // Rewarming boost (Health_System.md, Warmth Regeneration, Mike 2026-10-10): only the body's OWN net gain is
            // multiplied, and only while below the ceiling. A net loss is never touched (so cooling and the resting
            // plateaus are unchanged, because a plateau is net zero), and a campfire's radiant watts are added after.
            float body = production - loss - dissipation;
            if (body > 0f && c.rewarmMultiplier > 1f && coreC < c.rewarmBelowC)
                body *= c.rewarmMultiplier;
            float net = body + c.radiantWatts + c.solarWatts;
            float waterTaper = Clamp01((coreC - c.waterTaperNoneC) / Math.Max(0.01f, c.waterTaperFullC - c.waterTaperNoneC));
            float waterWatts = c.waterCoolingWatts > 0f ? c.waterCoolingWatts * waterTaper : 0f;
            net -= waterWatts;

            coreC = Math.Min(s.maxCoreC, Math.Max(s.minCoreC, coreC + net / s.heatCapacityWhPerC * dt));
            r = new Result { coreC = coreC, shiver = shiver, totalMet = totalMet, productionWatts = production, lossWatts = loss, netWatts = net,
                             sweatWatts = dissipation, waterWatts = waterWatts, waterTaper = waterTaper };
        }
        return r;
    }

    static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;
}
