using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Unity.Entities.Racing.Gameplay
{
    /// <summary>
    /// Creates audio source and handles volume and position 
    /// </summary>
    public class PlayerAudioManager : MonoBehaviour
    {
        public static PlayerAudioManager Instance;
        public float InitialAudioSourceVolume;
        public AnimationCurve VolumeCurve;
        public AnimationCurve PitchCurve;
        public GameObject ReverbZonesPrefab;
        public GameObject AudioSourcePrefab;
        public AudioClip StartEngine;
        public AudioClip LoopEngine;
        public AudioClip TopSpeed;

        public AudioClip Lobby;
        public AudioClip Click;
        public AudioClip Race;
        public AudioClip Celebration;

        public AudioMixerGroup MusicChannel;
        public AudioMixerGroup UIChannel;
        public AudioMixer AudioMixer;
        private readonly Dictionary<Entity, AudioReference> m_Collection = new();
        private Scene m_AdditiveScene;
        private AudioSource MusicAudioSource;
        private AudioSource UIAudioSource;

        private float m_PreviousVolume = 1f;
        private GameObject m_ReverbZones;

        public bool IsMute 
        {
            get
            {
                AudioMixer.GetFloat("Volume", out var value);
                return value < -180f;
            }
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
#if UNITY_EDITOR
                if (!EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    m_AdditiveScene = SceneManager.GetSceneByName("AdditiveAudioSourceScene");
                }
                else
#endif
                {
                    m_AdditiveScene = SceneManager.CreateScene("AdditiveAudioSourceScenePlayMode");
                }

                m_ReverbZones = Instantiate(ReverbZonesPrefab);
                SceneManager.MoveGameObjectToScene(m_ReverbZones, m_AdditiveScene);
            }
        }

        public void ToggleVolume()
        {
            if (IsMute)
            {
                m_PreviousVolume = 1f;
                m_ReverbZones.SetActive(true);
            }
            else
            {
                m_PreviousVolume = 0.0001f;
                m_ReverbZones.SetActive(false);
            }

            AudioMixer.SetFloat("Volume", math.log(m_PreviousVolume) * 20f);
        }

        public void AddAudioSource(Entity player, bool is2D)
        {
            if (m_Collection.ContainsKey(player))
            {
                DeleteAudioSource(player);
            }

            var instance = Instantiate(AudioSourcePrefab);
            var audioSource = InstantiateAudioSource(instance, StartEngine, is2D);
            instance.name = $"{(is2D ? "2D" : "3D")} AudioSource - Entity : [{player.Index}]";
            
            var data = new AudioReference
            {
                GameObject = instance,
                AudioSource = audioSource,
                StartEngine = audioSource.clip
            };

            m_Collection.Add(player, data);
            SceneManager.MoveGameObjectToScene(instance, m_AdditiveScene);
            StartCoroutine(InitEngine(audioSource));
        }

        public void UpdatePosition(Entity player, float3 position, EntityManager manager)
        {
            if (!math.isfinite(position).x)
            {
                return;
            }

            if (m_Collection.ContainsKey(player))
            {
                m_Collection[player].GameObject.transform.position = position;
            }


            var list = new List<Entity>();
            foreach (var entity in m_Collection.Keys)
            {
                if (!manager.Exists(entity))
                {
                    list.Add(entity);
                }
            }

            foreach (var entity in list)
            {
                DeleteAudioSource(entity);
            }
        }

        public void DeleteAudioSource(Entity player)
        {
            if (m_Collection.ContainsKey(player)) 
            {
                Destroy(m_Collection[player].GameObject);
                m_Collection.Remove(player);
            }
        }

        public void UpdatePitchAndVolume(Entity player, float velocity)
        {
            if (m_Collection.ContainsKey(player) &&
                m_Collection[player].AudioSource.clip != m_Collection[player].StartEngine)
            {
                if (velocity >= 1f && m_Collection[player].AudioSource.clip != TopSpeed)
                {
                    ChangeAudio(m_Collection[player].AudioSource, TopSpeed);
                }

                else if (velocity < 1f && m_Collection[player].AudioSource.clip != LoopEngine)
                {
                    ChangeAudio(m_Collection[player].AudioSource, LoopEngine);
                }

                m_Collection[player].AudioSource.pitch = PitchCurve.Evaluate(velocity);
                m_Collection[player].AudioSource.volume = VolumeCurve.Evaluate(velocity);
            }
        }

        public void CreateAndPlayMusicAudioSourceOnce() 
        {
            if (MusicAudioSource != null)
                return;

            var instance = Instantiate(AudioSourcePrefab);
            instance.name = "UI - AudioSource";
            MusicAudioSource = InstantiateAudioSource(instance, Lobby,true);
            MusicAudioSource.outputAudioMixerGroup = MusicChannel;
            MusicAudioSource.Play();
            SceneManager.MoveGameObjectToScene(instance, m_AdditiveScene);
        }

        public void CreateUIAudioSource() 
        {
            if (UIAudioSource != null)
                return;

            var instance = Instantiate(AudioSourcePrefab);
            instance.name = "Music - AudioSource";
            UIAudioSource = InstantiateAudioSource(instance, Click, true);
            UIAudioSource.outputAudioMixerGroup = UIChannel;
            UIAudioSource.loop = false;
            SceneManager.MoveGameObjectToScene(instance, m_AdditiveScene);
        }

        public void PlayRaceMusic()
        {
            ChangeAudio(MusicAudioSource, Race);
        }

        public void PlayCelebrationMusic()
        {
            ChangeAudio(MusicAudioSource, Celebration);
        }

        public void PlayLobbyMusic()
        {
            ChangeAudio(MusicAudioSource, Lobby);
        }

        public void PlayClick()
        {
            if (UIAudioSource == null)
            {
                CreateUIAudioSource();
            }
            UIAudioSource.clip = Click;
            UIAudioSource.Play();
        }

        private IEnumerator InitEngine(AudioSource audioSource)
        {
            ChangeAudio(audioSource, StartEngine);
            audioSource.Play();
            yield return new WaitForEndOfFrame();
            yield return new WaitForSeconds(audioSource.clip.length);
            ChangeAudio(audioSource, LoopEngine);
        }

        private AudioSource InstantiateAudioSource(GameObject instance, AudioClip clip, bool is2D)
        {
            var audioSource = instance.GetComponent<AudioSource>();
            audioSource.spatialBlend = is2D ? 0f : 1f;
            audioSource.clip = clip;
            audioSource.volume = InitialAudioSourceVolume;
            audioSource.velocityUpdateMode = AudioVelocityUpdateMode.Fixed;
            return audioSource;
        }

        private void ChangeAudio(AudioSource audioSource, AudioClip clip)
        {
            if (audioSource.clip != clip) 
            {
                audioSource.clip = clip;
                audioSource.Play();
            }
        }

        private struct AudioReference
        {
            public GameObject GameObject;
            public AudioSource AudioSource;
            public AudioClip StartEngine;
        }
    }
}