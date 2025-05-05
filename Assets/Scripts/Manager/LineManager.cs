using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LineManager : MonoBehaviour
{
    public static LineManager Instance { get; private set; }
    public List<GameObject> Lines;

    private Dictionary<(GameObject, GameObject), GameObject> LineDictionary =
        new Dictionary<(GameObject, GameObject), GameObject>();

    private ObjectPool<GameObject> LinePool;

    private GameObject linePrefab;

    void Awake()
    {
        if (Instance == null)
            Instance = this;

        Lines = new List<GameObject>();
        linePrefab = Resources.Load<GameObject>("Prefabs/Line");
    }

    void Start()
    {
        LinePool = new ObjectPool<GameObject>(
            () => Instantiate(linePrefab),
            (GameObject line) => line.SetActive(true),
            (GameObject line) => line.SetActive(false),
            (GameObject line) => Destroy(line),
            true,
            10,
            100
        );
    }

    public void AddLine(GameObject a, GameObject b)
    {
        if (a == null || b == null)
        {
            Debug.LogWarning("AddLine called with null parameter(s)");
            return;
        }

        if (LineDictionary.ContainsKey((a, b))) return;

        GameObject lineGameObject = LinePool.Get();
        if (!lineGameObject.TryGetComponent<LineRenderer>(out var lineRenderer))
        {
            lineGameObject.AddComponent<LineRenderer>().sortingOrder = -10;
        }

        if (!lineGameObject.TryGetComponent<Line>(out Line line))
            lineGameObject.AddComponent<Line>();
        line = lineGameObject.GetComponent<Line>();

        lineGameObject.transform.SetParent(transform);

        LineDictionary.Add((a, b), lineGameObject);
        LineDictionary.Add((b, a), lineGameObject);

        Lines.Add(lineGameObject);

        line.objectA = a;
        line.objectB = b;
    }

    public void RemoveLine(GameObject a, GameObject b)
    {
        if (!LineDictionary.ContainsKey((a, b))) return;

        GameObject lineToDelete = LineDictionary[(a, b)];

        if (Lines != null && Lines.Contains(lineToDelete))
        {
            Lines.Remove(lineToDelete);
        }

        LineDictionary.Remove((a, b));
        LineDictionary.Remove((b, a));

        LinePool.Release(lineToDelete);
    }

    public void ReleaseLine(GameObject line)
    {
        if(!LineDictionary.ContainsValue(line)) return;

        if (Lines != null && Lines.Contains(line))
        {
            Lines.Remove(line);
        }

        List<(GameObject, GameObject)> keysToRemove = new List<(GameObject, GameObject)>();

        foreach (var pair in LineDictionary)
        {
            if (pair.Value == line)
            {
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            LineDictionary.Remove(key);
        }

        LinePool.Release(line);
    }

    public void ClearLines()
    {
        foreach (var line in Lines)
        {
            LinePool.Release(line);
        }
        Lines.Clear();
        LineDictionary.Clear();
    }
}

