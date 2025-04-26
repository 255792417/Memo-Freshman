using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    public float cardMoveDuration = 1f;

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

    IEnumerator SetInfo(string sentenceName)
    {
        yield return new WaitForSeconds(cardMoveDuration);
        if (sentenceInfoDictionary.ContainsKey(sentenceName))
        {
            SentenceInfo sentenceInfo = sentenceInfoDictionary[sentenceName];
            string description = sentenceInfo.Description;
            string imageName = sentenceInfo.ImageName;
            InfoManager.Instance.SetSentenceInfo(description, imageName);
        }
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
                ShowSentence(sentenceInfo);
            }
        }
    }

    private void ShowSentence(SentenceInfo sentenceInfo)
    {
        CardManager.Instance.ResetAllCardsState();

        AudioManager.Instance.PlayAudioClip("句式成立", false);

        // 生成补全单词
        string[] completed;
        string[] cards = sentenceInfo.Cards;
        if(sentenceInfo.Completed == null)
            completed = sentenceInfo.Cards;
        else
        {
            completed = sentenceInfo.Completed;

            for (int i = 0, j = 0; j < completed.Length; j++)
            {
                if (completed[j] == cards[i])
                {
                    i++; j++;
                }
                else
                {
                    CardManager.Instance.SpawnCard(completed[j]);
                    j++;
                }
            }
        }

        // 移位，摆成一条线
        float length = CombineRegion.Instance.GetSizeX();
        float offset = length / completed.Length;
        float startX = CombineRegion.Instance.CenterPosition.x - length / 2 + offset / 2;
        for(int i = 0; i < completed.Length; i++)
        {
            GameObject cardGameObject = CardManager.Instance.GetCard(completed[i]);
            Card card = cardGameObject.GetComponent<Card>();

            CombineRegion.Instance.AddCard(card);
            StoreRegion.Instance.RemoveCard(card);

            Vector3 targetPosition = new Vector3(startX + offset * i, CombineRegion.Instance.CenterPosition.y, 0);
            cardGameObject.transform.DOMove(targetPosition, cardMoveDuration);
        }
        LineManager.Instance.ClearLines();

        StartCoroutine(SetInfo(sentenceInfo.Name));
    }
}

public class SentenceInfo
{
    public string Name;
    public string Description;
    public string ImageName;
    public string[] Cards;
    public string[] Completed; // 需要补全的句子补全后的样子
}
