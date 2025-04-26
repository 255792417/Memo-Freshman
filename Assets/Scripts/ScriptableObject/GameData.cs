using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameData", menuName = "ScriptableObjects/GameData", order = 1)]
public class GameData : ScriptableObject
{
    public int currentSceneIndex = 0;

    public void SetScene(int sceneIndex)
    {
        currentSceneIndex = sceneIndex;
    }
}
