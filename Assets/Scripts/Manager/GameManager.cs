
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameObject startAnimation;
    public List<CardDropRegion> regions = new List<CardDropRegion>();

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        GetGameInfo();

        if (startAnimation != null)
        {
            StartCoroutine(startAnimation.GetComponent<Starting>().PlayStartingAnimation());
        }

    }

    void GetGameInfo()
    {
        string jsonAsset = Resources.Load<TextAsset>("Json/preset" + SceneManager.GetActiveScene().name[5]).text;
        GameInfo gameInfo = JsonConvert.DeserializeObject<GameInfo>(jsonAsset);
        if (gameInfo.CardInfo == null)
        {
            //Debug.Log("CardInfo is null");
        }

        if (gameInfo.SentenceInfo == null)
        {
            //Debug.Log("SentenceInfo is null");
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
