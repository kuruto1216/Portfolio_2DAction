using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManagerÇ™ë∂ç›ÇµÇ‹ÇπÇÒÅB");
            return;
        }

        AudioManager.Instance.PlayBGM(bgmClip, volume);
    }
}
