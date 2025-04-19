using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardDropRegion : MonoBehaviour
{
    public static CardDropRegion Instance { get; private set; }
    public RectTransform RectTransform = null;
    Vector3[] regionCorners = new Vector3[4];
    private Bounds RegionBounds;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (RectTransform == null)
        {
            RectTransform = GetComponent<RectTransform>();
        }

        RectTransform.GetWorldCorners(regionCorners);
        RegionBounds = new Bounds(
            (regionCorners[0] + regionCorners[2]) * 0.5f,
            regionCorners[2] - regionCorners[0]
        );
    }

    public bool isPointInRegion(Vector2 point)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            RectTransform,
            Camera.main.WorldToScreenPoint(point),
            null,
            out localPoint);
        return RectTransform.rect.Contains(point);
    }

    public bool isRectInRegion(RectTransform cardRect, float overlapThreshold = 0.5f)
    {
        Vector3[] cardCorners = new Vector3[4];
        cardRect.GetWorldCorners(cardCorners);

        Bounds cardBounds = new Bounds(
            (cardCorners[0] + cardCorners[2]) * 0.5f,
            cardCorners[2] - cardCorners[0]
        );

        float overlapArea = CalculateOverlapArea(cardBounds, RegionBounds);
        float cardArea = (cardBounds.size.x * cardBounds.size.y);

        return (overlapArea / cardArea) >= overlapThreshold;
    }

    private float CalculateOverlapArea(Bounds a, Bounds b)
    {
        float xOverlap = Mathf.Max(0, Mathf.Min(a.max.x, b.max.x) - Mathf.Max(a.min.x, b.min.x));
        float yOverlap = Mathf.Max(0, Mathf.Min(a.max.y, b.max.y) - Mathf.Max(a.min.y, b.min.y));
        return xOverlap * yOverlap;
    }
}
