using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PathChainPlacer : MonoBehaviour
{
    [Header("Path Points")]
    [SerializeField] private Transform[] points;

    [Header("Chain")]
    [SerializeField] private GameObject chainPrefab;
    [SerializeField] private float spacing = 0.32f;
    [SerializeField] private bool rotateAlongDirection = true;
    [SerializeField] private bool loop = true;

#if UNITY_EDITOR
    public void Generate()
    {
        if (points == null || points.Length < 2 || chainPrefab == null) return;

        Clear();

        for (int i = 0; i < points.Length - 1; i++)
        {
            PlaceBetween(points[i], points[i + 1]);
        }

        if (loop)
        {
            PlaceBetween(points[points.Length - 1], points[0]);
        }
    }

    private void PlaceBetween(Transform start, Transform end)
    {
        if (start == null || end == null) return;

        Vector3 a = start.position;
        Vector3 b = end.position;
        Vector3 dir = b - a;
        float length = dir.magnitude;

        if (length <= 0.001f) return;

        Vector3 stepDir = dir.normalized;
        int count = Mathf.FloorToInt(length / spacing) + 1;

        Quaternion rot = rotateAlongDirection
            ? Quaternion.FromToRotation(Vector3.right, stepDir)
            : Quaternion.identity;

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = a + stepDir * (i * spacing);
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(chainPrefab, transform);
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.name = $"Chain_{start.name}_{end.name}_{i:000}";
        }
    }

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(PathChainPlacer))]
public class PathChainPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        PathChainPlacer t = (PathChainPlacer)target;

        GUILayout.Space(8);

        if (GUILayout.Button("Generate Chain"))
        {
            Undo.RegisterFullObjectHierarchyUndo(t.gameObject, "Generate Chain");
            t.Generate();
        }

        if (GUILayout.Button("Clear Chain"))
        {
            Undo.RegisterFullObjectHierarchyUndo(t.gameObject, "Clear Chain");
            t.Clear();
        }
    }
}
#endif