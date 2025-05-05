
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    public GameData gamedata;
    public Camera mainCamera;

    private AsyncOperation asyncOperation;

    private void Awake()
    {

    }

    private void Start()
    {
        AudioManager.Instance.PlayBGM("主界面BGM");

        asyncOperation = SceneLoader.PreloadScene("Level1Scene");
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            SceneManager.LoadScene("Level1Scene");
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            SceneManager.LoadScene("Level2Scene");
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            SceneManager.LoadScene("Level3Scene");
        }
    }

    public void OnStartButtonClick()
    {
        StartCoroutine(OnStartButtonClickCoroutine());
    }

    private IEnumerator OnStartButtonClickCoroutine()
    {
        AudioManager.Instance.PlayAudioClip("UI点击",false);
        yield return GetComponent<CameraAnimationController>().StartAnimation();
        SceneLoader.ActivatePreloadedScene(asyncOperation);
        AudioManager.Instance.StopBGM();
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
