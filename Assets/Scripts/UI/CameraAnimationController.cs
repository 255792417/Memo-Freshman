using System.Collections;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraAnimationController : MonoBehaviour
{
    [Header("组件引用")]
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineDollyCart dollyCart;
    public CinemachineSmoothPath dollyPath;

    [Header("移动设置")]
    public float transitionDuration = 2.0f;
    public AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("旋转设置")]
    public float totalRotationAngle = 360f; // 总旋转角度
    public AnimationCurve rotationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("缩放设置")]
    public float startOrthoSize = 5f;
    public float endOrthoSize = 10f;
    public AnimationCurve zoomCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Quaternion initialRotation;

    void Start()
    {

        // 确保相机设置正确
        virtualCamera.m_Lens.Orthographic = true;

        // 设置Dolly Cart
        dollyCart.m_Path = dollyPath;
        dollyCart.m_Position = 0;
        dollyCart.m_Speed = 0; // 我们将通过代码控制

        // 设置相机跟随Dolly Cart
        virtualCamera.Follow = dollyCart.transform;

        // 记录初始旋转
        initialRotation = virtualCamera.transform.rotation;

        // 设置初始正交大小
        LensSettings lens = virtualCamera.m_Lens;
        lens.OrthographicSize = startOrthoSize;
        virtualCamera.m_Lens = lens;
    }

    public IEnumerator StartAnimation()
    {
        yield return StartCoroutine(MoveCameraAlongPath());
    }

    private IEnumerator MoveCameraAlongPath()
    {
        // 确保相机优先级最高
        virtualCamera.Priority = 20;

        // 重置Dolly Cart位置
        dollyCart.m_Position = 0;

        float startTime = Time.time;
        float endTime = startTime + transitionDuration;

        while (Time.time < endTime)
        {
            // 计算当前进度比例(0-1)
            float t = (Time.time - startTime) / transitionDuration;

            // 1. 控制移动 - 使用移动曲线
            float moveProgress = movementCurve.Evaluate(t);
            dollyCart.m_Position = moveProgress * dollyPath.PathLength;

            // 2. 控制旋转 - 使用旋转曲线
            float rotateProgress = rotationCurve.Evaluate(t);
            float currentRotationAngle = rotateProgress * totalRotationAngle;
            virtualCamera.transform.rotation = initialRotation * Quaternion.Euler(0, 0, currentRotationAngle);

            // 3. 控制正交大小 - 使用缩放曲线
            float zoomProgress = zoomCurve.Evaluate(t);
            float currentOrthoSize = Mathf.Lerp(startOrthoSize, endOrthoSize, zoomProgress);

            LensSettings lens = virtualCamera.m_Lens;
            lens.OrthographicSize = currentOrthoSize;
            virtualCamera.m_Lens = lens;

            yield return null;
        }

        // 确保到达终点状态
        dollyCart.m_Position = dollyPath.PathLength;
        virtualCamera.transform.rotation = initialRotation * Quaternion.Euler(0, 0, totalRotationAngle);

        LensSettings finalLens = virtualCamera.m_Lens;
        finalLens.OrthographicSize = endOrthoSize;
        virtualCamera.m_Lens = finalLens;
    }

}
