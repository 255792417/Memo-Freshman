using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CombineRegion : CardDropRegion
{
    public static CombineRegion Instance { get; private set; }

    public List<Card> cards;
    public List<string> cardNames;

    void Start()
    {
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
        cardNames.Add(card.GetCardName());
        RefreshLines();
    }

    public void RemoveCard(Card card)
    {
        if (card == null || !cards.Contains(card)) return;

        cards.Remove(card);
        cardNames.Remove(card.GetCardName());
        RefreshLines();
    }

    public void RefreshLines()
    {
        cards.Sort((card1, card2) => card1.transform.position.x.CompareTo(card2.transform.position.x));


        int lineCount = LineManager.Instance.transform.childCount;
        for (int i = 0; i < lineCount; i++)
        {
            GameObject line = LineManager.Instance.transform.GetChild(i).gameObject;
            LineManager.Instance.ReleaseLine(line);
        }

        SentenceManager.Instance.CheckAdjCards();
        SentenceManager.Instance.CheckSentences();
    }
}