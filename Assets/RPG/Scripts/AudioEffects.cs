using UnityEngine;

namespace Riftbound
{
    public class AudioEffects : MonoBehaviour
    {
        public static AudioEffects Instance { get; private set; }

        AudioSource source;

        AudioClip sfxLevelUp;
        AudioClip sfxQuestComplete;
        AudioClip sfxQuestAccept;
        AudioClip sfxLootCoin;
        AudioClip sfxBladeHit;
        AudioClip sfxSpellCast;
        AudioClip sfxClick;
        AudioClip sfxZoneDiscover;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(this); else DestroyImmediate(this);
                return;
            }
            Instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D UI sound

            GenerateProceduralAudioClips();
        }

        void GenerateProceduralAudioClips()
        {
            sfxClick = CreateToneClip("Click", 0.05f, 900f, 0.25f, ToneType.Sine);
            sfxBladeHit = CreateNoiseHitClip("BladeHit", 0.15f, 0.4f);
            sfxSpellCast = CreateChirpClip("SpellCast", 0.28f, 260f, 680f, 0.35f);
            sfxLootCoin = CreateArpeggioClip("LootCoin", 0.25f, new float[] { 880f, 1320f, 1760f }, 0.35f);
            sfxQuestAccept = CreateArpeggioClip("QuestAccept", 0.45f, new float[] { 440f, 554.37f, 659.25f }, 0.4f);
            sfxQuestComplete = CreateArpeggioClip("QuestComplete", 0.7f, new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.5f);
            sfxLevelUp = CreateFanfareClip("LevelUp", 1.1f);
            sfxZoneDiscover = CreateArpeggioClip("ZoneDiscover", 0.85f, new float[] { 392f, 493.88f, 587.33f, 783.99f }, 0.45f);
        }

        enum ToneType { Sine, Square, Sawtooth }

        AudioClip CreateToneClip(string clipName, float duration, float freq, float volume, ToneType type)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = 1f - (float)i / samples;
                float phase = t * freq * Mathf.PI * 2f;
                float val = Mathf.Sin(phase);
                if (type == ToneType.Square) val = val >= 0 ? 0.7f : -0.7f;
                data[i] = val * envelope * volume;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip CreateNoiseHitClip(string clipName, float duration, float volume)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            var rng = new System.Random(42);

            for (int i = 0; i < samples; i++)
            {
                float envelope = Mathf.Pow(1f - (float)i / samples, 2.5f);
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.7f;
                float lowFreq = Mathf.Sin(i * 120f * Mathf.PI * 2f / sampleRate) * 0.5f;
                data[i] = (noise + lowFreq) * envelope * volume;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip CreateChirpClip(string clipName, float duration, float startFreq, float endFreq, float volume)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float frac = (float)i / samples;
                float envelope = Mathf.Sin(frac * Mathf.PI);
                float currentFreq = Mathf.Lerp(startFreq, endFreq, frac);
                float phase = (float)i / sampleRate * currentFreq * Mathf.PI * 2f;
                data[i] = Mathf.Sin(phase) * envelope * volume;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip CreateArpeggioClip(string clipName, float duration, float[] freqs, float volume)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            int noteSamples = samples / freqs.Length;

            for (int i = 0; i < samples; i++)
            {
                int noteIndex = Mathf.Clamp(i / noteSamples, 0, freqs.Length - 1);
                int sampleInNote = i % noteSamples;
                float noteFrac = (float)sampleInNote / noteSamples;
                float envelope = 1f - noteFrac;
                float freq = freqs[noteIndex];
                float phase = (float)i / sampleRate * freq * Mathf.PI * 2f;
                data[i] = (Mathf.Sin(phase) + 0.3f * Mathf.Sin(phase * 2f)) * envelope * volume;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip CreateFanfareClip(string clipName, float duration)
        {
            int sampleRate = 44100;
            int samples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[samples];
            // C4, E4, G4, C5 arpeggio with triumphal chord
            float[] notes = new float[] { 261.63f, 329.63f, 392.00f, 523.25f, 659.25f };
            int noteStep = samples / (notes.Length + 1);

            for (int i = 0; i < samples; i++)
            {
                float masterEnv = 1f - (float)i / samples;
                int currentNote = Mathf.Clamp(i / noteStep, 0, notes.Length - 1);
                float freq = notes[currentNote];
                float phase = (float)i / sampleRate * freq * Mathf.PI * 2f;
                // Add rich harmonics
                float tone = Mathf.Sin(phase) * 0.5f + Mathf.Sin(phase * 2f) * 0.3f + Mathf.Sin(phase * 3f) * 0.15f;
                data[i] = tone * masterEnv * 0.5f;
            }

            var clip = AudioClip.Create(clipName, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public static void PlayClick() { if (Instance && Instance.sfxClick) Instance.source.PlayOneShot(Instance.sfxClick, 0.3f); }
        public static void PlayBladeHit() { if (Instance && Instance.sfxBladeHit) Instance.source.PlayOneShot(Instance.sfxBladeHit, 0.45f); }
        public static void PlaySpellCast() { if (Instance && Instance.sfxSpellCast) Instance.source.PlayOneShot(Instance.sfxSpellCast, 0.45f); }
        public static void PlayLootCoin() { if (Instance && Instance.sfxLootCoin) Instance.source.PlayOneShot(Instance.sfxLootCoin, 0.5f); }
        public static void PlayQuestAccept() { if (Instance && Instance.sfxQuestAccept) Instance.source.PlayOneShot(Instance.sfxQuestAccept, 0.6f); }
        public static void PlayQuestComplete() { if (Instance && Instance.sfxQuestComplete) Instance.source.PlayOneShot(Instance.sfxQuestComplete, 0.65f); }
        public static void PlayLevelUp() { if (Instance && Instance.sfxLevelUp) Instance.source.PlayOneShot(Instance.sfxLevelUp, 0.75f); }
        public static void PlayBossVictory() { if (Instance && Instance.sfxLevelUp) Instance.source.PlayOneShot(Instance.sfxLevelUp, 0.95f); }
        public static void PlayZoneDiscover() { if (Instance && Instance.sfxZoneDiscover) Instance.source.PlayOneShot(Instance.sfxZoneDiscover, 0.55f); }
    }
}
