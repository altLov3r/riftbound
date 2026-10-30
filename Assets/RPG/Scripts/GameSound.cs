using UnityEngine;

namespace Riftbound
{
    public enum SoundCue { Hit, Heal, Rune, Guard }
    public static class GameSound
    {
        static readonly AudioClip[] Clips = new AudioClip[4];
        public static void Play(SoundCue cue, Vector3 position)
        {
            if (!Application.isPlaying || FrontendMenu.IsOpen) return;
            int index = (int)cue;
            if (!Clips[index])
            {
                const int rate = 22050; float duration = cue == SoundCue.Rune ? .45f : cue == SoundCue.Heal ? .3f : .11f;
                var samples = new float[Mathf.CeilToInt(rate * duration)]; var random = new System.Random(81 + index);
                float filtered = 0;
                for (int i = 0; i < samples.Length; i++)
                {
                    float t = i / (float)rate, envelope = Mathf.Pow(1 - t / duration, 2) * Mathf.Min(1, t * 250);
                    filtered = Mathf.Lerp(filtered, (float)random.NextDouble() * 2 - 1, .18f);
                    float tone = Mathf.Sin(2 * Mathf.PI * (cue == SoundCue.Heal ? 660 + t * 700 : cue == SoundCue.Rune ? 440 : 180) * t);
                    samples[i] = envelope * (cue == SoundCue.Hit ? filtered : tone * .3f + filtered * .12f) * .35f;
                }
                Clips[index] = AudioClip.Create("Riftbound " + cue, samples.Length, 1, rate, false); Clips[index].SetData(samples, 0);
            }
            var go = new GameObject("Short game sound"); go.transform.position = position;
            var source = go.AddComponent<AudioSource>(); source.clip = Clips[index]; source.spatialBlend = .4f; source.minDistance = 4; source.maxDistance = 22; source.volume = .6f; source.Play();
            Object.Destroy(go, Clips[index].length + .2f);
        }
    }
}
