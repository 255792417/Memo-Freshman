using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoManager : MonoBehaviour
{
    public static InfoManager Instance { get; private set; }

    public TextMeshProUGUI description;
    public Image image;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInfo(string description, string imageName)
    {
        this.description.text = description;
        this.image.sprite = Resources.Load<Sprite>("Images/" + imageName);
    }
}
