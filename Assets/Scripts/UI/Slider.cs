
using UnityEngine;

public class Slider : MonoBehaviour
{
    [SerializeField] private string sliderName;

    void Start()
    {
        UnityEngine.UI.Slider slider = GetComponent<UnityEngine.UI.Slider>();
        switch (sliderName)
        {
            case "BGM":
                slider.onValueChanged.AddListener(AudioManager.Instance.SetBGMVolume);
                slider.value = AudioManager.Instance.GetBGMVolume();
                break;
            case "SFX":
                slider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
                slider.value = AudioManager.Instance.GetSFXVolume();
                break;
        }
    }

    
}
