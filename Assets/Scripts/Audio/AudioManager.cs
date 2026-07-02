using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }   // Singleton

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    // 音量設定
    private float bgmVolume = 0.7f;
    private float seVolume = 0.7f;

    // PlayerPrefs Key
    private const string BgmVolumeParam = "BGMVolume";
    private const string SeVolumeParam = "SEVolume";
    private const string BgmVolumeKey = "BGMVolumeSetting";
    private const string SeVolumeKey = "SEVolumeSetting";

    // ===== 初期化 =====

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

    // 保存済みの音量設定を適用する
    private void ApplySavedVolume()
    {
        SetBgmVolume(bgmVolume);
        SetSeVolume(seVolume);
    }

    // ===== BGM・SE再生 =====

    // BGMを再生する
    public void PlayBGM(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.volume = volume;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // BGMを停止する
    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    // SEを再生する
    public void PlaySE(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        seSource.PlayOneShot(clip, volume);
    }

    // ===== 音量設定 =====

    // BGM音量を設定して保存する
    public void SetBgmVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);

        float db = ConvertVolumeToDb(bgmVolume);
        audioMixer.SetFloat(BgmVolumeParam, db);

        PlayerPrefs.SetFloat(BgmVolumeKey, bgmVolume);
        PlayerPrefs.Save();
    }

    // SE音量を設定して保存する
    public void SetSeVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);

        float db = ConvertVolumeToDb(seVolume);
        audioMixer.SetFloat(SeVolumeParam, db);

        PlayerPrefs.SetFloat(SeVolumeKey, seVolume);
        PlayerPrefs.Save();
    }

    // スライダー値(0～1)をAudioMixer用のdBに変換する
    // 0だとLog10が計算できないため、-80dB(ほぼ無音)として扱う
    private float ConvertVolumeToDb(float sliderValue)
    {
        if (sliderValue <= 0.0001f)
        {
            return -80f;
        }

        float normalizedVolume = sliderValue / 0.7f;

        return Mathf.Log10(normalizedVolume) * 20f;
    }

    // ===== 音量取得 =====

    // 現在のBGM音量を取得する
    public float GetBgmVolume()
    {
        return bgmVolume;
    }

    // 現在のSE音量を取得する
    public float GetSeVolume()
    {
        return seVolume;
    }
}
