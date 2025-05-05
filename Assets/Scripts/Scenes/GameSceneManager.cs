
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public GameData gameData;

    public void Start()
    {
        AudioManager.Instance.PlayBGM("游戏内BGM");
    }

    public void OnSettingsButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击", false);
    }

    public void OnContinueButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击", false);
    }

    public void OnAudioButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击", false);
    }

    public void OnReturnButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击", false);
        SceneManager.LoadScene("StartScene");
        gameData.SetScene(0);
    }

    public void OnAudioPanelButtonClick()
    {
        AudioManager.Instance.PlayAudioClip("UI点击", false);
    }
}
