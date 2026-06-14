using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject optionsPanel;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider seSlider;
    [SerializeField] private GameObject firstSelectedObject;

    private void Start()
    {
        optionsPanel.SetActive(false);

        bgmSlider.SetValueWithoutNotify(AudioManager.Instance.GetBgmVolume());
        seSlider.SetValueWithoutNotify(AudioManager.Instance.GetSeVolume());

        bgmSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        seSlider.onValueChanged.AddListener(OnSeVolumeChanged);
    }

    public void OpenOptions()
    {
        optionsPanel.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(firstSelectedObject);
    }

    public void CloseOptions()
    {
        optionsPanel.SetActive(false);
    }

    private void OnBgmVolumeChanged(float value)
    {
        AudioManager.Instance.SetBgmVolume(value);
    }

    private void OnSeVolumeChanged(float value)
    {
        AudioManager.Instance.SetSeVolume(value);
    }
}
