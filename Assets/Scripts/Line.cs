using UnityEngine;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;
    public int resolution = 50;
    public float minTurnRadius = 1.0f;
    public float lineWidth = 0.1f;
    public Color curveColor = Color.white;
    public bool debugMode = false;
    [Range(0, 1)]
    public float transitionThreshold = 0.2f; // 控制何时在贝塞尔和圆弧之间平滑过渡

    private LineRenderer lineRenderer;
    private enum ConnectionType { Bezier, CircularArcs, Transition }
    private ConnectionType lastConnectionType = ConnectionType.Bezier;

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = resolution + 1;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = curveColor;
        lineRenderer.endColor = curveColor;
    }

    void Update()
    {
        if (objectA == null || objectB == null) return;

        Vector3 startPos = objectA.transform.position + objectA.transform.right * (objectA.transform.localScale.x / 2);
        Vector3 endPos = objectB.transform.position - objectB.transform.right * (objectB.transform.localScale.x / 2);

        Vector3 startDir = objectA.transform.right.normalized;
        Vector3 endDir = objectB.transform.right.normalized;

        // 分析几何情况
        ConnectionType connectionType = DetermineConnectionType(startPos, startDir, endPos, endDir);

        // 如果连接类型发生变化，使用过渡效果
        if (connectionType != lastConnectionType)
        {
            connectionType = ConnectionType.Transition;
        }

        // 根据连接类型绘制路径
        switch (connectionType)
        {
            case ConnectionType.Bezier:
                DrawWithBezierCurve(startPos, startDir, endPos, endDir);
                break;
            case ConnectionType.CircularArcs:
                DrawWithCircularArcs(startPos, startDir, endPos, endDir);
                break;
            case ConnectionType.Transition:
                DrawWithTransition(startPos, startDir, endPos, endDir);
                break;
        }

        lastConnectionType = connectionType;
    }

    // 确定连接类型
    ConnectionType DetermineConnectionType(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        // 计算两个方向的夹角
        float angle = Vector3.Angle(startDir, endDir);

        // 当方向接近平行时，使用贝塞尔曲线
        if (angle < 30f || angle > 150f)
        {
            return ConnectionType.Bezier;
        }

        // 计算两点之间的距离
        float distance = Vector3.Distance(startPos, endPos);

        // 计算使用圆弧连接所需的最小距离
        float minDistanceNeeded = 2 * minTurnRadius * Mathf.Sin(Mathf.Deg2Rad * angle / 2);

        // 添加缓冲区以平滑过渡
        float transitionBuffer = minDistanceNeeded * transitionThreshold;

        if (distance > minDistanceNeeded + transitionBuffer)
        {
            return ConnectionType.CircularArcs;
        }
        else if (distance < minDistanceNeeded - transitionBuffer)
        {
            return ConnectionType.Bezier;
        }
        else
        {
            return ConnectionType.Transition;
        }
    }

    // 使用贝塞尔和圆弧的混合过渡
    void DrawWithTransition(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        // 创建两种路径的点
        List<Vector3> bezierPoints = new List<Vector3>();
        List<Vector3> arcPoints = new List<Vector3>();

        // 生成贝塞尔曲线点
        float distance = Vector3.Distance(startPos, endPos);
        float controlPointDistance = Mathf.Max(distance / 3, minTurnRadius);

        Vector3 controlPoint1 = startPos + startDir * controlPointDistance;
        Vector3 controlPoint2 = endPos - endDir * controlPointDistance;

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            bezierPoints.Add(CubicBezier(startPos, controlPoint1, controlPoint2, endPos, t));
        }

        // 尝试生成圆弧点
        try
        {
            GenerateCircularArcPoints(startPos, startDir, endPos, endDir, out arcPoints);
        }
        catch
        {
            // 如果圆弧生成失败，使用贝塞尔曲线
            for (int i = 0; i <= resolution; i++)
            {
                lineRenderer.SetPosition(i, bezierPoints[i]);
            }
            return;
        }

        // 如果圆弧点数量不对，使用贝塞尔曲线
        if (arcPoints.Count != resolution + 1)
        {
            for (int i = 0; i <= resolution; i++)
            {
                lineRenderer.SetPosition(i, bezierPoints[i]);
            }
            return;
        }

        // 计算过渡比例
        float angle = Vector3.Angle(startDir, endDir);
        float minDistanceNeeded = 2 * minTurnRadius * Mathf.Sin(Mathf.Deg2Rad * angle / 2);
        float actualDistance = Vector3.Distance(startPos, endPos);

        // 规范化到0-1范围
        float transitionRange = minDistanceNeeded * transitionThreshold * 2;
        float lowerBound = minDistanceNeeded - minDistanceNeeded * transitionThreshold;
        float blendFactor = Mathf.Clamp01((actualDistance - lowerBound) / transitionRange);

        // 混合两种路径
        for (int i = 0; i <= resolution; i++)
        {
            Vector3 blendedPoint = Vector3.Lerp(bezierPoints[i], arcPoints[i], blendFactor);
            lineRenderer.SetPosition(i, blendedPoint);
        }

        // 调试可视化
        if (debugMode)
        {
            Debug.DrawLine(startPos, controlPoint1, Color.yellow);
            Debug.DrawLine(endPos, controlPoint2, Color.yellow);
            Debug.Log($"Transition blend factor: {blendFactor}");
        }
    }

    // 生成圆弧点
    void GenerateCircularArcPoints(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, out List<Vector3> points)
    {
        points = new List<Vector3>();

        // 计算两个方向的夹角
        float angle = Vector3.Angle(startDir, endDir);
        bool isClockwise = Vector3.Cross(startDir, endDir).z < 0;

        // 计算圆心
        Vector3 startNormal = isClockwise ? new Vector3(-startDir.y, startDir.x, 0) : new Vector3(startDir.y, -startDir.x, 0);
        Vector3 endNormal = isClockwise ? new Vector3(-endDir.y, endDir.x, 0) : new Vector3(endDir.y, -endDir.x, 0);

        Vector3 startCenter = startPos + startNormal * minTurnRadius;
        Vector3 endCenter = endPos + endNormal * minTurnRadius;

        // 计算两个圆之间的连接线
        Vector3 centerToCenter = endCenter - startCenter;
        float centerDistance = centerToCenter.magnitude;

        // 如果两个圆心距离小于2*radius，则无法使用两个半圆连接
        if (centerDistance < 2 * minTurnRadius)
        {
            throw new System.Exception("Cannot create circular arcs");
        }

        // 计算切点
        Vector3 centerDir = centerToCenter.normalized;
        Vector3 tangentDir = new Vector3(-centerDir.y, centerDir.x, 0);

        Vector3 startTangent = startCenter + tangentDir * minTurnRadius;
        Vector3 endTangent = endCenter + tangentDir * minTurnRadius;

        // 计算圆弧的起始角度和结束角度
        Vector3 startRadiusVec = startPos - startCenter;
        Vector3 startTangentVec = startTangent - startCenter;

        Vector3 endRadiusVec = endPos - endCenter;
        Vector3 endTangentVec = endTangent - endCenter;

        float startAngle = Mathf.Atan2(startRadiusVec.y, startRadiusVec.x);
        float startEndAngle = Mathf.Atan2(startTangentVec.y, startTangentVec.x);

        float endAngle = Mathf.Atan2(endRadiusVec.y, endRadiusVec.x);
        float endStartAngle = Mathf.Atan2(endTangentVec.y, endTangentVec.x);

        // 确保角度是连续的
        if (isClockwise)
        {
            if (startEndAngle > startAngle) startEndAngle -= 2 * Mathf.PI;
            if (endAngle > endStartAngle) endAngle -= 2 * Mathf.PI;
        }
        else
        {
            if (startEndAngle < startAngle) startEndAngle += 2 * Mathf.PI;
            if (endAngle < endStartAngle) endAngle += 2 * Mathf.PI;
        }

        // 分配点到三个部分：起始圆弧、中间直线、结束圆弧
        int arcPoints = resolution / 3;
        int linePoints = resolution - 2 * arcPoints;

        // 生成起始圆弧点
        for (int i = 0; i <= arcPoints; i++)
        {
            float t = i / (float)arcPoints;
            float arcAngle = Mathf.Lerp(startAngle, startEndAngle, isClockwise ? 1 - t : t);
            Vector3 point = startCenter + new Vector3(Mathf.Cos(arcAngle), Mathf.Sin(arcAngle), 0) * minTurnRadius;
            points.Add(point);
        }

        // 生成中间直线点
        for (int i = 1; i < linePoints; i++)
        {
            float t = i / (float)linePoints;
            Vector3 point = Vector3.Lerp(startTangent, endTangent, t);
            points.Add(point);
        }

        // 生成结束圆弧点
        for (int i = 0; i <= arcPoints; i++)
        {
            float t = i / (float)arcPoints;
            float arcAngle = Mathf.Lerp(endStartAngle, endAngle, isClockwise ? 1 - t : t);
            Vector3 point = endCenter + new Vector3(Mathf.Cos(arcAngle), Mathf.Sin(arcAngle), 0) * minTurnRadius;
            points.Add(point);
        }
    }

    // 使用圆弧连接
    void DrawWithCircularArcs(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        List<Vector3> arcPoints = new List<Vector3>();

        try
        {
            GenerateCircularArcPoints(startPos, startDir, endPos, endDir, out arcPoints);

            // 设置LineRenderer点
            for (int i = 0; i < arcPoints.Count; i++)
            {
                if (i <= resolution)
                {
                    lineRenderer.SetPosition(i, arcPoints[i]);
                }
            }

            // 如果点数不够，填充剩余点
            if (arcPoints.Count <= resolution)
            {
                Vector3 lastPoint = arcPoints[arcPoints.Count - 1];
                for (int i = arcPoints.Count; i <= resolution; i++)
                {
                    lineRenderer.SetPosition(i, lastPoint);
                }
            }
        }
        catch
        {
            // 如果圆弧生成失败，回退到贝塞尔曲线
            DrawWithBezierCurve(startPos, startDir, endPos, endDir);
        }

        // 调试可视化
        if (debugMode)
        {
            // 显示物体方向
            Debug.DrawRay(startPos, startDir * minTurnRadius, Color.blue);
            Debug.DrawRay(endPos, endDir * minTurnRadius, Color.blue);

            // 计算圆心
            bool isClockwise = Vector3.Cross(startDir, endDir).z < 0;
            Vector3 startNormal = isClockwise ? new Vector3(-startDir.y, startDir.x, 0) : new Vector3(startDir.y, -startDir.x, 0);
            Vector3 endNormal = isClockwise ? new Vector3(-endDir.y, endDir.x, 0) : new Vector3(endDir.y, -endDir.x, 0);

            Vector3 startCenter = startPos + startNormal * minTurnRadius;
            Vector3 endCenter = endPos + endNormal * minTurnRadius;

            // 显示圆心和圆周
            DrawDebugCircle(startCenter, minTurnRadius, 20, Color.green);
            DrawDebugCircle(endCenter, minTurnRadius, 20, Color.green);
        }
    }

    // 使用贝塞尔曲线连接
    void DrawWithBezierCurve(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        // 计算控制点距离
        float distance = Vector3.Distance(startPos, endPos);
        float controlPointDistance = Mathf.Max(distance / 3, minTurnRadius);

        // 设置控制点
        Vector3 controlPoint1 = startPos + startDir * controlPointDistance;
        Vector3 controlPoint2 = endPos - endDir * controlPointDistance;

        // 绘制三次贝塞尔曲线
        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = CubicBezier(startPos, controlPoint1, controlPoint2, endPos, t);
            lineRenderer.SetPosition(i, point);
        }

        // 调试可视化
        if (debugMode)
        {
            Debug.DrawLine(startPos, controlPoint1, Color.yellow);
            Debug.DrawLine(endPos, controlPoint2, Color.yellow);
        }
    }

    // 三次贝塞尔曲线公式
    Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }

    // 绘制调试圆
    void DrawDebugCircle(Vector3 center, float radius, int segments, Color color)
    {
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + Vector3.right * radius;

        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep;
            Vector3 nextPoint = center + Quaternion.Euler(0, 0, angle) * Vector3.right * radius;
            Debug.DrawLine(prevPoint, nextPoint, color);
            prevPoint = nextPoint;
        }
    }
}
