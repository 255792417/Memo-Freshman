using UnityEngine;
using System.Collections.Generic;

public class Line : MonoBehaviour
{
    public GameObject objectA;
    public GameObject objectB;
    public int resolution = 50;
    public float minTurnRadius = 1.0f;
    public float lineWidth = 0.05f;
    public Color curveColor = Color.white;
    public bool debugMode = false;
    [Range(0, 1)]
    public float transitionThreshold = 0.2f;

    private LineRenderer lineRenderer;
    private enum ConnectionType { Bezier, CircularArcs, Transition }
    private ConnectionType lastConnectionType = ConnectionType.Bezier;

    void Start()
    {
        lineRenderer = gameObject.GetComponent<LineRenderer>();
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

        ConnectionType connectionType = DetermineConnectionType(startPos, startDir, endPos, endDir);

        if (connectionType != lastConnectionType)
        {
            connectionType = ConnectionType.Transition;
        }

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

    ConnectionType DetermineConnectionType(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        float angle = Vector3.Angle(startDir, endDir);

        if (angle < 30f || angle > 150f)
        {
            return ConnectionType.Bezier;
        }

        float distance = Vector3.Distance(startPos, endPos);

        float minDistanceNeeded = 2 * minTurnRadius * Mathf.Sin(Mathf.Deg2Rad * angle / 2);

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

    void DrawWithTransition(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        List<Vector3> bezierPoints = new List<Vector3>();
        List<Vector3> arcPoints = new List<Vector3>();

        float distance = Vector3.Distance(startPos, endPos);
        float controlPointDistance = Mathf.Max(distance / 3, minTurnRadius);

        Vector3 controlPoint1 = startPos + startDir * controlPointDistance;
        Vector3 controlPoint2 = endPos - endDir * controlPointDistance;

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            bezierPoints.Add(CubicBezier(startPos, controlPoint1, controlPoint2, endPos, t));
        }

        try
        {
            GenerateCircularArcPoints(startPos, startDir, endPos, endDir, out arcPoints);
        }
        catch
        {
            for (int i = 0; i <= resolution; i++)
            {
                lineRenderer.SetPosition(i, bezierPoints[i]);
            }
            return;
        }
        if (arcPoints.Count != resolution + 1)
        {
            for (int i = 0; i <= resolution; i++)
            {
                lineRenderer.SetPosition(i, bezierPoints[i]);
            }
            return;
        }

        float angle = Vector3.Angle(startDir, endDir);
        float minDistanceNeeded = 2 * minTurnRadius * Mathf.Sin(Mathf.Deg2Rad * angle / 2);
        float actualDistance = Vector3.Distance(startPos, endPos);

        float transitionRange = minDistanceNeeded * transitionThreshold * 2;
        float lowerBound = minDistanceNeeded - minDistanceNeeded * transitionThreshold;
        float blendFactor = Mathf.Clamp01((actualDistance - lowerBound) / transitionRange);

        // 混合两种路径
        for (int i = 0; i <= resolution; i++)
        {
            Vector3 blendedPoint = Vector3.Lerp(bezierPoints[i], arcPoints[i], blendFactor);
            lineRenderer.SetPosition(i, blendedPoint);
        }

        if (debugMode)
        {
            Debug.DrawLine(startPos, controlPoint1, Color.yellow);
            Debug.DrawLine(endPos, controlPoint2, Color.yellow);
            Debug.Log($"Transition blend factor: {blendFactor}");
        }
    }

    void GenerateCircularArcPoints(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir, out List<Vector3> points)
    {
        points = new List<Vector3>();

        float angle = Vector3.Angle(startDir, endDir);
        bool isClockwise = Vector3.Cross(startDir, endDir).z < 0;

        // 计算圆心
        Vector3 startNormal = isClockwise ? new Vector3(-startDir.y, startDir.x, 0) : new Vector3(startDir.y, -startDir.x, 0);
        Vector3 endNormal = isClockwise ? new Vector3(-endDir.y, endDir.x, 0) : new Vector3(endDir.y, -endDir.x, 0);

        Vector3 startCenter = startPos + startNormal * minTurnRadius;
        Vector3 endCenter = endPos + endNormal * minTurnRadius;

        Vector3 centerToCenter = endCenter - startCenter;
        float centerDistance = centerToCenter.magnitude;

        if (centerDistance < 2 * minTurnRadius)
        {
            throw new System.Exception("Cannot create circular arcs");
        }

        Vector3 centerDir = centerToCenter.normalized;
        Vector3 tangentDir = new Vector3(-centerDir.y, centerDir.x, 0);

        Vector3 startTangent = startCenter + tangentDir * minTurnRadius;
        Vector3 endTangent = endCenter + tangentDir * minTurnRadius;

        Vector3 startRadiusVec = startPos - startCenter;
        Vector3 startTangentVec = startTangent - startCenter;

        Vector3 endRadiusVec = endPos - endCenter;
        Vector3 endTangentVec = endTangent - endCenter;

        float startAngle = Mathf.Atan2(startRadiusVec.y, startRadiusVec.x);
        float startEndAngle = Mathf.Atan2(startTangentVec.y, startTangentVec.x);

        float endAngle = Mathf.Atan2(endRadiusVec.y, endRadiusVec.x);
        float endStartAngle = Mathf.Atan2(endTangentVec.y, endTangentVec.x);

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

        int arcPoints = resolution / 3;
        int linePoints = resolution - 2 * arcPoints;

        for (int i = 0; i <= arcPoints; i++)
        {
            float t = i / (float)arcPoints;
            float arcAngle = Mathf.Lerp(startAngle, startEndAngle, isClockwise ? 1 - t : t);
            Vector3 point = startCenter + new Vector3(Mathf.Cos(arcAngle), Mathf.Sin(arcAngle), 0) * minTurnRadius;
            points.Add(point);
        }

        for (int i = 1; i < linePoints; i++)
        {
            float t = i / (float)linePoints;
            Vector3 point = Vector3.Lerp(startTangent, endTangent, t);
            points.Add(point);
        }

        for (int i = 0; i <= arcPoints; i++)
        {
            float t = i / (float)arcPoints;
            float arcAngle = Mathf.Lerp(endStartAngle, endAngle, isClockwise ? 1 - t : t);
            Vector3 point = endCenter + new Vector3(Mathf.Cos(arcAngle), Mathf.Sin(arcAngle), 0) * minTurnRadius;
            points.Add(point);
        }
    }

    void DrawWithCircularArcs(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        List<Vector3> arcPoints = new List<Vector3>();

        try
        {
            GenerateCircularArcPoints(startPos, startDir, endPos, endDir, out arcPoints);

            for (int i = 0; i < arcPoints.Count; i++)
            {
                if (i <= resolution)
                {
                    lineRenderer.SetPosition(i, arcPoints[i]);
                }
            }

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
            DrawWithBezierCurve(startPos, startDir, endPos, endDir);
        }

        if (debugMode)
        {
            Debug.DrawRay(startPos, startDir * minTurnRadius, Color.blue);
            Debug.DrawRay(endPos, endDir * minTurnRadius, Color.blue);

            bool isClockwise = Vector3.Cross(startDir, endDir).z < 0;
            Vector3 startNormal = isClockwise ? new Vector3(-startDir.y, startDir.x, 0) : new Vector3(startDir.y, -startDir.x, 0);
            Vector3 endNormal = isClockwise ? new Vector3(-endDir.y, endDir.x, 0) : new Vector3(endDir.y, -endDir.x, 0);

            Vector3 startCenter = startPos + startNormal * minTurnRadius;
            Vector3 endCenter = endPos + endNormal * minTurnRadius;

            DrawDebugCircle(startCenter, minTurnRadius, 20, Color.green);
            DrawDebugCircle(endCenter, minTurnRadius, 20, Color.green);
        }
    }

    void DrawWithBezierCurve(Vector3 startPos, Vector3 startDir, Vector3 endPos, Vector3 endDir)
    {
        float distance = Vector3.Distance(startPos, endPos);
        float controlPointDistance = Mathf.Max(distance / 3, minTurnRadius);

        Vector3 controlPoint1 = startPos + startDir * controlPointDistance;
        Vector3 controlPoint2 = endPos - endDir * controlPointDistance;

        for (int i = 0; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 point = CubicBezier(startPos, controlPoint1, controlPoint2, endPos, t);
            lineRenderer.SetPosition(i, point);
        }

        if (debugMode)
        {
            Debug.DrawLine(startPos, controlPoint1, Color.yellow);
            Debug.DrawLine(endPos, controlPoint2, Color.yellow);
        }
    }

    Vector3 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        return uuu * p0 + 3 * uu * t * p1 + 3 * u * tt * p2 + ttt * p3;
    }

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
