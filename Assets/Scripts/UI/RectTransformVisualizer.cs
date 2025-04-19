using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class RectTransformVisualizer
{
    static RectTransformVisualizer()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        // 获取当前选中的对象
        if (Selection.activeGameObject == null)
            return;

        // 检查是否有 RectTransform 组件
        RectTransform rectTransform = Selection.activeGameObject.GetComponent<RectTransform>();
        if (rectTransform == null)
            return;

        // 获取 RectTransform 的四个角点（本地坐标）
        Vector3[] corners = new Vector3[4];
        rectTransform.GetLocalCorners(corners);

        // 将本地坐标转换为世界坐标
        for (int i = 0; i < 4; i++)
        {
            corners[i] = rectTransform.TransformPoint(corners[i]);
        }

        // 绘制矩形边框
        Handles.color = Color.green;
        Handles.DrawLine(corners[0], corners[1]);
        Handles.DrawLine(corners[1], corners[2]);
        Handles.DrawLine(corners[2], corners[3]);
        Handles.DrawLine(corners[3], corners[0]);
    }
}
