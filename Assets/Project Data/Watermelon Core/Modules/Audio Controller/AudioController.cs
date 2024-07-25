using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Watermelon
{
    [RequireSetting("Vibration", PrefsSettings.FieldType.Bool)]
    [RequireSetting("Volume", PrefsSettings.FieldType.Float)]
    public class AudioController
    {
        static AudioController instance;

        FieldInfo[] fields;
        const int AUDIO_SOURCES_AMOUNT = 4;

        GameObject targetGameObject;

        List<AudioSource> audioSources = new();

        List<AudioSource> activeSoundSources = new();
        List<AudioSource> activeMusicSources = new();

        List<AudioSource> customSources = new();
        List<AudioCaseCustom> activeCustomSourcesCases = new();

        static bool vibrationState;
        static float volume;

        static AudioClip[] musicAudioClips;
        public static AudioClip[] MusicAudioClips => musicAudioClips;

        static Sounds sounds;
        public static Sounds Sounds => sounds;

        static Music music;
        public static Music Music => music;

        public static OnVolumeChangedCallback OnVolumeChanged;
        public static OnVibrationChangedCallback OnVibrationChanged;

        static AudioListener audioListener;
        public static AudioListener AudioListener => audioListener;

        static List<AudioMinDelayData> minDelayQueue = new();

        public void Initialise(AudioSettings settings, GameObject targetGameObject)
        {
            if (instance != null)
            {
                Debug.Log("[Audio Controller]: Module already exists!");

                return;
            }

            if (settings == null)
            {
                Debug.LogError(
                    "[AudioController]: Audio Settings is NULL! Please assign audio settings scriptable on Audio Controller script.");

                return;
            }

            this.targetGameObject = targetGameObject;

            instance = this;
            fields = typeof(Music).GetFields();
            musicAudioClips = new AudioClip[fields.Length];

            for (var i = 0; i < fields.Length; i++)
            {
                musicAudioClips[i] = fields[i].GetValue(settings.Music) as AudioClip;
            }

            music = settings.Music;
            sounds = settings.Sounds;

            //Create audio source objects
            audioSources.Clear();
            for (var i = 0; i < AUDIO_SOURCES_AMOUNT; i++)
            {
                audioSources.Add(CreateAudioSourceObject(false));
            }

            // Load default states
            vibrationState = PrefsSettings.GetBool(PrefsSettings.Key.Vibration);
            volume = PrefsSettings.GetFloat(PrefsSettings.Key.Volume);

            Tween.InvokeCoroutine(MinDelayQueueUpdate());
        }

        public static void CreateAudioListener()
        {
            if (audioListener != null)
                return;

            // Create game object for listener
            var listenerObject = new GameObject("[AUDIO LISTENER]");
            listenerObject.transform.position = Vector3.zero;

            // Mark as non-destroyable
            Object.DontDestroyOnLoad(listenerObject);

            // Add listener component to created object
            audioListener = listenerObject.AddComponent<AudioListener>();
        }

        public static bool IsVibrationModuleEnabled()
        {
            return PrefsSettings.GetBool(PrefsSettings.Key.Vibration);
        }

        public static bool IsAudioModuleEnabled()
        {
            return (!((PrefsSettings.GetFloat(PrefsSettings.Key.Volume) - 0.00005f) < 0));
        }

        public static void PlayRandomMusic()
        {
            if (!musicAudioClips.IsNullOrEmpty())
                PlayMusic(musicAudioClips.GetRandomItem());
        }

        public static void ReleaseStreams()
        {
            ReleaseMusic();
            ReleaseSounds();
            ReleaseCustomStreams();
        }

        public static void ReleaseMusic()
        {
            var activeMusicCount = instance.activeMusicSources.Count - 1;
            for (var i = activeMusicCount; i >= 0; i--)
            {
                instance.activeMusicSources[i].Stop();
                instance.activeMusicSources[i].clip = null;
                instance.activeMusicSources.RemoveAt(i);
            }
        }


        public static void ReleaseSounds()
        {
            var activeStreamsCount = instance.activeSoundSources.Count - 1;
            for (var i = activeStreamsCount; i >= 0; i--)
            {
                instance.activeSoundSources[i].Stop();
                instance.activeSoundSources[i].clip = null;
                instance.activeSoundSources.RemoveAt(i);
            }
        }


        public static void ReleaseCustomStreams()
        {
            var activeStreamsCount = instance.activeCustomSourcesCases.Count - 1;
            for (var i = activeStreamsCount; i >= 0; i--)
            {
                if (instance.activeCustomSourcesCases[i].autoRelease)
                {
                    var source = instance.activeCustomSourcesCases[i].source;
                    instance.activeCustomSourcesCases[i].source.Stop();
                    instance.activeCustomSourcesCases[i].source.clip = null;
                    instance.activeCustomSourcesCases.RemoveAt(i);
                    instance.customSources.Add(source);
                }
            }
        }

        public static void StopStream(AudioCase audioCase, float fadeTime = 0)
        {
            if (audioCase.type == AudioType.Sound)
            {
                instance.StopSound(audioCase.source, fadeTime);
            }
            else
            {
                instance.StopMusic(audioCase.source, fadeTime);
            }
        }

        public static void StopStream(AudioCaseCustom audioCase, float fadeTime = 0)
        {
            ReleaseCustomSource(audioCase, fadeTime);
        }

        void StopSound(AudioSource source, float fadeTime = 0)
        {
            var streamID = activeSoundSources.FindIndex(x => x == source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    activeSoundSources[streamID].Stop();
                    activeSoundSources[streamID].clip = null;
                    activeSoundSources.RemoveAt(streamID);
                }
                else
                {
                    activeSoundSources[streamID].DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        activeSoundSources.Remove(source);
                        source.Stop();
                    });
                }
            }
        }

        void StopMusic(AudioSource source, float fadeTime = 0)
        {
            var streamID = activeMusicSources.FindIndex(x => x == source);
            if (streamID != -1)
            {
                if (fadeTime == 0)
                {
                    activeMusicSources[streamID].Stop();
                    activeMusicSources[streamID].clip = null;
                    activeMusicSources.RemoveAt(streamID);
                }
                else
                {
                    activeMusicSources[streamID].DOVolume(0f, fadeTime).OnComplete(() =>
                    {
                        activeMusicSources.Remove(source);
                        source.Stop();
                    });
                }
            }
        }

        static void AddMusic(AudioSource source)
        {
            if (!instance.activeMusicSources.Contains(source))
            {
                instance.activeMusicSources.Add(source);
            }
        }

        static void AddSound(AudioSource source)
        {
            if (!instance.activeSoundSources.Contains(source))
            {
                instance.activeSoundSources.Add(source);
            }
        }

        public static void PlayMusic(AudioClip clip, float volumePercentage = 1.0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            var source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Music);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.Play();

            AddMusic(source);
        }

        public static AudioCase PlaySmartMusic(AudioClip clip, float volumePercentage = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            var source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Music);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.pitch = pitch;

            var audioCase = new AudioCase(clip, source, AudioType.Music);

            audioCase.Play();

            AddMusic(source);

            return audioCase;
        }


        public static void Play(AudioClip clip, float volumePercentage = 1.0f, float pitch = 1.0f, float minDelay = 0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            if (minDelay > 0)
            {
                if (minDelayQueue.Exists(data => data.AudioHash.Equals(clip.GetHashCode())))
                {
                    return;
                }

                minDelayQueue.Add(new AudioMinDelayData(clip.GetHashCode(), minDelay));
            }

            var source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Sound);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.pitch = pitch;
            source.Play();

            AddSound(source);
        }

        public static AudioCase PlaySmartSound(AudioClip clip, float volumePercentage = 1.0f, float pitch = 1.0f)
        {
            if (clip == null)
                Debug.LogError("[AudioController]: Audio clip is null");

            var source = instance.GetAudioSource();

            SetSourceDefaultSettings(source, AudioType.Sound);

            source.clip = clip;
            source.volume *= volumePercentage;
            source.pitch = pitch;

            var audioCase = new AudioCase(clip, source, AudioType.Sound);
            audioCase.Play();

            AddSound(source);

            return audioCase;
        }

        public static AudioCaseCustom GetCustomSource(bool autoRelease, AudioType audioType)
        {
            AudioSource source = null;

            if (!instance.customSources.IsNullOrEmpty())
            {
                source = instance.customSources[0];
                instance.customSources.RemoveAt(0);
            }
            else
            {
                source = instance.CreateAudioSourceObject(true);
            }

            SetSourceDefaultSettings(source, audioType);

            var audioCase = new AudioCaseCustom(null, source, audioType, autoRelease);

            instance.activeCustomSourcesCases.Add(audioCase);

            return audioCase;
        }

        public static void ReleaseCustomSource(AudioCaseCustom audioCase, float fadeTime = 0)
        {
            var streamID = instance.activeCustomSourcesCases.FindIndex(x => x.source == audioCase.source);
            if (streamID == -1) return;

            if (fadeTime == 0)
            {
                instance.activeCustomSourcesCases[streamID].source.Stop();
                instance.activeCustomSourcesCases[streamID].source.clip = null;
                instance.activeCustomSourcesCases.RemoveAt(streamID);
                instance.customSources.Add(audioCase.source);
            }
            else
            {
                instance.activeCustomSourcesCases[streamID].source.DOVolume(0f, fadeTime).OnComplete(() =>
                {
                    instance.activeCustomSourcesCases.Remove(audioCase);
                    audioCase.source.Stop();
                    instance.customSources.Add(audioCase.source);
                });
            }
        }

        AudioSource GetAudioSource()
        {
            var sourcesAmount = audioSources.Count;
            for (var i = 0; i < sourcesAmount; i++)
            {
                if (!audioSources[i].isPlaying)
                    return audioSources[i];
            }

            var createdSource = CreateAudioSourceObject(false);
            audioSources.Add(createdSource);
            return createdSource;
        }

        AudioSource CreateAudioSourceObject(bool isCustom)
        {
            var audioSource = targetGameObject.AddComponent<AudioSource>();
            SetSourceDefaultSettings(audioSource);
            return audioSource;
        }

        void SetVolumeForAudioSources(float volume)
        {
            SetSoundsVolume(volume);
            SetMusicVolume(volume);

            foreach (var audi in activeCustomSourcesCases)
            {
                audi.source.volume = volume;
            }
        }

        public static void SetSoundsVolume(float newVolume)
        {
            foreach (var src in instance.activeSoundSources)
            {
                src.volume = newVolume;
            }
        }

        public static void SetMusicVolume(float newVolume)
        {
            foreach (var src in instance.activeMusicSources)
            {
                src.volume = newVolume;
            }
        }

        public static void SetVolume(float volume)
        {
            AudioController.volume = volume;
            PrefsSettings.SetFloat(PrefsSettings.Key.Volume, volume);
            instance.SetVolumeForAudioSources(volume);
            OnVolumeChanged?.Invoke(volume);
        }

        public static float GetVolume() => volume;

        public static bool IsVibrationEnabled() => vibrationState;

        public static void SetVibrationState(bool vibrationState)
        {
            AudioController.vibrationState = vibrationState;
            PrefsSettings.SetBool(PrefsSettings.Key.Vibration, vibrationState);
            OnVibrationChanged?.Invoke(vibrationState);
        }

        public static void SetSourceDefaultSettings(AudioSource source, AudioType type = AudioType.Sound)
        {
            var vol = PrefsSettings.GetFloat(PrefsSettings.Key.Volume);

            source.loop = type switch
            {
                AudioType.Sound => false,
                AudioType.Music => true,
                _ => source.loop
            };

            source.clip = null;
            source.volume = vol;
            source.pitch = 1.0f;
            source.spatialBlend = 0; // 2D Sound
            source.mute = false;
            source.playOnAwake = false;
            source.outputAudioMixerGroup = null;
        }

        public enum AudioType
        {
            Music = 0,
            Sound = 1
        }

        public delegate void OnVolumeChangedCallback(float volume);

        public delegate void OnVibrationChangedCallback(bool state);


        IEnumerator MinDelayQueueUpdate()
        {
            var delay = new WaitForSeconds(0.1f);

            while (true)
            {
                if (minDelayQueue.Count == 0)
                {
                    yield return delay;
                }
                else
                {
                    for (var i = 0; i < minDelayQueue.Count; i++)
                    {
                        if (!(Time.timeSinceLevelLoad >= minDelayQueue[i].EnableTime)) continue;
                        minDelayQueue.RemoveAt(i);
                        i--;
                    }

                    yield return delay;
                }
            }
        }
    }

    public struct AudioMinDelayData
    {
        public readonly int AudioHash;
        public readonly float EnableTime;

        public AudioMinDelayData(int audioHash, float delayDuration)
        {
            AudioHash = audioHash;
            EnableTime = Time.timeSinceLevelLoad + delayDuration;
        }
    }
}
 