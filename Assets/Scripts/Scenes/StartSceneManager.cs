using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    public GameData gamedata;

    private void Awake()
    {

    }

    private void Start()
    {
        AudioManager.Instance.PlayBGM("主界面BGM");
    }


    public void OnStartButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击",false);
        SceneManager.LoadScene("LoadingScene");
        gamedata.SetScene(2);
    }

    public void OnCreditButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击", false);
        SceneManager.LoadScene("LoadingScene");
        gamedata.SetScene(3);
    }
    public void OnExitButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击", false);
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
