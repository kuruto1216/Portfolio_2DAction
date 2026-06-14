using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private float bgmVolume = 0.7f;
    private float seVolume = 0.7f;

    private const string BgmVolumeParam = "BGMVolume";
    private const string SeVolumeParam = "SEVolume";
    private const string BgmVolumeKey = "BGMVolumeSetting";
    private const string SeVolumeKey = "SEVolumeSetting";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        bgmVolume = PlayerPrefs.GetFloat(BgmVolumeKey, 0.7f);
        seVolume = PlayerPrefs.GetFloat(SeVolumeKey, 0.7f);
    }

    private void Start()
    {
        ApplySavedVolume();
    }

    private void ApplySavedVolume()
    {
        SetBgmVolume(bgmVolume);
        SetSeVolume(seVolume);
    }

    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    public void PlaySE(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        seSource.PlayOneShot(clip, volume);
    }

    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        float db = ConvertVolumeToDb(bgmVolume);
        audioMixer.SetFloat(BgmVolumeParam, db);

        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSeVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);

        float db = ConvertVolumeToDb(seVolume);
        audioMixer.SetFloat(SeVolumeParam, db);

        PlayerPrefs.SetFloat(SeVolumeKey, seVolume);
        PlayerPrefs.Save();
    }

    private float ConvertVolumeToDb(float sliderValue)
    {
        if (sliderValue <= 0.0001f)
        {
            return -80f;
        }

        float normalizedVolume = sliderValue / 0.7f;

        return Mathf.Log10(normalizedVolume) * 20f;
    }

    public float GetBgmVolume()
    {
        return bgmVolume;
    }

    public float GetSeVolume()
    {
        return seVolume;
    }
}
