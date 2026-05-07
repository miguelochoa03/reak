#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts In Scene")]
    public static void ScanScene()
    {
        int total = 0;
        int gameObjectsWithMissing = 0;
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
            total += ScanRecursively(root, ref gameObjectsWithMissing);
        Debug.Log($"[FindMissingScripts] Scanned scene '{scene.name}'. Missing-script components: {total} on {gameObjectsWithMissing} GameObjects.");
    }

    [MenuItem("Tools/Remove Missing Scripts In Scene")]
    public static void RemoveScene()
    {
        int total = 0;
        var scene = SceneManager.GetActiveScene();
        foreach (var root in scene.GetRootGameObjects())
            total += RemoveRecursively(root);
        Debug.Log($"[FindMissingScripts] Removed {total} missing-script components from scene '{scene.name}'.");
        EditorSceneManager.MarkSceneDirty(scene);
    }

    static int ScanRecursively(GameObject go, ref int gameObjectsWithMissing)
    {
        int missing = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
        if (missing > 0)
        {
            gameObjectsWithMissing++;
            Debug.LogWarning($"[FindMissingScripts] '{GetPath(go)}' has {missing} missing script(s).", go);
        }
        int total = missing;
        foreach (Transform child in go.transform)
            total += ScanRecursively(child.gameObject, ref gameObjectsWithMissing);
        return total;
    }

    static int RemoveRecursively(GameObject go)
    {
        int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
        foreach (Transform child in go.transform)
            removed += RemoveRecursively(child.gameObject);
        return removed;
    }

    static string GetPath(GameObject go)
    {
        var path = go.name;
        var t = go.transform.parent;
        while (t != null) { path = t.name + "/" + path; t = t.parent; }
        return path;
    }
}
#endif
