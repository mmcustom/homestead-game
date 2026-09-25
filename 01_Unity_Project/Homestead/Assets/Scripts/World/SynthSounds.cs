using UnityEngine;

// Placeholder one-shots for Phase 2's tools, synthesised in code because no recordings have been sourced yet: a line
// cast splash, a bobber bite plop, a bow release and a rifle shot. Played through AudioManager.PlayAt, so they follow
// the SFX volume. Each should be replaced by a sourced clip — Audio_System.md is where those get mapped.
public static class SynthSounds
{
    const int Rate = 22050;

    static Sound splash, plop, bow, gunshot;

    public static Sound Splash => splash ??= Make("Splash (placeholder)", 0.5f, 0.35f, BuildSplash);
    public static Sound Plop => plop ??= Make("Bite plop (placeholder)", 0.18f, 0.5f, BuildPlop);
    public static Sound BowRelease => bow ??= Make("Bow release (placeholder)", 0.35f, 0.6f, BuildBow);
    public static Sound Gunshot => gunshot ??= Make("Rifle shot (placeholder)", 1.6f, 1f, BuildGunshot);

    static Sound Make(string name, float seconds, float volume, System.Action<float[]> fill)
    {
        var samples = new float[(int)(Rate * seconds)];
        fill(samples);
        Normalise(samples);
        AudioClip clip = AudioClip.Create(name, samples.Length, 1, Rate, false);
        clip.SetData(samples, 0);
        return new Sound { clip = clip, volume = volume };
    }

    static void BuildSplash(float[] s)
    {
        var r = new System.Random(11);
        float lp = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float env = Mathf.Exp(-t * 9f) * Mathf.Min(1f, t * 200f);
            lp += ((float)(r.NextDouble() * 2 - 1) - lp) * 0.35f;
            s[i] = lp * env;
        }
    }

    static void BuildPlop(float[] s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float freq = Mathf.Lerp(700f, 180f, t / 0.18f); // falling pitch, like a drop
            s[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * Mathf.Exp(-t * 28f);
        }
    }

    static void BuildBow(float[] s)
    {
        var r = new System.Random(5);
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float twang = Mathf.Sin(2f * Mathf.PI * 110f * t) * Mathf.Exp(-t * 14f);
            float whoosh = (float)(r.NextDouble() * 2 - 1) * Mathf.Exp(-t * 20f) * 0.35f;
            s[i] = twang + whoosh;
        }
    }

    static void BuildGunshot(float[] s)
    {
        var r = new System.Random(3);
        float lp = 0f, low = 0f;
        for (int i = 0; i < s.Length; i++)
        {
            float t = i / (float)Rate;
            float white = (float)(r.NextDouble() * 2 - 1);
            lp += (white - lp) * 0.5f;
            low += (white - low) * 0.02f;
            float crack = lp * Mathf.Exp(-t * 60f);
            float boom = low * 6f * Mathf.Exp(-t * 3.5f); // rolling echo off the woods
            s[i] = crack + boom;
        }
    }

    static void Normalise(float[] s)
    {
        float peak = 0f;
        foreach (float v in s)
            peak = Mathf.Max(peak, Mathf.Abs(v));
        if (peak > 0f)
        {
            for (int i = 0; i < s.Length; i++)
                s[i] *= 0.9f / peak;
        }
    }
}
