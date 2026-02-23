using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class ChainPlacer : MonoBehaviour
{
    [Header("Points")]
    public Transform start;
    public Transform end;

    [Header("Chain")]
    public GameObject chainPrefab;
    public float spacing = 0.32f;
    public bool rotateAlongDirection = true;

#if UNITY_EDITOR
    public void Generate()
    {
        if (start == null || end == null || chainPrefab == null) return;

        Clear();

        Vector3 a = start.position;
        Vector3 b = end.position;
        Vector3 dir = (b - a);
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
            var go = (GameObject)PrefabUtility.InstantiatePrefab(chainPrefab, transform);
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.name = $"Chain_{i:000}";
        }
    }

    public void Clear()
    {
        // 子を全部削除（エディタ用）
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(ChainPlacer))]
public class  ChainPlacerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var t = (ChainPlacer)target;

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