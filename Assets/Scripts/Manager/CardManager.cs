using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;
using UnityEngine.Pool;
using System.Linq;
using DG.Tweening;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    private Dictionary<string, CardInfo> cardInfoDictionary = new Dictionary<string, CardInfo>();
    private Dictionary<string, CardType> cardTypeDictionary = new Dictionary<string, CardType>();
    private ObjectPool<GameObject> cardPool;

    public Dictionary<string, GameObject> cardGameObjectDictionary = new Dictionary<string, GameObject>();

    private GameObject cardPrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        cardPrefab = Resources.Load<GameObject>("Prefabs/Cards/Card");

        cardPool = new ObjectPool<GameObject>(
            () => Instantiate(cardPrefab),
            (GameObject card) => card.SetActive(true),
            (GameObject card) => card.SetActive(false),
            (GameObject card) => Destroy(card),
            true,
            10,
            100
        );
        
        Card[] cards = FindObjectsByType<Card>(FindObjectsSortMode.None);
        foreach (Card card in cards)
        {
            cardGameObjectDictionary.TryAdd(card.GetCardName(), card.gameObject);
            card.CheckRegion(out var _);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCardInfoDict(Dictionary<string,CardInfo> cardInfo)
    {
        this.cardInfoDictionary = cardInfo;
        foreach (var card in cardInfo)
        {
            string cardName = card.Key;
            string cardType = card.Value.Type;
            if (cardType == "Character")
            {
                cardTypeDictionary.Add(cardName, CardType.Character);
            }
            else if (cardType == "Item")
            {
                cardTypeDictionary.Add(cardName, CardType.Item);
            }
            else if (cardType == "Motion")
            {
                cardTypeDictionary.Add(cardName, CardType.Motion);
            }
            else if (cardType == "Information")
            {
                cardTypeDictionary.Add(cardName, CardType.Information);
            }
            else if (cardType == "Completion")
            {
                cardTypeDictionary.Add(cardName, CardType.Completion);
            }
        }
    }

    public void SetInfo(string cardName)
    {
        string description = "";
        string imageName = "";
        CardType cardType = GetCardType(cardName);
        if (cardInfoDictionary.ContainsKey(cardName))
        {
            description = cardInfoDictionary[cardName].Description;
            imageName = cardInfoDictionary[cardName].ImageName;
        }
        else
        {
            description = cardInfoDictionary["Default"].Description;
            imageName = cardInfoDictionary["Default"].ImageName;
        }

        InfoManager.Instance.SetCardInfo(description, imageName, cardType);
    }


    public void SpawnCards(string[] cardNames)
    {
        foreach(var cardName in cardNames)
        {
            SpawnCard(cardName);
        }
    }

    public void SpawnCard(string cardName, Vector3? pos = null)
    {
        if (cardGameObjectDictionary.ContainsKey(cardName)) return;

        GameObject cardGameObject = cardPool.Get();
        Card card = cardGameObject.GetComponent<Card>();
        GameObject targetGameObject = Resources.Load<GameObject>("Prefabs/Cards/" + cardName);
        if (targetGameObject == null)
        {
            Debug.LogError("Card prefab not found: " + cardName);
            return;
        }

        cardGameObjectDictionary.TryAdd(cardName, cardGameObject);

        // 设置卡牌属性
        card.SetCardName(targetGameObject.name);
        card.SetCardImage(targetGameObject.GetComponent<SpriteRenderer>().sprite);
        cardGameObject.GetComponent<RectTransform>().CopyFrom(targetGameObject.GetComponent<RectTransform>());
        if (targetGameObject.TryGetComponent<BoxCollider2D>(out var collider))
        {
            cardGameObject.AddComponent<BoxCollider2D>().CopyFrom(collider);
        }
        card.SetCardScale(cardGameObject.GetComponent<RectTransform>().localScale);
        cardGameObject.transform.position = pos ?? Vector3.zero;
    }

    public void PlaceCardRandomlyInStoreRegion(GameObject cardGameObject, string cardName)
    {
        StoreRegion storeRegion = StoreRegion.Instance;

        Vector3[] corners = storeRegion.regionCorners;

        Vector3 bottomLeft = corners[0];
        Vector3 topRight = corners[2];
        Vector3 center = (bottomLeft + topRight) / 2f;
        float width = topRight.x - bottomLeft.x;
        float height = topRight.y - bottomLeft.y;

        RectTransform cardRect = cardGameObject.GetComponent<RectTransform>();
        float cardWidth = cardRect.rect.width;
        float cardHeight = cardRect.rect.height;

        float minX = bottomLeft.x + 0.5f;
        float maxX = topRight.x - 0.5f;
        float minY = bottomLeft.y + 0.5f;
        float maxY = topRight.y - 0.5f;

        List<GameObject> existingCards = new List<GameObject>();
        foreach (var existingCard in storeRegion.cards)
        {
            if (existingCard.gameObject.activeSelf && existingCard.gameObject != cardGameObject)
            {
                existingCards.Add(existingCard.gameObject);
            }
        }

        Vector3 bestPosition = center;
        int maxAttempts = 50;
        float minDistance = Mathf.Max(cardWidth, cardHeight) * 1.5f;
        bool foundPosition = false;

        for (int i = 0; i < maxAttempts; i++)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            Vector3 testPosition = new Vector3(x, y, 0);

            bool overlapping = false;
            foreach (var existingCard in existingCards)
            {
                float distance = Vector3.Distance(testPosition, existingCard.transform.position);
                if (distance < minDistance)
                {
                    overlapping = true;
                    break;
                }
            }

            if (!overlapping)
            {
                bestPosition = testPosition;
                foundPosition = true;
                break;
            }
        }

        if (!foundPosition)
        {
            minDistance *= 1f;
            for (int i = 0; i < maxAttempts; i++)
            {
                float x = Random.Range(minX, maxX);
                float y = Random.Range(minY, maxY);
                Vector3 testPosition = new Vector3(x, y, 0);

                bool overlapping = false;
                foreach (var existingCard in existingCards)
                {
                    float distance = Vector3.Distance(testPosition, existingCard.transform.position);
                    if (distance < minDistance)
                    {
                        overlapping = true;
                        break;
                    }
                }

                if (!overlapping)
                {
                    bestPosition = testPosition;
                    foundPosition = true;
                    break;
                }
            }
        }

        if (!foundPosition)
        {
            float x = Random.Range(minX, maxX);
            float y = Random.Range(minY, maxY);
            bestPosition = new Vector3(x, y, 0);
        }

        cardGameObject.transform.DOMove(bestPosition, 1f);

        Card card = cardGameObject.GetComponent<Card>();
        storeRegion.AddCard(card);

        cardGameObject.transform.SetAsLastSibling();
    }



    public GameObject GetCard(string cardName)
    {
        if (!cardGameObjectDictionary.ContainsKey(cardName))
        {
            SpawnCard(cardName);
        }
        GameObject cardGameObject = cardGameObjectDictionary[cardName];
        cardGameObject.SetActive(true);
        return cardGameObject;
    }

    private bool isResettingCards = false;

    public void ResetAllCardsState()
    {
        if (isResettingCards)
            return;

        isResettingCards = true;

        try
        {
            foreach (var card in cardGameObjectDictionary.Values.ToList())
            {
                if (card != null)
                {
                    card.GetComponent<Card>().ResetCardState();
                }
            }
        }
        finally
        {
            isResettingCards = false;
        }
    }


    public CardType GetCardType(string cardName)
    {
        if (cardTypeDictionary.ContainsKey(cardName))
        {
            return cardTypeDictionary[cardName];
        }
        else
        {
            Debug.LogError("Card type not found: " + cardName);
            return CardType.Character;
        }
    }

    public void ReleaseCard(string CardName)
    {
        if(!cardGameObjectDictionary.ContainsKey(CardName)) return;

        GameObject cardGameObject = cardGameObjectDictionary[CardName];
        cardGameObject.SetActive(false);
        cardPool.Release(cardGameObject);
        cardGameObjectDictionary.Remove(CardName);
        cardGameObject.GetComponent<Card>().ResetCardState();
    }
}

public class CardInfo
{
    public string Name;
    public string Description;
    public string Type;
    public string ImageName;
}

public enum CardType
{
    Character,
    Item,
    Motion,
    Information,
    Completion
}
