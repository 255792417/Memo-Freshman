using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelBackground : MonoBehaviour
{
    [SerializeField] private List<backgroundData> backgroundList = new List<backgroundData>();
    [SerializeField] private float transitionTime = 3.0f; // 过渡时间

    [Serializable]
    public class backgroundData
    {
        public string cardName; // 生成什么卡片的时候变换背景
        public Sprite background; // 背景图片
    }

    private Image currentImage;
    [SerializeField] private Image nextImage;
    private bool isTransitioning = false;

    private void Awake()
    {
        currentImage = GetComponent<Image>();
    }

    public void UpdateBackground(string cardName)
    {
        if (backgroundList.Count == 0 || isTransitioning) return;

        if (cardName == backgroundList[0].cardName)
        {
            Sprite newBackground = backgroundList[0].background;
            backgroundList.RemoveAt(0);

            StartCoroutine(CrossFadeToNewBackground(newBackground));
        }
    }

    private IEnumerator CrossFadeToNewBackground(Sprite newBackground)
    {
        isTransitioning = true;

        nextImage.sprite = newBackground;
        float elapsedTime = 0;
        while (elapsedTime < transitionTime)
        {
            float t = elapsedTime / transitionTime;
            nextImage.color = new Color(1, 1, 1, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        nextImage.color = new Color(1, 1, 1, 1);
        currentImage.sprite = newBackground;
        nextImage.color = new Color(1, 1, 1, 0);

        isTransitioning = false;
    }
}
