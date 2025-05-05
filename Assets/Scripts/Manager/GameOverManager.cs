
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [SerializeField] private string[] sentenceNames;
    [SerializeField] private string nextSceneName;
    public Ending ending;

    private AsyncOperation asyncOperation;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    void Start()
    {
        asyncOperation = SceneLoader.PreloadScene(nextSceneName);
    }

    public void CheckGameOver(string sentenceName)
    {
        foreach (var curSentenceName in sentenceNames)
        {
            if (sentenceName == curSentenceName)
            {
                StartCoroutine(GameOver());
            }
        }
    }

    IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1f);
        if(ending != null)
            yield return ending.PlayEndingAnimation();
        SceneLoader.ActivatePreloadedScene(asyncOperation);
    }
}
