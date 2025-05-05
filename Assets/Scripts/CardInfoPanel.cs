using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInfoPanel : MonoBehaviour
{
    public List<GameObject> CardImageObjects = new List<GameObject>();
    public CardIllustraionPanel cardIllustraionPanel;

    public void OnImageButtonClick(GameObject image)
    {
        if (IsLastSibling(image))
        {
            string illustrationName = CardManager.Instance.CardIllustrationDictionary[image.gameObject.name];
            if(illustrationName != "null")
            {
                cardIllustraionPanel.SetIllustrationImage(illustrationName);
                cardIllustraionPanel.ShowIllustrationPanel();
                return;
            }
        }
        image.transform.SetAsLastSibling();
    }

    public bool IsLastSibling(GameObject gameObject)
    {
        Transform transform = gameObject.transform;
        Transform parent = transform.parent;

        if (parent == null)
        {
            return transform.GetSiblingIndex() == UnityEngine.SceneManagement.SceneManager.GetActiveScene().rootCount - 1;
        }

        return transform.GetSiblingIndex() == parent.childCount - 1;
    }
}
