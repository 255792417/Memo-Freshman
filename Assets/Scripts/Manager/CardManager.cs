using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    private Dictionary<string, CardInfo> cardInfoDictionary = new Dictionary<string, CardInfo>();
    private Dictionary<string, CardType> cardTypeDictionary = new Dictionary<string, CardType>();

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
        }
    }

    public void SetInfo(string cardName)
    {
        string description = "";
        string imageName = "";
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

        InfoManager.Instance.SetInfo(description, imageName);
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
    Information
}
