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

    [MenuItem("Tools/Scene/OffLateActiveObject")]
    static void OffLateActiveObject()
    {
        Undo.SetCurrentGroupName("OffLateActiveObject");
        int undoGroup = Undo.GetCurrentGroup();

        LateActiveObject[] allObjects = FindObjectsByType<LateActiveObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (LateActiveObject obj in allObjects)
        {
            if (!obj.IsActiveObject)
                continue;

            foreach(Transform child in obj.gameObject.transform)
            {
                if(child.gameObject.activeSelf)
                {
                    Undo.RecordObject(child, "Object off");
                    child.gameObject.SetActive(false);
                }
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
    }

    [MenuItem("Tools/Scene/OnLateActiveObject")]
    static void OnLateActiveObject()
    {
        Undo.SetCurrentGroupName("OnLateActiveObject");
        int undoGroup = Undo.GetCurrentGroup();

        LateActiveObject[] allObjects = FindObjectsByType<LateActiveObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (LateActiveObject obj in allObjects)
        {
            if (!obj.IsActiveObject)
                continue;

            foreach (Transform child in obj.gameObject.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    Undo.RecordObject(child, "Object on");
                    child.gameObject.SetActive(true);
                }
            }
        }

        Undo.CollapseUndoOperations(undoGroup);
    }
}
