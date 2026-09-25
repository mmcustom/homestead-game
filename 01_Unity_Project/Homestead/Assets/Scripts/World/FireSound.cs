using UnityEngine;

// Fallback campfire crackle, synthesised in code: a soft low rumble of burning with random pops and snaps over it.
// Loops seamlessly. The Campfire prefab now carries the real recording (Audio/Ambient/Campfire.wav, Audio_System.md's
// ambient_campfire_crackle); Campfire only falls back to this if its AudioSource has no clip.
public static class FireSound
{
    const int SampleRate = 22050;
    const float Seconds = 6f;

    static AudioClip crackle;

    public static AudioClip Crackle => crackle != null ? crackle : crackle = Build();

    static AudioClip Build()
    {
        int length = (int)(SampleRate * Seconds);
        var samples = new float[length];
        var random = new System.Random(1234); // fixed, so the sound is the same every run

        // Low rumble: heavily smoothed noise.
        float low = 0f, lower = 0f;
        for (int i = 0; i < length; i++)
        {
            float white = (float)(random.NextDouble() * 2.0 - 1.0);
            low += (white - low) * 0.02f;
            lower += (low - lower) * 0.05f;
            samples[i] = lower * 1.6f;
        }

        // Crackles: short, sharp noise bursts at random, some louder snaps among many small ticks.
        int pops = (int)(Seconds * 14f);
        for (int p = 0; p < pops; p++)
        {
            int start = random.Next(length);
            bool snap = random.NextDouble() < 0.12;
            float amplitude = snap ? 0.55f + (float)random.NextDouble() * 0.35f : 0.08f + (float)random.NextDouble() * 0.2f;
            float decay = snap ? 0.9965f : 0.992f;
            int burst = snap ? 900 : 350;
            float envelope = amplitude, filtered = 0f;
            for (int k = 0; k < burst; k++)
            {
                int i = (start + k) % length; // wraps, so pops near the end continue at the start and the loop is seamless
                float white = (float)(random.NextDouble() * 2.0 - 1.0);
                filtered += (white - filtered) * (snap ? 0.6f : 0.8f);
                samples[i] += filtered * envelope;
                envelope *= decay;
            }
        }

        // Crossfade the rumble's ends so the loop point doesn't click.
        int fade = SampleRate / 10;
        for (int i = 0; i < fade; i++)
        {
            float t = i / (float)fade;
            float a = samples[i], b = samples[length - fade + i];
            samples[i] = a * t + b * (1f - t);
        }
        System.Array.Resize(ref samples, length - fade);

        float peak = 0f;
        foreach (float s in samples)
            peak = Mathf.Max(peak, Mathf.Abs(s));
        if (peak > 0f)
        {
            for (int i = 0; i < samples.Length; i++)
                samples[i] *= 0.9f / peak;
        }

        AudioClip clip = AudioClip.Create("Campfire Crackle (placeholder)", samples.Length, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
