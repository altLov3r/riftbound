using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Riftbound
{
    public class ZoneAudioAmbience : MonoBehaviour
    {
        public static ZoneAudioAmbience Instance { get; private set; }

        public enum AmbienceMood { AmberVale, Bloodrock, Duskwood, Wyrmcrest, DungeonCrypts }

        AudioSource musicSourceA;
        AudioSource musicSourceB;
        AudioSource combatPercussionSource;

        AudioClip clipAmberVale;
        AudioClip clipBloodrock;
        AudioClip clipDuskwood;
        AudioClip clipWyrmcrest;
        AudioClip clipDungeonCrypts;
        AudioClip clipCombatRhythm;

        AmbienceMood currentMood = (AmbienceMood)(-1);
        AudioSource activeSource;
        AudioSource inactiveSource;

        PlayerController player;
        float masterVolume = 0.35f;
        bool crossfading = false;
        float targetCombatVolume = 0f;

        public static float MasterMusicVolume
        {
            get => Instance ? Instance.masterVolume : 0.35f;
            set
            {
                if (Instance)
                {
                    Instance.masterVolume = Mathf.Clamp01(value);
                    if (Instance.activeSource) Instance.activeSource.volume = Instance.masterVolume;
                }
            }
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Application.isPlaying) Destroy(this); else DestroyImmediate(this);
                return;
            }
            Instance = this;

            musicSourceA = gameObject.AddComponent<AudioSource>();
            musicSourceA.loop = true;
            musicSourceA.playOnAwake = false;
            musicSourceA.spatialBlend = 0f;

            musicSourceB = gameObject.AddComponent<AudioSource>();
            musicSourceB.loop = true;
            musicSourceB.playOnAwake = false;
            musicSourceB.spatialBlend = 0f;

            combatPercussionSource = gameObject.AddComponent<AudioSource>();
            combatPercussionSource.loop = true;
            combatPercussionSource.playOnAwake = false;
            combatPercussionSource.spatialBlend = 0f;
            combatPercussionSource.volume = 0f;

            activeSource = musicSourceA;
            inactiveSource = musicSourceB;

            GenerateAtmosphericMusicClips();
        }

        void Start()
        {
            player = GetComponent<PlayerController>();
            if (!player) player = FindFirstObjectByType<PlayerController>();

            if (combatPercussionSource && clipCombatRhythm)
            {
                combatPercussionSource.clip = clipCombatRhythm;
                combatPercussionSource.Play();
            }
        }

        void Update()
        {
            if (FrontendMenu.IsOpen)
            {
                if (activeSource.isPlaying) activeSource.Pause();
                if (combatPercussionSource.isPlaying) combatPercussionSource.Pause();
                return;
            }

            if (!activeSource.isPlaying && activeSource.clip != null) activeSource.UnPause();
            if (!combatPercussionSource.isPlaying && combatPercussionSource.clip != null) combatPercussionSource.UnPause();

            if (!player) player = FindFirstObjectByType<PlayerController>();
            if (!player) return;

            // Determine current mood from zone
            AmbienceMood mood = GetMoodForPosition(player.transform.position);
            if (mood != currentMood)
            {
                currentMood = mood;
                AudioClip nextClip = GetClipForMood(mood);
                if (nextClip != null)
                {
                    StartCoroutine(CrossfadeToClip(nextClip, 2.5f));
                }
            }

            // Combat tension percussion layer
            bool inCombat = player.InCombat();
            targetCombatVolume = inCombat ? (masterVolume * 0.75f) : 0f;
            combatPercussionSource.volume = Mathf.MoveTowards(combatPercussionSource.volume, targetCombatVolume, Time.deltaTime * 0.8f);
        }

        AmbienceMood GetMoodForPosition(Vector3 pos)
        {
            if (pos.y < -20f) return AmbienceMood.DungeonCrypts;
            if (pos.y > 38f || (pos.z > 210f && Mathf.Abs(pos.x) < 25f && pos.y > 22f)) return AmbienceMood.Wyrmcrest;
            if (pos.x > 35f && pos.z > 170f) return AmbienceMood.Bloodrock;
            if (pos.x < -35f || pos.z > 130f) return AmbienceMood.Duskwood;
            return AmbienceMood.AmberVale;
        }

        AudioClip GetClipForMood(AmbienceMood mood)
        {
            switch (mood)
            {
                case AmbienceMood.AmberVale: return clipAmberVale;
                case AmbienceMood.Bloodrock: return clipBloodrock;
                case AmbienceMood.Duskwood: return clipDuskwood;
                case AmbienceMood.Wyrmcrest: return clipWyrmcrest;
                case AmbienceMood.DungeonCrypts: return clipDungeonCrypts;
                default: return clipAmberVale;
            }
        }

        IEnumerator CrossfadeToClip(AudioClip newClip, float duration)
        {
            if (crossfading) yield break;
            crossfading = true;

            inactiveSource.clip = newClip;
            inactiveSource.volume = 0f;
            inactiveSource.Play();

            float elapsed = 0f;
            float startVol = activeSource.volume;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                activeSource.volume = Mathf.Lerp(startVol, 0f, t);
                inactiveSource.volume = Mathf.Lerp(0f, masterVolume, t);
                yield return null;
            }

            activeSource.Stop();
            activeSource.volume = 0f;

            // Swap active / inactive references
            var temp = activeSource;
            activeSource = inactiveSource;
            inactiveSource = temp;

            crossfading = false;
        }

        void GenerateAtmosphericMusicClips()
        {
            const int sampleRate = 22050;

            // 1. Amber Vale: Peaceful pastoral acoustic chord loop (8.0 seconds, C major / Am / F / G)
            clipAmberVale = GeneratePastoralLoop("Music_AmberVale", sampleRate, 8.0f);

            // 2. Camp Bloodrock: Low tribal war drone & heavy percussive heartbeat (6.0 seconds)
            clipBloodrock = GenerateTribalLoop("Music_Bloodrock", sampleRate, 6.0f);

            // 3. Duskwood: Eerie gothic minor choir & resonant hollow bells (8.0 seconds, D minor)
            clipDuskwood = GenerateGothicLoop("Music_Duskwood", sampleRate, 8.0f);

            // 4. Wyrmcrest Summit: Howling high wind & heroic brass crescendos (8.0 seconds)
            clipWyrmcrest = GenerateSummitLoop("Music_Wyrmcrest", sampleRate, 8.0f);

            // 5. Dungeon Crypts: Subterranean deep reverb stone echo (7.0 seconds)
            clipDungeonCrypts = GenerateCryptLoop("Music_DungeonCrypts", sampleRate, 7.0f);

            // 6. Combat Tension Rhythm: Fast 130 BPM marching battle drive (4.0 seconds)
            clipCombatRhythm = GenerateCombatLoop("Music_CombatRhythm", sampleRate, 4.0f);
        }

        AudioClip GeneratePastoralLoop(string name, int sampleRate, float duration)
        {
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            // 4 chords: C maj (261.6, 329.6, 392.0), Am (220.0, 261.6, 329.6), F maj (174.6, 220.0, 261.6), G maj (196.0, 246.9, 293.7)
            float[][] chords = {
                new float[] { 261.63f, 329.63f, 392.00f, 523.25f },
                new float[] { 220.00f, 261.63f, 329.63f, 440.00f },
                new float[] { 174.61f, 220.00f, 261.63f, 349.23f },
                new float[] { 196.00f, 246.94f, 293.66f, 392.00f }
            };

            int chordLen = totalSamples / chords.Length;

            for (int i = 0; i < totalSamples; i++)
            {
                int chordIdx = Mathf.Clamp(i / chordLen, 0, chords.Length - 1);
                float[] notes = chords[chordIdx];
                int inChord = i % chordLen;
                float chordFrac = (float)inChord / chordLen;

                // Soft bell-like pluck envelope
                float env = Mathf.Sin(chordFrac * Mathf.PI);

                float sample = 0f;
                for (int n = 0; n < notes.Length; n++)
                {
                    float phase = (float)i / sampleRate * notes[n] * Mathf.PI * 2f;
                    sample += Mathf.Sin(phase) * 0.22f + Mathf.Sin(phase * 2f) * 0.08f;
                }

                // Smooth loop boundary fade
                float loopFade = Mathf.Sin((float)i / totalSamples * Mathf.PI);
                data[i] = sample * env * loopFade * 0.45f;
            }

            var clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateTribalLoop(string name, int sampleRate, float duration)
        {
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[totalSamples];
            var rng = new System.Random(1337);

            // Heavy kick beat every 1.0 second (60 BPM)
            int beatInterval = sampleRate;

            for (int i = 0; i < totalSamples; i++)
            {
                int beatPos = i % beatInterval;
                float beatFrac = (float)beatPos / beatInterval;
                float drumEnv = Mathf.Pow(Mathf.Clamp01(1f - beatFrac * 3.5f), 3f);

                // Low bass kick pitch drop 110Hz -> 45Hz
                float kickFreq = Mathf.Lerp(110f, 45f, beatFrac);
                float kickPhase = (float)beatPos / sampleRate * kickFreq * Mathf.PI * 2f;
                float kick = Mathf.Sin(kickPhase) * drumEnv * 0.55f;

                // Desert wind noise
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.12f;
                float windPhase = (float)i / sampleRate * 95f * Mathf.PI * 2f;
                float wind = Mathf.Sin(windPhase) * 0.15f + noise;

                float loopFade = Mathf.Sin((float)i / totalSamples * Mathf.PI);
                data[i] = (kick + wind) * loopFade * 0.45f;
            }

            var clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateGothicLoop(string name, int sampleRate, float duration)
        {
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            // D minor pad: D3 (146.8), F3 (174.6), A3 (220.0), C#4 (277.2 - harmonic minor)
            float[] dMinor = { 146.83f, 174.61f, 220.00f, 277.18f };

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;
                float choir = 0f;

                for (int n = 0; n < dMinor.Length; n++)
                {
                    // Gentle vibrato (5Hz, 1.5Hz depth)
                    float vibrato = Mathf.Sin(t * 5f * Mathf.PI * 2f) * 1.5f;
                    float freq = dMinor[n] + vibrato;
                    float phase = t * freq * Mathf.PI * 2f;
                    choir += (Mathf.Sin(phase) + Mathf.Sin(phase * 3f) * 0.25f) * 0.20f;
                }

                // Church bell toll at start of 8s loop
                float bellFrac = (float)i / totalSamples;
                float bellEnv = Mathf.Pow(Mathf.Clamp01(1f - bellFrac * 1.8f), 4f);
                float bell = Mathf.Sin(t * 440f * Mathf.PI * 2f) * bellEnv * 0.35f;

                float loopFade = Mathf.Sin(bellFrac * Mathf.PI);
                data[i] = (choir + bell) * loopFade * 0.45f;
            }

            var clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateSummitLoop(string name, int sampleRate, float duration)
        {
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[totalSamples];
            var rng = new System.Random(999);

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // High altitude howling gale
                float windMod = Mathf.Sin(t * 0.8f * Mathf.PI * 2f) * 0.5f + 0.5f;
                float noise = ((float)rng.NextDouble() * 2f - 1f) * 0.22f * windMod;

                // Soaring French horn chords: Bb3 (233.08), F4 (349.23), Bb4 (466.16)
                float h1 = Mathf.Sin(t * 233.08f * Mathf.PI * 2f) * 0.25f;
                float h2 = Mathf.Sin(t * 349.23f * Mathf.PI * 2f) * 0.20f;
                float h3 = Mathf.Sin(t * 466.16f * Mathf.PI * 2f) * 0.15f;

                float loopFade = Mathf.Sin((float)i / totalSamples * Mathf.PI);
                data[i] = (noise + h1 + h2 + h3) * loopFade * 0.40f;
            }

            var clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateCryptLoop(string name, int sampleRate, float duration)
        {
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            for (int i = 0; i < totalSamples; i++)
            {
                float t = (float)i / sampleRate;

                // Subterranean sub-bass rumble (38Hz and 55Hz)
                float sub = Mathf.Sin(t * 38f * Mathf.PI * 2f) * 0.35f + Mathf.Sin(t * 55f * Mathf.PI * 2f) * 0.25f;

                // Water droplet ping every 2.3 seconds
                float dropTime = t % 2.3f;
                float dropEnv = Mathf.Pow(Mathf.Clamp01(1f - dropTime * 12f), 3f);
                float drop = Mathf.Sin(dropTime * 1450f * Mathf.PI * 2f) * dropEnv * 0.30f;

                float loopFade = Mathf.Sin((float)i / totalSamples * Mathf.PI);
                data[i] = (sub + drop) * loopFade * 0.40f;
            }

            var clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        AudioClip GenerateCombatLoop(string name, int sampleRate, float duration)
        {
            int totalSamples = Mathf.CeilToInt(sampleRate * duration);
            float[] data = new float[totalSamples];

            // 130 BPM = ~0.46s per beat
            float beatDuration = 60f / 130f;
            int beatSamples = Mathf.CeilToInt(sampleRate * beatDuration);

            for (int i = 0; i < totalSamples; i++)
            {
                int beatIdx = i / beatSamples;
                int inBeat = i % beatSamples;
                float frac = (float)inBeat / beatSamples;

                // Sharp percussion transient
                float punch = Mathf.Pow(1f - frac, 4f);
                float tone = Mathf.Sin((float)inBeat / sampleRate * 120f * Mathf.PI * 2f);

                // Accent on beats 1 and 3
                float accent = (beatIdx % 2 == 0) ? 1.3f : 0.8f;

                float loopFade = Mathf.Sin((float)i / totalSamples * Mathf.PI);
                data[i] = punch * tone * accent * loopFade * 0.45f;
            }

            var clip = AudioClip.Create(name, totalSamples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
