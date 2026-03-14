using UnityEditor;
using UnityEngine;

public class ChangeActiveObjects : MonoBehaviour
{
    [MenuItem("Tools/Scene/OffAllObjects")]
    static void DisableAllObjects()
    {
        Undo.SetCurrentGroupName("OffAllObjects");
        int undoGroup = Undo.GetCurrentGroup();

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj.activeSelf)
            {
                Undo.RecordObject(obj, "Object off");
                obj.SetActive(false);
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
    }

    [MenuItem("Tools/Scene/OnAllObjects")]
    static void EnableAllObjects()
    {
        Undo.SetCurrentGroupName("OnAllObjects");
        int undoGroup = Undo.GetCurrentGroup();

        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (!obj.activeSelf)
            {
                Undo.RecordObject(obj, "Object on");
                obj.SetActive(true);
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
    }
}
