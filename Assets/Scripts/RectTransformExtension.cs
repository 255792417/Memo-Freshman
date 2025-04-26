using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RectTransformExtension
{
    public static void CopyFrom(this RectTransform target, RectTransform source)
    {
        target.anchorMin = source.anchorMin;
        target.anchorMax = source.anchorMax;
        target.pivot = source.pivot;
        target.anchoredPosition = source.anchoredPosition;
        target.sizeDelta = source.sizeDelta;
        target.rotation = source.rotation;
        target.localScale = source.localScale;
    }
}
