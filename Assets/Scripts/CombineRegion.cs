using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineRegion : CardDropRegion
{
    public static CombineRegion Instance { get; private set; }

    public List<Card> cards;
    public List<string> cardNames;

    void Awake()
    {
        base.Awake();
        if (Instance == null)
            Instance = this;

        cards = new List<Card>();
        cardNames = new List<string>();
    }

    void Update()
    {

    }

    public void AddCard(Card card)
    {
        if (card == null || cards.Contains(card)) return;

        cards.Add(card);
        card.transform.SetParent(transform);
        cardNames.Add(card.GetCardName());
        RefreshLines();
        SentenceManager.Instance.CheckSentences();
        InfoManager.Instance.SetCardInfoImages(cardNames);
    }

    public void RemoveCard(Card card)
    {
        if (card == null || !cards.Contains(card)) return;

        cards.Remove(card);
        cardNames.Remove(card.GetCardName());
        RefreshLines();
        ReleaseCompletionCards();
        InfoManager.Instance.SetCardInfoImages(cardNames);
    }

    public void RefreshLines()
    {
        cards.Sort((card1, card2) => card1.transform.position.x.CompareTo(card2.transform.position.x));
        for (var i = 0; i < cards.Count; i++)
        {
            cardNames[i] = cards[i].GetCardName();
        }


        int lineCount = LineManager.Instance.transform.childCount;
        for (int i = 0; i < lineCount; i++)
        {
            GameObject line = LineManager.Instance.transform.GetChild(i).gameObject;
            LineManager.Instance.ReleaseLine(line);
        }

        SentenceManager.Instance.CheckAdjCards();
    }

    public void ReleaseCompletionCards()
    {
        for (int i = cards.Count - 1; i >= 0; i--)
        {
            var card = cards[i];
            if (CardManager.Instance.GetCardType(card.GetCardName()) == CardType.Completion)
            {
                CardManager.Instance.ReleaseCard(card.GetCardName());
                cardNames.Remove(card.GetCardName());
                cards.RemoveAt(i);
            }
        }
    }
}