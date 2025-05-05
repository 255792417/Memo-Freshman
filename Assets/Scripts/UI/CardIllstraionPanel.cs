using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // 需要引入 DOTween

public class CardIllustraionPanel : MonoBehaviour
{
    [SerializeField] private GameObject blockingPanel;
    [SerializeField] private Image image;

    [SerializeField] private Vector2 startPosition;
    [SerializeField] private Vector2 startSize = new Vector2(100, 150);
    [SerializeField] private Vector2 finalSize;

    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private Ease easeType = Ease.OutQuad;

    private RectTransform imageRectTransform;

    private void Awake()
    {
        imageRectTransform = image.GetComponent<RectTransform>();

        if (finalSize == Vector2.zero)
        {
            finalSize = new Vector2(Screen.width, Screen.height);
        }
    }

    public void OnIllustrationPanelClick()
    {
        HideIllustrationPanel();
    }

    public void SetIllustrationImage(string illustrationName)
    {
        Sprite sprite = Resources.Load<Sprite>("Images/" + illustrationName);
        image.sprite = sprite;
    }

    public void ShowIllustrationPanel()
    {
        if (blockingPanel != null)
        {
            blockingPanel.SetActive(true);
        }

        // 设置初始状态
        image.gameObject.SetActive(true);
        image.color = new Color(image.color.r, image.color.g, image.color.b, 0);
        imageRectTransform.anchoredPosition = startPosition;
        imageRectTransform.sizeDelta = startSize;

        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        // 添加透明度动画
        sequence.Join(image.DOFade(1, animationDuration));

        // 添加大小动画
        sequence.Join(imageRectTransform.DOSizeDelta(finalSize, animationDuration).SetEase(easeType));

        // 添加位置动画
        sequence.Join(imageRectTransform.DOAnchorPos(Vector2.zero, animationDuration).SetEase(easeType));

        // 播放动画
        sequence.Play();
    }

    public void HideIllustrationPanel()
    {
        // 创建动画序列
        Sequence sequence = DOTween.Sequence();

        // 添加透明度动画
        sequence.Join(image.DOFade(0, animationDuration));

        // 添加大小动画
        sequence.Join(imageRectTransform.DOSizeDelta(startSize, animationDuration).SetEase(easeType));

        // 添加位置动画
        sequence.Join(imageRectTransform.DOAnchorPos(startPosition, animationDuration).SetEase(easeType));

        // 动画完成后的回调
        sequence.OnComplete(() => {
            if (blockingPanel != null)
            {
                blockingPanel.SetActive(false);
            }
            image.gameObject.SetActive(false);
        });

        // 播放动画
        sequence.Play();
    }

    // 保留协程版本，以便与原有代码兼容
    public IEnumerator ShowIllustrationPanelCoroutine()
    {
        ShowIllustrationPanel();
        yield return new WaitForSeconds(animationDuration);
    }

    public IEnumerator HideIllustrationPanelCoroutine()
    {
        HideIllustrationPanel();
        yield return new WaitForSeconds(animationDuration);
    }
}
