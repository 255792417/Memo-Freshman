using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditSceneManager : MonoBehaviour
{
    public void OnReturnButtonClick()
    {
        SceneManager.LoadScene("StartScene");
    }
}
