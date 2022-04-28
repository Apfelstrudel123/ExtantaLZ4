using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(WaterPlane))]
[CanEditMultipleObjects]
public class WaterPlaneEditor : Editor
{
    SerializedProperty depth;
    SerializedProperty fog;
    SerializedProperty _collider;
    SerializedProperty width;
    SerializedProperty length;

    void OnEnable()
    {
        depth = serializedObject.FindProperty("depth");
        fog = serializedObject.FindProperty("fog");
        _collider = serializedObject.FindProperty("_collider");
        width = serializedObject.FindProperty("width");
        length = serializedObject.FindProperty("length");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(fog);
        EditorGUILayout.PropertyField(_collider);
        EditorGUILayout.PropertyField(depth);
        EditorGUILayout.PropertyField(width);
        EditorGUILayout.PropertyField(length);
        serializedObject.ApplyModifiedProperties();

        if(GUILayout.Button("Generate"))
        {
            foreach(GameObject g in Selection.GetFiltered<GameObject>(SelectionMode.Editable))
            {
                if(g.TryGetComponent<WaterPlane>(out WaterPlane w))
                {
                    w.Recalculate();
                }
            }
        }
    }
}