using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoManager : MonoBehaviour
{
    public static InfoManager Instance { get; private set; }

    public GameObject cardInfoPanel;
    public List<GameObject> CardImageObjects;
    public TextMeshProUGUI CardDescription;
    public CardDescriptionScroll cardDescriptionScroll;
    public Image CardImage;

    public GameObject SentenceInfoPanel;
    private CanvasGroup sentenceInfoPanelCanvasGroup;
    public float fadeDuration = 0.5f;

    public TextMeshProUGUI SentenceDescription;
    public Image SentenceImage;

    [System.Serializable]
    public class CardPos
    {
        public string cardName;
        public Vector3 position;
    }



    public List<CardPos> cardPosList = new List<CardPos>();
    private Dictionary<string, Vector3> cardPosDictionary = new Dictionary<string, Vector3>();

    [SerializeField] private TMP_FontAsset font1;
    [SerializeField] private TMP_FontAsset font2;

    [Header("高亮设置")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.6f, 0f, 0.5f); // 橙色半透明
    private string highlightColorHex;

    private string originalSentenceText;

    [Header("空白区域设置")]
    [SerializeField][Range(0.1f, 1.0f)] private float blankSpaceWidthFactor = 0.5f;


    void Awake()
    {
        if (Instance == null)
            Instance = this;

        highlightColorHex = ColorUtility.ToHtmlStringRGBA(highlightColor);
    }

    void Start()
    {
        sentenceInfoPanelCanvasGroup = SentenceInfoPanel.GetComponent<CanvasGroup>();
        CardImageObjects = cardInfoPanel.GetComponent<CardInfoPanel>().CardImageObjects;

        foreach (var cardPos in cardPosList)
        {
            cardPosDictionary[cardPos.cardName] = cardPos.position;
        }
    }

    public void SetCardInfoDescription(string description, CardType cardType)
    {
        if (description != "null")
        {
            CardDescription.gameObject.SetActive(true);
            // 处理描述文本中的[""]标记
            string processedText = ProcessCardDescription(description);
            this.CardDescription.text = processedText;
            cardDescriptionScroll.ScrollToTop();
            if (cardType == CardType.Information || cardType == CardType.Motion)
            {
                this.CardDescription.font = font1;
            }

            if (cardType == CardType.Item || cardType == CardType.Character)
            {
                this.CardDescription.font = font2;
            }
        }
    }

    public void SetCardInfoImages(List<string> cardNames)
    {
        List<string> imageNames = new List<string>();
        foreach (var cardName in cardNames)
        {
            imageNames.Add(CardManager.Instance.GetCardInfo(cardName).ImageName);
        }

        int currentIndex = 0;
        for (int i = 0; i < imageNames.Count; i++)
        {
            if(currentIndex >= CardImageObjects.Count) break;

            string imageName = imageNames[i];
            if (imageName != "null")
            {
                Image CardImage = CardImageObjects[currentIndex].GetComponent<Image>();
                CardImage.gameObject.SetActive(true);
                Sprite sprite = Resources.Load<Sprite>("Images/" + imageName);
                if (sprite == null) return;
                CardImage.sprite = sprite;
                CardImage.gameObject.name = imageName;

                CardImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                CardImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                CardImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

                float imageWidth = sprite.rect.width;
                float imageHeight = sprite.rect.height;

                float maxWidth = 2f;
                float maxHeight = 2.5f;

                float aspectRatio = imageWidth / imageHeight;

                float targetWidth, targetHeight;

                if (imageWidth / imageHeight > maxWidth / maxHeight)
                {
                    targetWidth = Mathf.Min(imageWidth, maxWidth);
                    targetHeight = targetWidth / aspectRatio;
                }
                else
                {
                    targetHeight = Mathf.Min(imageHeight, maxHeight);
                    targetWidth = targetHeight * aspectRatio;
                }

                CardImage.rectTransform.sizeDelta = new Vector2(targetWidth, targetHeight);

                CardImage.preserveAspect = true;

                currentIndex++;
            }
        }

        for (int i = currentIndex; i < CardImageObjects.Count; i++)
        {
            CardImageObjects[currentIndex].gameObject.SetActive(false);
        }
    }


    private string ProcessCardDescription(string description)
    {
        if (string.IsNullOrEmpty(description))
            return description;

        string pattern = @"\[""(.+?)""\]";

        List<GameObject> cardList = new List<GameObject>();

        string processedText = Regex.Replace(description, pattern, match => SpawnCardsInCardDescription(match, cardList));

        StartCoroutine(ShowCardsInCardDescription(cardList));

        return processedText;
    }

    private string SpawnCardsInCardDescription(Match match, List<GameObject> cardList)
    {
        string content = match.Groups[1].Value;
        if (!CardManager.Instance.HaveCard(content))
        {
            cardPosDictionary.TryGetValue(content, out var cardPos);
            CardManager.Instance.SpawnCard(content, cardPos);
            GameObject card = CardManager.Instance.GetCard(content);
            card.transform.localScale = Vector3.zero;
            cardList.Add(card);
            CardManager.Instance.PlaceCardRandomlyInStoreRegion(card,content);
        }
        return $"<color=#F35F00>{content}</color>";
    }

    IEnumerator ShowCardsInCardDescription(List<GameObject> cardList)
    {
        float elapsedTime = 0f;
        float duration = 1f;
        foreach (var card in cardList)
        {
            card.transform.localScale = Vector3.zero;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            foreach (var card in cardList)
            {
                card.transform.localScale = card.GetComponent<Card>().originalScale * (elapsedTime / duration);
            }

            yield return null;
        }

        foreach (var card in cardList)
        {
            card.transform.localScale = card.GetComponent<Card>().originalScale;
        }
    }

    public void ClearCardInfo()
    {
        this.CardDescription.text = "";
        this.CardImage.sprite = null;
    }

    private string currentSentenceName = null;
    public void SetSentenceInfo(string sentenceName,string[] descriptions)
    {
        currentSentenceName = sentenceName;
        currentSentenceDescriptions = descriptions;
        currentSentenceDescriptionIndex = 0;
        SentenceInfoPanel.SetActive(true);
        StartCoroutine(ShowSentenceInfoPanel());
        StartCoroutine(ShowNextSentenceDescription());
    }

    List<SpriteRenderer> cardSpriteRenderers = new List<SpriteRenderer>();
    public void PlaceNewCard()
    {
        foreach (var card in cardSpriteRenderers)
        {
            card.sortingLayerName = "Default";
            CardManager.Instance.PlaceCardRandomlyInStoreRegion(card.gameObject, card.name);
        }
    }

    private int currentSentenceDescriptionIndex = 0;
    private string[] currentSentenceDescriptions;
    public IEnumerator ShowNextSentenceDescription()
    {
        if (currentSentenceDescriptionIndex >= currentSentenceDescriptions.Length)
        {
            yield return StartCoroutine(HideSentenceInfoPanel());
            PlaceNewCard();
            GameOverManager.Instance.CheckGameOver(currentSentenceName);
            yield break;
        }

        if (currentSentenceDescriptionIndex != 0)
        {
            yield return StartCoroutine(HideSentenceDescription());
            yield return StartCoroutine(HideSentenceImage());

            PlaceNewCard();
        }

        string description = currentSentenceDescriptions[currentSentenceDescriptionIndex];
        string imageName = "null";
        if (description.StartsWith("{image}"))
        {
            imageName = description.Substring(7);
            description = "null";
        }

        if (description != "null")
        {
            originalSentenceText = description;
            StartCoroutine(ShowSentenceDescription());
            ParseDescription();
        }

        if (imageName != "null")
        {
            Sprite sprite = Resources.Load<Sprite>("Images/" + imageName);
            this.SentenceImage.sprite = sprite;

            this.SentenceImage.preserveAspect = true;

            RectTransform canvasRect = SentenceInfoPanel.GetComponent<RectTransform>();

            float canvasWidth = canvasRect.rect.width;
            float canvasHeight = canvasRect.rect.height;

            float imageWidth = sprite.rect.width;
            float imageHeight = sprite.rect.height;

            this.SentenceImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            this.SentenceImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            this.SentenceImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            this.SentenceImage.rectTransform.anchoredPosition = Vector2.zero;

            float scaleX = canvasWidth / imageWidth;
            float scaleY = canvasHeight / imageHeight;
            float scale = Mathf.Min(scaleX, scaleY);

            this.SentenceImage.rectTransform.sizeDelta = new Vector2(
                imageWidth * scale,
                imageHeight * scale
            );
            StartCoroutine(ShowSentenceImage());
        }

        currentSentenceDescriptionIndex++;
    }


    public void ClearSentenceInfo()
    {
        this.SentenceDescription.text = "";
        this.SentenceImage.sprite = null;
        StartCoroutine(HideSentenceInfoPanel());
    }

    IEnumerator ShowSentenceInfoPanel()
    {
        SentenceImage.gameObject.SetActive(false);
        SentenceDescription.gameObject.SetActive(false);
        float elapsedTime = 0;

        sentenceInfoPanelCanvasGroup.alpha = 0;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);

            sentenceInfoPanelCanvasGroup.alpha = alpha;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        sentenceInfoPanelCanvasGroup.alpha = 1;
    }

    IEnumerator HideSentenceInfoPanel()
    {
        float elapsedTime = 0;

        sentenceInfoPanelCanvasGroup.alpha = 1;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);

            sentenceInfoPanelCanvasGroup.alpha = alpha;

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        sentenceInfoPanelCanvasGroup.alpha = 0;
        SentenceInfoPanel.SetActive(false);
    }

    IEnumerator ShowSentenceImage()
    {
        SentenceImage.gameObject.SetActive(true);
        float fadeDuration = this.fadeDuration / 2;
        float elapsedTime = 0;

        Vector4 curColor = new Vector4(SentenceImage.color.r, SentenceImage.color.g, SentenceImage.color.b, 0);
        SentenceImage.color = curColor;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            curColor.w = alpha;
            SentenceImage.color = curColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        curColor.w = 1;
        SentenceImage.color = curColor;
    }

    IEnumerator HideSentenceImage()
    {
        float fadeDuration = this.fadeDuration / 2;
        float elapsedTime = 0;

        Vector4 curColor = new Vector4(SentenceImage.color.r, SentenceImage.color.g, SentenceImage.color.b, 1);
        SentenceImage.color = curColor;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            curColor.w = alpha;
            SentenceImage.color = curColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        curColor.w = 0;
        SentenceImage.color = curColor;
        SentenceImage.gameObject.SetActive(false);
    }

    IEnumerator ShowSentenceDescription()
    {
        SentenceDescription.gameObject.SetActive(true);
        float fadeDuration = this.fadeDuration / 2;
        float elapsedTime = 0;

        Vector4 curColor = new Vector4(SentenceDescription.color.r, SentenceDescription.color.g, SentenceDescription.color.b, 0);
        SentenceDescription.color = curColor;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            curColor.w = alpha;
            SentenceDescription.color = curColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        curColor.w = 1;
        SentenceDescription.color = curColor;
    }

    IEnumerator HideSentenceDescription()
    {
        float fadeDuration = this.fadeDuration / 2;
        float elapsedTime = 0;

        Vector4 curColor = new Vector4(SentenceDescription.color.r, SentenceDescription.color.g, SentenceDescription.color.b, 1);
        SentenceDescription.color = curColor;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);
            curColor.w = alpha;
            SentenceDescription.color = curColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        curColor.w = 0;
        SentenceDescription.color = curColor;
        SentenceDescription.gameObject.SetActive(false);
    }

    public void ParseDescription()
    {
        if (string.IsNullOrEmpty(originalSentenceText) || SentenceDescription == null)
            return;

        SentenceDescription.text = originalSentenceText;
        SentenceDescription.ForceMeshUpdate();

        string pattern = @"\[""(.+?)""\]";
        MatchCollection matches = Regex.Matches(originalSentenceText, pattern);

        List<(string cardName, int startIndex, int endIndex, string originalText)> cardInfos =
            new List<(string cardName, int startIndex, int endIndex, string originalText)>();

        foreach (Match match in matches)
        {
            string cardName = match.Groups[1].Value;
            int startIndex = match.Index;
            int endIndex = startIndex + match.Length - 1;
            string originalText = match.Value;

            cardInfos.Add((cardName, startIndex, endIndex, originalText));
        }

        TMP_TextInfo textInfo = SentenceDescription.textInfo;
        Dictionary<string, (int startLine, int endLine, int startIndex, int endIndex, Vector3 position)> cardLineInfo =
            new Dictionary<string, (int startLine, int endLine, int startIndex, int endIndex, Vector3 position)>();

        foreach (var info in cardInfos)
        {
            string cardName = info.cardName;
            int startIndex = info.startIndex;
            int endIndex = info.endIndex;
            int firstCharLine = -1;
            int lastCharLine = -1;

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, 0);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, 0);
            bool foundValidChar = false;

            for (int i = startIndex; i <= endIndex; i++)
            {
                if (i < textInfo.characterCount)
                {
                    TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

                    if (charInfo.isVisible)
                    {
                        foundValidChar = true;
                        int currentLine = charInfo.lineNumber;
                        if (firstCharLine == -1) firstCharLine = currentLine;
                        lastCharLine = currentLine;
                        if (lastCharLine != firstCharLine) break;

                        Vector3 bl = SentenceDescription.transform.TransformPoint(charInfo.bottomLeft);
                        Vector3 tl = SentenceDescription.transform.TransformPoint(charInfo.topLeft);
                        Vector3 tr = SentenceDescription.transform.TransformPoint(charInfo.topRight);
                        Vector3 br = SentenceDescription.transform.TransformPoint(charInfo.bottomRight);

                        min.x = Mathf.Min(min.x, bl.x, tl.x, tr.x, br.x);
                        min.y = Mathf.Min(min.y, bl.y, tl.y, tr.y, br.y);

                        max.x = Mathf.Max(max.x, bl.x, tl.x, tr.x, br.x);
                        max.y = Mathf.Max(max.y, bl.y, tl.y, tr.y, br.y);
                    }
                }
            }

            Vector3 center = Vector3.zero;
            if (foundValidChar)
            {
                center = new Vector3((min.x + max.x) / 2f, (min.y + max.y) / 2f, 0);
            }
            if (firstCharLine != -1)
            {
                cardLineInfo[cardName] = (firstCharLine, lastCharLine, startIndex, endIndex, center);
            }
        }

        string processedText = originalSentenceText;

        for (int i = cardInfos.Count - 1; i >= 0; i--)
        {
            var info = cardInfos[i];
            string originalText = info.originalText;
            int startIndex = info.startIndex;

            string replacement;

            if (blankSpaceWidthFactor <= 0.3f)
            {
                replacement = new string('\u2009', originalText.Length); // 细空格
            }
            else if (blankSpaceWidthFactor <= 0.6f)
            {
                replacement = new string('\u2002', originalText.Length / 2 + 1); // en空格
            }
            else
            {
                replacement = new string('\u2003', originalText.Length / 2); // em空格
            }

            processedText = processedText.Remove(startIndex, originalText.Length)
                                       .Insert(startIndex, replacement);
        }

        SentenceDescription.text = processedText;
        SentenceDescription.ForceMeshUpdate();

        StartCoroutine(ShowCards(cardLineInfo));
    }

    IEnumerator ShowCards(Dictionary<string, (int startLine, int endLine, int startIndex, int endIndex, Vector3 position)> cardLineInfo)
    {
        List<SpriteRenderer> cards = new List<SpriteRenderer>();
        foreach (var entry in cardLineInfo)
        {
            string cardName = entry.Key;
            cardPosDictionary.TryGetValue(cardName, out var cardPos);
            CardManager.Instance.SpawnCard(cardName, cardPos);
            GameObject card = CardManager.Instance.GetCard(cardName);
            card.transform.DOMove(cardPos,1f);
            card.GetComponent<SpriteRenderer>().sortingLayerName = "NewCard";
            cards.Add(card.GetComponent<SpriteRenderer>());
        }

        cardSpriteRenderers = cards;

        float elapsedTime = 0f;
        foreach (var card in cards)
        {
            card.color = new Color(1f, 1f, 1f, 0f);
        }

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);
            foreach (var card in cards)
            {
                card.color = new Color(1f, 1f, 1f, alpha);
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        foreach (var card in cards)
        {
            card.color = new Color(1f, 1f, 1f, 1f);
        }
    }


    private bool isProcessing = false;

    public void OnSentenceInfoPanelClick()
    {
        if (isProcessing)
            return;

        isProcessing = true;
        StartCoroutine(ShowNextSentenceDescriptionWithFlag());
    }

    private IEnumerator ShowNextSentenceDescriptionWithFlag()
    {
        yield return StartCoroutine(ShowNextSentenceDescription());

        isProcessing = false;
    }

    public void SetCardPos(string cardName, Vector3 pos)
    {
        cardPosDictionary[cardName] = pos;
    }
}


