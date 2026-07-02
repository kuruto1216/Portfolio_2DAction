using UnityEngine;

public class BGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip bgmClip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;  // BGM‚ÌŒÂ•Ê‰¹—Ê

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager‚ª‘¶İ‚µ‚Ü‚¹‚ñB");
            return;
        }

        AudioManager.Instance.PlayBGM(bgmClip, volume);
    }
}
