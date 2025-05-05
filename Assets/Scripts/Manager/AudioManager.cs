using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioData audioData;

    [System.Serializable]
    public class AudioClipData
    {
        public string name;
        public AudioClip[] clip;
        public float volume;
    }

    public List<AudioClipData> audioClips = new List<AudioClipData>();
    private Dictionary<string,AudioClipData> audioClipDictionary = new Dictionary<string, AudioClipData>();

    public AudioMixer audioMixer;
    public AudioMixerGroup bgmAudioGroup;
    public AudioMixerGroup sfxAudioGroup;

    private GameObject BgmAudioObject;

    public ObjectPool<GameObject> audioSourcePool;

    public float minVolume = -80;
    public float maxVolume = 20;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        foreach (var audioClip in audioClips)
        {
            audioClipDictionary.Add(audioClip.name, audioClip);
        }

        audioSourcePool = new ObjectPool<GameObject>(
            (() =>
            {
                GameObject audioSourceObject = new GameObject("AudioSource");
                audioSourceObject.AddComponent<AudioSource>();
                audioSourceObject.transform.SetParent(this.transform);
                return audioSourceObject;
            }),
            (source => source.SetActive(true)),
            (source => source.SetActive(false)),
            Destroy,
            true,
            10,
            100
        );

        BgmAudioObject = new GameObject("BGM");
        BgmAudioObject.AddComponent<AudioSource>();
        BgmAudioObject.transform.SetParent(this.transform);
    }

    public void PlayAudioClip(string audioName, bool isLoop)
    {
        audioClipDictionary.TryGetValue(audioName, out var audioClip);
        if (audioClip != null)
        {
            GameObject audioSourceObject = audioSourcePool.Get();
            AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();
            audioSource.clip = audioClip.clip[Random.Range(0, audioClip.clip.Length)];
            audioSource.loop = isLoop;
            audioSource.volume = audioClip.volume;
            audioSource.outputAudioMixerGroup = sfxAudioGroup;
            audioSource.Play();
            if(isLoop == false)
                StartCoroutine(ReleaseAudioSource(audioSourceObject, audioSource.clip.length));
        }
    }

    IEnumerator ReleaseAudioSource(GameObject audioSourceObject, float delay)
    {
        AudioSource audioSource = audioSourceObject.GetComponent<AudioSource>();
        yield return new WaitForSeconds(delay);
        audioSource.Stop();
        audioSourcePool.Release(audioSourceObject);
    }

    public void PlayBGM(string BGMName)
    {
        audioClipDictionary.TryGetValue(BGMName, out var audioClip);
        if (audioClip != null)
        {
            AudioSource bgmAudioSource = BgmAudioObject.GetComponent<AudioSource>();
            bgmAudioSource.clip = audioClip.clip[Random.Range(0, audioClip.clip.Length)];
            bgmAudioSource.volume = audioClip.volume;
            bgmAudioSource.loop = true;
            bgmAudioSource.outputAudioMixerGroup = bgmAudioGroup;
            bgmAudioSource.Play();
        }
    }

    public void SetBGMVolume(float value)
    {
        float epsilon = 1e-6f;
        float volume;
        if(value < epsilon)
            volume = minVolume;
        else
            volume = minVolume + (maxVolume - minVolume) * (Mathf.Log10(value / epsilon) / Mathf.Log10(1 / epsilon));
        audioMixer.SetFloat("BGMVolume", volume);
        audioData.BGMVolume = value;
    }

    public void SetSFXVolume(float value)
    {
        float epsilon = 1e-6f;
        float volume;
        if (value < epsilon)
            volume = minVolume;
        else
            volume = minVolume + (maxVolume - minVolume) * (Mathf.Log10(value / epsilon) / Mathf.Log10(1 / epsilon));
        audioMixer.SetFloat("SFXVolume", volume);
        audioData.SFXVolume = value;
    }

    public void StopBGM()
    {
        AudioSource bgmAudioSource = BgmAudioObject.GetComponent<AudioSource>();
        bgmAudioSource.Stop();
    }

    public float GetBGMVolume()
    {
        return audioData.BGMVolume;
    }

    public float GetSFXVolume()
    {
        return audioData.SFXVolume;
    }
}
