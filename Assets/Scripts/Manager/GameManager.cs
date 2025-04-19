using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        GetGameInfo();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GetGameInfo()
    {
        string jsonAsset = Resources.Load<TextAsset>("Json/preset").text;
        GameInfo gameInfo = JsonConvert.DeserializeObject<GameInfo>(jsonAsset);
        if (gameInfo.CardInfo == null)
        {
            Debug.Log("CardInfo is null");
        }

        if (gameInfo.SentenceInfo == null)
        {
            Debug.Log("SentenceInfo is null");
        }
        CardManager.Instance.SetCardInfoDict(gameInfo.CardInfo);
        SentenceManager.Instance.SetSentenceInfoDict(gameInfo.SentenceInfo);
    }
}

public class GameInfo
{
    public Dictionary<string, CardInfo> CardInfo;
    public Dictionary<string, SentenceInfo> SentenceInfo;
}
