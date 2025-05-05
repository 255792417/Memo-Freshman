using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardDescriptionScroll : MonoBehaviour
{
    private ScrollRect scrollRect;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
    }

    public void ScrollToTop()
    {
        Canvas.ForceUpdateCanvases();

        scrollRect.normalizedPosition = new Vector2(scrollRect.normalizedPosition.x, 1.0f);
    }

}
