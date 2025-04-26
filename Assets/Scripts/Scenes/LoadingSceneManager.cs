using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSceneManager : MonoBehaviour
{
    public GameData gamedata;

    void Start()
    {
        StartCoroutine(LoadScene(gamedata.currentSceneIndex));
    }


    IEnumerator LoadScene(int sceneIndex)
    {
        yield return new WaitForSeconds(1f);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
    }
}
