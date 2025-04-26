using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
                break;
            case "SFX":
                slider.onValueChanged.AddListener(AudioManager.Instance.SetSFXVolume);
                break;
        }
    }

    
}
