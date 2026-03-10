using UnityEngine;
using System;
using System.Collections.Generic;

namespace Rebus.Audio
{
    /// <summary>
    /// Singleton audio manager with procedurally generated sound effects.
    /// Uses AudioClip.Create() and SetData() to produce simple waveforms
    /// so no external audio assets are required.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public enum Sound
        {
            PanelFlip,
            Match,
            Mismatch,
            Victory,
            ButtonClick,
            PuzzleReveal,
            WrongAnswer
        }

        [Header("Settings")]
        [SerializeField] private float masterVolume = 0.6f;
        [SerializeField] private bool musicEnabled = false;

        private AudioSource sfxSource;
        private AudioSource musicSource;
        private Dictionary<Sound, AudioClip> clips;

        private const int SampleRate = 44100;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.volume = 0.15f;

            GenerateAllClips();

            if (musicEnabled)
                StartBackgroundMusic();
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        /// <summary>
        /// Plays the specified sound effect.
        /// </summary>
        public void PlaySound(Sound sound)
        {
            if (clips.TryGetValue(sound, out AudioClip clip))
            {
                sfxSource.PlayOneShot(clip, masterVolume);
            }
        }

        /// <summary>
        /// Toggles background music on or off.
        /// </summary>
        public void ToggleMusic()
        {
            musicEnabled = !musicEnabled;
            if (musicEnabled)
                StartBackgroundMusic();
            else
                musicSource.Stop();
        }

        /// <summary>
        /// Sets the master volume for sound effects (0-1).
        /// </summary>
        public void SetVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
        }

        // ------------------------------------------------------------------
        // Clip Generation
        // ------------------------------------------------------------------

        private void GenerateAllClips()
        {
            clips = new Dictionary<Sound, AudioClip>
            {
                { Sound.PanelFlip, GeneratePanelFlipClip() },
                { Sound.Match, GenerateMatchClip() },
                { Sound.Mismatch, GenerateMismatchClip() },
                { Sound.Victory, GenerateVictoryClip() },
                { Sound.ButtonClick, GenerateButtonClickClip() },
                { Sound.PuzzleReveal, GeneratePuzzleRevealClip() },
                { Sound.WrongAnswer, GenerateWrongAnswerClip() },
            };
        }

        /// <summary>
        /// Short white noise burst (click).
        /// </summary>
        private AudioClip GeneratePanelFlipClip()
        {
            int samples = (int)(SampleRate * 0.05f); // 50ms
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float envelope = 1f - t; // Linear decay
                data[i] = UnityEngine.Random.Range(-1f, 1f) * envelope * 0.4f;
            }

            return CreateClip("PanelFlip", data);
        }

        /// <summary>
        /// Ascending two-tone beep.
        /// </summary>
        private AudioClip GenerateMatchClip()
        {
            float duration = 0.3f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];
            int half = samples / 2;

            float freq1 = 523.25f; // C5
            float freq2 = 659.25f; // E5

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float freq = i < half ? freq1 : freq2;
                float envelope = 1f - ((float)i / samples) * 0.5f;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.5f;
            }

            return CreateClip("Match", data);
        }

        /// <summary>
        /// Descending buzz for mismatch.
        /// </summary>
        private AudioClip GenerateMismatchClip()
        {
            float duration = 0.35f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float freqStart = 300f;
            float freqEnd = 150f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(freqStart, freqEnd, t);
                float time = (float)i / SampleRate;
                float envelope = 1f - t;
                // Square-ish wave for buzz quality
                float wave = Mathf.Sin(2f * Mathf.PI * freq * time);
                wave = wave > 0 ? 0.5f : -0.5f;
                data[i] = wave * envelope * 0.35f;
            }

            return CreateClip("Mismatch", data);
        }

        /// <summary>
        /// Ascending arpeggio: C5 E5 G5 C6.
        /// </summary>
        private AudioClip GenerateVictoryClip()
        {
            float noteDuration = 0.2f;
            float[] freqs = { 523.25f, 659.25f, 783.99f, 1046.5f }; // C5 E5 G5 C6
            int samplesPerNote = (int)(SampleRate * noteDuration);
            int totalSamples = samplesPerNote * freqs.Length + (int)(SampleRate * 0.5f); // extra sustain
            float[] data = new float[totalSamples];

            for (int n = 0; n < freqs.Length; n++)
            {
                int offset = n * samplesPerNote;
                for (int i = 0; i < samplesPerNote; i++)
                {
                    float t = (float)i / SampleRate;
                    float envelope = 1f - (float)i / samplesPerNote * 0.3f;
                    int idx = offset + i;
                    if (idx < totalSamples)
                        data[idx] += Mathf.Sin(2f * Mathf.PI * freqs[n] * t) * envelope * 0.4f;
                }
            }

            // Sustain final note
            float lastFreq = freqs[freqs.Length - 1];
            int sustainStart = freqs.Length * samplesPerNote;
            int sustainSamples = totalSamples - sustainStart;
            for (int i = 0; i < sustainSamples; i++)
            {
                float t = (float)(sustainStart + i) / SampleRate;
                float envelope = 1f - (float)i / sustainSamples;
                int idx = sustainStart + i;
                if (idx < totalSamples)
                    data[idx] += Mathf.Sin(2f * Mathf.PI * lastFreq * t) * envelope * 0.35f;
            }

            return CreateClip("Victory", data);
        }

        /// <summary>
        /// Soft click for buttons.
        /// </summary>
        private AudioClip GenerateButtonClickClip()
        {
            int samples = (int)(SampleRate * 0.03f); // 30ms
            float[] data = new float[samples];

            float freq = 1000f;
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = 1f - (float)i / samples;
                envelope *= envelope; // Quadratic decay
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * envelope * 0.3f;
            }

            return CreateClip("ButtonClick", data);
        }

        /// <summary>
        /// Rising shimmer for puzzle reveal.
        /// </summary>
        private AudioClip GeneratePuzzleRevealClip()
        {
            float duration = 0.6f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float freqStart = 400f;
            float freqEnd = 1200f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(freqStart, freqEnd, t * t); // Accelerating rise
                float time = (float)i / SampleRate;
                // Bell-shaped envelope
                float envelope = Mathf.Sin(Mathf.PI * t) * 0.8f;
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * time) * envelope * 0.35f;
                // Add harmonic
                data[i] += Mathf.Sin(2f * Mathf.PI * freq * 2f * time) * envelope * 0.1f;
            }

            return CreateClip("PuzzleReveal", data);
        }

        /// <summary>
        /// Low buzz for wrong answer.
        /// </summary>
        private AudioClip GenerateWrongAnswerClip()
        {
            float duration = 0.5f;
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            float freq = 120f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float time = (float)i / SampleRate;
                float envelope = 1f - t;
                // Harsh buzz with harmonics
                float wave = Mathf.Sin(2f * Mathf.PI * freq * time) * 0.4f;
                wave += Mathf.Sin(2f * Mathf.PI * freq * 3f * time) * 0.2f;
                wave += Mathf.Sin(2f * Mathf.PI * freq * 5f * time) * 0.1f;
                data[i] = wave * envelope * 0.4f;
            }

            return CreateClip("WrongAnswer", data);
        }

        // ------------------------------------------------------------------
        // Background Music
        // ------------------------------------------------------------------

        private void StartBackgroundMusic()
        {
            AudioClip musicClip = GenerateAmbientTone();
            musicSource.clip = musicClip;
            musicSource.Play();
        }

        /// <summary>
        /// Generates a gentle looping ambient tone using layered sine waves.
        /// </summary>
        private AudioClip GenerateAmbientTone()
        {
            float duration = 8f; // 8-second loop
            int samples = (int)(SampleRate * duration);
            float[] data = new float[samples];

            // Soft pad: layered fifths with slow modulation
            float[] baseFreqs = { 130.81f, 196f, 261.63f }; // C3, G3, C4

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float loopT = (float)i / samples;

                // Slow LFO for gentle movement
                float lfo = 1f + 0.1f * Mathf.Sin(2f * Mathf.PI * 0.25f * t);

                float sample = 0f;
                for (int f = 0; f < baseFreqs.Length; f++)
                {
                    float freq = baseFreqs[f] * lfo;
                    sample += Mathf.Sin(2f * Mathf.PI * freq * t) * 0.12f;
                }

                // Smooth loop with fade at edges
                float fadeIn = Mathf.Clamp01(loopT * 10f);
                float fadeOut = Mathf.Clamp01((1f - loopT) * 10f);
                data[i] = sample * fadeIn * fadeOut;
            }

            AudioClip clip = AudioClip.Create("AmbientMusic", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private AudioClip CreateClip(string name, float[] data)
        {
            AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
