using System.Collections;
using UnityEngine;

public class Starting : MonoBehaviour
{
    [SerializeField] private string animationClipName;
    private Animator animator;
    [SerializeField] private GameObject maskPanel;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public IEnumerator PlayStartingAnimation()
    {
        if (maskPanel != null)
        {
            maskPanel.SetActive(true);
        }

        animator.Play(animationClipName, 0, 0f);
        animator.speed = 1f;
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        if (maskPanel != null)
        {
            maskPanel.SetActive(false);
        }
    }
}
