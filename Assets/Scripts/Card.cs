using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class Card : MonoBehaviour, IDraggable
{
    public CardState CardState;
    private PointerManager PointerManager;

    private Vector2 dragOffset;
    private Vector3 originalPosition;
    private Vector3 pointerDownPosition;
    private float pointerDownTime;

    private CardDropRegion[] regions;

    private RectTransform rectTransform;

    [SerializeField] private float normalScale = 1.0f;
    [SerializeField] private float draggedScale = 1.2f;
    [SerializeField] private float selectedScale = 1.1f;
    [SerializeField] private float scaleTransitionDuration = 0.2f;
    [SerializeField] private Ease scaleEase = Ease.OutBack;


    [SerializeField] private float maxClickDuration = 0.3f;
    [SerializeField] private float maxClickDistance = 10f;

    private CardState previousState;

    private void Awake()
    {
        CardState = CardState.Wait;
        originalPosition = transform.position;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        PointerManager = PointerManager.Instance;
    }

    private void Update()
    {
        StateUpdate();
    }

    private void StateUpdate()
    {
        switch (CardState)
        {
            case CardState.Wait:
                WaitUpdate();
                break;
            case CardState.Dragged:
                DraggedUpdate();
                break;
            case CardState.Selected:
                SelectedUpdate();
                break;
            case CardState.Used:
                break;
        }
    }

    void WaitUpdate()
    {
        DragCheck();
    }

    void SelectedUpdate()
    {
        DragCheck();

        if (PointerManager.IsMouseDown && PointerManager.TopHitObject != gameObject && PointerManager.TopHitObject != null)
        {
            UnselectCard();
        }
    }

    void DragCheck()
    {
        if (PointerManager.IsMouseDown && PointerManager.TopHitObject == gameObject)
        {
            pointerDownPosition = PointerManager.WorldPosition;
            pointerDownTime = Time.time;

            dragOffset = (Vector2)transform.position - PointerManager.WorldPosition;
            OnDragStart();
        }
    }

    void DraggedUpdate()
    {
        OnDrag(dragOffset);

        if (!PointerManager.IsMouseDown)
        {
            float clickDuration = Time.time - pointerDownTime;
            float clickDistance = Vector2.Distance(pointerDownPosition, PointerManager.WorldPosition);

            if (clickDuration <= maxClickDuration && clickDistance <= maxClickDistance)
            {
                OnClick();
            }
            else
            {
                OnDragEnd();
            }
        }
    }

    public void OnDragStart()
    {
        if (PointerManager.RegisterDraggingObject(this))
        {
            previousState = CardState;

            CardState = CardState.Dragged;
            originalPosition = transform.position;

            CombineRegion.Instance.RemoveCard(this);

            CardManager.Instance.SetInfo(GetCardName());

            transform.DOScale(draggedScale, scaleTransitionDuration).SetEase(scaleEase);
            
        }
    }

    public void OnDragEnd()
    {
        if(regions == null)
            regions = FindObjectsByType<CardDropRegion>(FindObjectsSortMode.None);

        CardDropRegion targetRegion = null;

        foreach (var region in regions)
        {
            if (region.isRectInRegion(rectTransform))
            {
                targetRegion = region;
                break;
            }
        }

        if (targetRegion == null)
        {
            transform.DOMove(originalPosition, 0.5f);

            if (previousState == CardState.Selected)
            {
                CardState = CardState.Selected;
                transform.DOScale(selectedScale, scaleTransitionDuration).SetEase(scaleEase);
            }
            else
            {
                CardState = CardState.Wait;
                transform.DOScale(normalScale, scaleTransitionDuration).SetEase(scaleEase);
            }
        }
        else
        {
            CardState = previousState;
            if (CardState == CardState.Selected)
            {
                transform.DOScale(selectedScale, scaleTransitionDuration).SetEase(scaleEase);
            }
            else
                transform.DOScale(normalScale, scaleTransitionDuration).SetEase(scaleEase);

            Debug.Log($"Card placed in region: {targetRegion.name}");
        }

        if (targetRegion is CombineRegion)
        {
            CombineRegion.Instance.AddCard(this);
        }

        PointerManager.UnregisterDraggingObject(this);
    }

    public void OnDrag(Vector2 delta)
    {
        transform.position = PointerManager.WorldPosition + delta;
    }

    public void OnClick()
    {
        if (previousState == CardState.Selected)
        {
            UnselectCard();
        }
        else if (CardState == CardState.Selected)
        {
            UnselectCard();
        }
        else
        {
            SelectCard();
        }

        PointerManager.UnregisterDraggingObject(this);
    }

    private void SelectCard()
    {

        CardState = CardState.Selected;
        transform.DOScale(selectedScale, scaleTransitionDuration).SetEase(scaleEase);

        Debug.Log($"Card selected: {gameObject.name}");
    }

    private void UnselectCard()
    {
        CardState = CardState.Wait;
        transform.DOScale(normalScale, scaleTransitionDuration).SetEase(scaleEase);

        CombineRegion.Instance.AddCard(this);
        Debug.Log($"Card unselected: {gameObject.name}");
    }

    public string GetCardName()
    {
        if (name.EndsWith("(Clone)"))
        {
            return name.Substring(0, name.Length - 7);
        }
        else
        {
            return name;
        }
    }
}

public enum CardState
{
    Wait,
    Dragged,
    Selected,
    Used
}
