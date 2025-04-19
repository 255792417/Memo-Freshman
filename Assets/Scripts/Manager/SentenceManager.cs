using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using TMPro;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class SentenceManager : MonoBehaviour
{
    public static SentenceManager Instance { get; private set; }
    private Dictionary<string, SentenceInfo> sentenceInfoDictionary = new Dictionary<string, SentenceInfo>();
    private Dictionary<string,HashSet<string>> cardAdjacentDictionary = new Dictionary<string, HashSet<string>>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetSentenceInfoDict(Dictionary<string, SentenceInfo> sentenceInfo)
    {
        this.sentenceInfoDictionary = sentenceInfo;
        foreach (var sentence in sentenceInfoDictionary)
        {
            string[] cards = sentence.Value.Cards;
            for (int i = 0; i < cards.Length - 1; i++)
            {
                string cardName = cards[i];
                string nextCardName = cards[i + 1];
                if(!cardAdjacentDictionary.ContainsKey(cardName))
                    cardAdjacentDictionary.Add(cardName, new HashSet<string>());
                if (cardAdjacentDictionary[cardName] == null)
                    cardAdjacentDictionary[cardName] = new HashSet<string>();

                cardAdjacentDictionary[cardName].Add(nextCardName);
            }
        }
    }

    public void SetInfo(string sentenceName)
    {
        string description = sentenceInfoDictionary[sentenceName].Description;
        string imageName = sentenceInfoDictionary[sentenceName].ImageName;
        InfoManager.Instance.SetInfo(description, imageName);
    }

    public void CheckAdjCards()
    {
        List<Card> cards = CombineRegion.Instance.cards;
        for (int i = 0; i < cards.Count - 1; i++)
        {
            Card card = cards[i];
            Card nextCard = cards[i + 1];
            string cardName = card.GetCardName();
            string nextCardName = nextCard.GetCardName();

            if (cardAdjacentDictionary.ContainsKey(cardName) && cardAdjacentDictionary[cardName].Contains(nextCardName))
            {
                LineManager.Instance.AddLine(card.gameObject, nextCard.gameObject);
            }
        }
    }

    public void CheckSentences()
    {
        foreach (var sentenceInfo in sentenceInfoDictionary.Values)
        {
            string[] cards = sentenceInfo.Cards;
            List<string> curCards = CombineRegion.Instance.cardNames;
            bool isSentence = true;
            if (cards.Length == curCards.Count)
            {
                for (int i = 0; i < cards.Length; i++)
                {
                    if (cards[i] != curCards[i])
                    {
                        isSentence = false;
                        break;
                    }
                }
            }
            else
            {
                isSentence = false;
            }

            if (isSentence)
            {
                SetInfo(sentenceInfo.Name);
                CardManager.Instance.SpawnCards(sentenceInfo.NewCards);
            }
        }
    }
}

public class SentenceInfo
{
    public string Name;
    public string Description;
    public string ImageName;
    public string[] Cards;
    public string[] NewCards;
}
