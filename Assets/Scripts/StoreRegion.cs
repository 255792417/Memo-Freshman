using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class StoreRegion : CardDropRegion
{
    public static StoreRegion Instance { get; private set; }

    public List<Card> cards;
    private Dictionary<string, GameObject> characterCardDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> itemCardDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> motionCardDictionary = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> informationCardDictionary = new Dictionary<string, GameObject>();

    public CardType currentType;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
            Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddCard(Card card)
    {
        if (card == null || cards.Contains(card)) return;

        cards.Add(card);
        card.transform.SetParent(transform);

        string cardName = card.GetCardName();
        CardType cardType = CardManager.Instance.GetCardType(cardName);
        if (cardType == CardType.Character)
            characterCardDictionary.Add(cardName, card.gameObject);
        else if (cardType == CardType.Item)
            itemCardDictionary.Add(cardName, card.gameObject);
        else if (cardType == CardType.Motion)
            motionCardDictionary.Add(cardName, card.gameObject);
        else if (cardType == CardType.Information)
            informationCardDictionary.Add(cardName, card.gameObject);

        SentenceManager.Instance.CheckSentences();
    }

    public void RemoveCard(Card card)
    {
        if (card == null || !cards.Contains(card)) return;
        cards.Remove(card);
        string cardName = card.GetCardName();
        CardType cardType = CardManager.Instance.GetCardType(cardName);
        if (cardType == CardType.Character)
            characterCardDictionary.Remove(cardName);
        else if (cardType == CardType.Item)
            itemCardDictionary.Remove(cardName);
        else if (cardType == CardType.Motion)
            motionCardDictionary.Remove(cardName);
        else if (cardType == CardType.Information)
            informationCardDictionary.Remove(cardName);
    }

    public void OnCharacterButtonClick()
    {
        currentType = CardType.Character;
        RefreshCards();
    }

    public void OnItemButtonClick()
    {
        currentType = CardType.Item;
        RefreshCards();
    }

    public void OnMotionButtonClick()
    {
        currentType = CardType.Motion;
        RefreshCards();
    }

    public void OnInformationButtonClick()
    {
        currentType = CardType.Information;
        RefreshCards();
    }

    public void RefreshCards()
    {
        Vector3 topLeft = regionCorners[1];

        float cardSpacing = 0.5f; // 卡片之间的间距
        float leftMargin = 2f;  // 左边距
        float topMargin = 1.5f;   // 上边距

        float startX = topLeft.x + leftMargin;
        float startY = topLeft.y - topMargin;

        List<Card> visibleCards = cards;

        for (int i = 0; i < visibleCards.Count; i++)
        {
            Card card = visibleCards[i];
            RectTransform cardRect = card.GetComponent<RectTransform>();

            if (cardRect != null)
            {
                float cardWidth = cardRect.rect.width;

                float xPos = startX + i * (cardWidth + cardSpacing);

                cardRect.position = new Vector3(xPos, startY, 0);

                card.transform.SetAsLastSibling();
            }
        }
    }

}
