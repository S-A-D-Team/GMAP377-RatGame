using UnityEngine;
using UnityEditor;

public class ReplaceWithPrefab : EditorWindow
{
    private GameObject prefab;

    [MenuItem("Tools/Replace With Prefab")]
    public static void ShowWindow()
    {
        GetWindow<ReplaceWithPrefab>("Replace With Prefab");
    }

    void OnGUI()
    {
        GUILayout.Label("Replace Selected Objects", EditorStyles.boldLabel);
        prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefab, typeof(GameObject), false);

        if (GUILayout.Button("Replace Selected"))
        {
            if (prefab == null)
            {
                return;
            }

            ReplaceSelectedWithPrefab();
        }
    }

    void ReplaceSelectedWithPrefab()
    {
        GameObject[] selectedObjects = Selection.gameObjects;

        if (selectedObjects.Length == 0)
        {
            return;
        }


        foreach (GameObject go in selectedObjects)
        {
            Transform t = go.transform;

            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab);

            newObj.transform.SetPositionAndRotation(t.position, t.rotation);
            newObj.transform.localScale = t.localScale;

            newObj.transform.parent = t.parent; //maintain hierarchy

        }

    }
}
