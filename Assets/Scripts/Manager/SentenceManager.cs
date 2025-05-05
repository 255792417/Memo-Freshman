
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class SentenceManager : MonoBehaviour
{
    public class MultiSentenceInfo
    {
        public int curIndex;
        public List<SentenceInfo> sentenceInfos;
    }

    [System.Serializable]
    public class Precondition
    {
        public List<string> conditions;
        public string sentenceName;
    }

    public static SentenceManager Instance { get; private set; }
    private Dictionary<string, SentenceInfo> sentenceInfoDictionary = new Dictionary<string, SentenceInfo>();
    private Dictionary<string, MultiSentenceInfo> multiSentenceInfoDictionary = new Dictionary<string, MultiSentenceInfo>();
    private Dictionary<string,HashSet<string>> cardAdjacentDictionary = new Dictionary<string, HashSet<string>>();
    public float cardMoveDuration = 1f;

    public List<Precondition> preconditions = new List<Precondition>();
    private Dictionary<string, List<string>> preconditionDictionary = new Dictionary<string, List<string>>(); // 预设条件字典，key为句子名，value为条件列表
    private HashSet<string> activatedSentences = new HashSet<string>();

    [System.Serializable]
    public class uniqueCardPosInfo
    {
        public string sentenceName;
        public Vector3 position;
    }

    public List<uniqueCardPosInfo> uniqueCardPosInfos = new List<uniqueCardPosInfo>();

    public string uniqueCardName = "1月17日"; // 处理"1月17日"的名字
    //处理"1月17日"的位置
    private Dictionary<string, Vector3> uniqueCardPos = new Dictionary<string, Vector3>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        foreach (var sentenceInfo in sentenceInfoDictionary.Values)
        {
            if (char.IsDigit(sentenceInfo.Name.Last()))
            {
                string multiSentenceName = sentenceInfo.Name.Substring(0, sentenceInfo.Name.Length - 1);
                if (!multiSentenceInfoDictionary.ContainsKey(multiSentenceName))
                {
                    MultiSentenceInfo multiSentenceInfo = new MultiSentenceInfo();
                    multiSentenceInfo.sentenceInfos = new List<SentenceInfo>();
                    multiSentenceInfo.curIndex = 0;
                    multiSentenceInfoDictionary.Add(multiSentenceName, multiSentenceInfo);
                }
                multiSentenceInfoDictionary[multiSentenceName].sentenceInfos.Add(sentenceInfo);
            }
        }

        foreach (var multiSentenceInfo in multiSentenceInfoDictionary)
        {
            multiSentenceInfo.Value.sentenceInfos = multiSentenceInfo.Value.sentenceInfos.OrderBy(x => x.Name.Last()).ToList();
            for (int i = 0; i < multiSentenceInfo.Value.sentenceInfos.Count; i++)
            {
                sentenceInfoDictionary.Remove(multiSentenceInfo.Value.sentenceInfos[i].Name);
            }
            sentenceInfoDictionary.Add(multiSentenceInfo.Key, multiSentenceInfo.Value.sentenceInfos[0]);
        }

        foreach (var precondition in preconditions)
        {
            if (!preconditionDictionary.ContainsKey(precondition.sentenceName))
            {
                preconditionDictionary.Add(precondition.sentenceName, new List<string>());
            }
            preconditionDictionary[precondition.sentenceName].AddRange(precondition.conditions);
        }

        foreach (var uniqueCardPosInfo in uniqueCardPosInfos)
        {
            if (!uniqueCardPos.ContainsKey(uniqueCardPosInfo.sentenceName))
            {
                uniqueCardPos.Add(uniqueCardPosInfo.sentenceName, uniqueCardPosInfo.position);
            }
        }
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

    IEnumerator SetInfo(KeyValuePair<string,SentenceInfo> sentenceInfo)
    {
        if (uniqueCardPos.ContainsKey(sentenceInfo.Value.Name))
        {
            InfoManager.Instance.SetCardPos(uniqueCardName, uniqueCardPos[sentenceInfo.Value.Name]);
        }

        yield return new WaitForSeconds(cardMoveDuration);
        if (sentenceInfoDictionary.ContainsKey(sentenceInfo.Key))
        {
            string[] description = sentenceInfo.Value.Description;
            InfoManager.Instance.SetSentenceInfo(sentenceInfo.Value.Name,description);
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

    private float lastCheckTime = 0f;
    private float checkInterval = 0.2f;
    public void CheckSentences()
    {
        StartCoroutine(CheckSentencesCoroutine());
    }

    private string lastSentence = null;
    private IEnumerator CheckSentencesCoroutine()
    {
        List<string> curCards = CombineRegion.Instance.cardNames;
        KeyValuePair<string, SentenceInfo> resSentenceInfo = new KeyValuePair<string, SentenceInfo>();

        foreach (var sentenceInfo in sentenceInfoDictionary)
        {
            if (preconditionDictionary.ContainsKey(sentenceInfo.Value.Name))
            {
                List<string> preconditions = preconditionDictionary[sentenceInfo.Value.Name];
                bool isPrecondition = false;
                foreach (var precondition in preconditions)
                {
                    if (activatedSentences.Contains(precondition))
                    {
                        isPrecondition = true;
                        break;
                    }
                }
                if (!isPrecondition) continue;
            }

            if (lastSentence == sentenceInfo.Key && !multiSentenceInfoDictionary.ContainsKey(sentenceInfo.Key)) continue; // 避免重复触发
            string[] cards = sentenceInfo.Value.Cards;
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
                if(Time.time - lastCheckTime < checkInterval) yield break;
                lastCheckTime = Time.time;

                lastSentence = sentenceInfo.Key;
                activatedSentences.Add(sentenceInfo.Key);
                // 等待 ShowSentence 完成
                yield return StartCoroutine(ShowSentenceCoroutine(sentenceInfo));
                resSentenceInfo = sentenceInfo;
                break;
            }
        }

        if (resSentenceInfo.Key == null) yield break;

        if (multiSentenceInfoDictionary.ContainsKey(resSentenceInfo.Key))
        {
            multiSentenceInfoDictionary[resSentenceInfo.Key].curIndex++;
            if (multiSentenceInfoDictionary[resSentenceInfo.Key].curIndex >= multiSentenceInfoDictionary[resSentenceInfo.Key].sentenceInfos.Count)
            {
                multiSentenceInfoDictionary.Remove(resSentenceInfo.Key);
                sentenceInfoDictionary.Remove(resSentenceInfo.Key);
            }
            else
            {
                sentenceInfoDictionary[resSentenceInfo.Key] = multiSentenceInfoDictionary[resSentenceInfo.Key].sentenceInfos[multiSentenceInfoDictionary[resSentenceInfo.Key].curIndex];
            }
        }
    }

    private IEnumerator ShowSentenceCoroutine(KeyValuePair<string,SentenceInfo> sentenceInfo)
    {
        CardManager.Instance.ResetAllCardsState();
        AudioManager.Instance.PlayAudioClip("句式成立", false);

        // 生成补全单词
        string[] completed;
        string[] cards = sentenceInfo.Value.Cards;
        if (sentenceInfo.Value.Completed == null)
            completed = sentenceInfo.Value.Cards;
        else
        {
            completed = sentenceInfo.Value.Completed;

            for (int i = 0, j = 0; j < completed.Length && i < cards.Length; j++)
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
        for (int i = 0; i < completed.Length; i++)
        {
            CardManager.Instance.SpawnCard(completed[i]);
            GameObject cardGameObject = CardManager.Instance.GetCard(completed[i]);
            Card card = cardGameObject.GetComponent<Card>();

            CombineRegion.Instance.AddCard(card);
            StoreRegion.Instance.RemoveCard(card);

            Vector3 targetPosition = new Vector3(startX + offset * i, CombineRegion.Instance.CenterPosition.y + Random.Range(-.3f, .3f), 0);
            cardGameObject.transform.DOMove(targetPosition, cardMoveDuration);
        }
        LineManager.Instance.ClearLines();

        yield return StartCoroutine(SetInfo(sentenceInfo));
    }
}

public class SentenceInfo
{
    public string Name;
    public string[] Description;
    public string[] Cards;
    public string[] Completed; // 需要补全的句子补全后的样子
}
