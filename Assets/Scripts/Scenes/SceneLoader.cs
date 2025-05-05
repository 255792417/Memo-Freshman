using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static AsyncOperation PreloadScene(string sceneName)
    { 
        return LoadSceneAsync(sceneName);
    }

    private static AsyncOperation LoadSceneAsync(string sceneName)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

        asyncLoad.allowSceneActivation = false;

        return asyncLoad;
    }

    public static void ActivatePreloadedScene(AsyncOperation asyncOperation)
    {
        asyncOperation.allowSceneActivation = true;
    }
}