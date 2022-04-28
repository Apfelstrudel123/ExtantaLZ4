using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
[CustomEditor(typeof(UISetting))]
[CanEditMultipleObjects]
public class SettingInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (targets.Length == 1)
        {
            UISetting set = (UISetting)target;
            SerializedObject obj = new SerializedObject(set);

            EditorGUILayout.PropertyField(obj.FindProperty("code"));

            EditorGUILayout.PropertyField(obj.FindProperty("ignoreFirst"));
            EditorGUILayout.PropertyField(obj.FindProperty("onChanged"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(obj.FindProperty("settingsType"));

            if (set.settingsType == UISetting.SettingType.SliderValue)
            {
                EditorGUILayout.PropertyField(obj.FindProperty("slider"));
                EditorGUILayout.PropertyField(obj.FindProperty("valueText"));
                EditorGUILayout.PropertyField(obj.FindProperty("valueStrings"));
                EditorGUILayout.PropertyField(obj.FindProperty("localize"));
                EditorGUILayout.PropertyField(obj.FindProperty("values"));
                EditorGUILayout.PropertyField(obj.FindProperty("intSettingObjects"));
                Slider s = set.slider;
                if(s != null)
                {
                    s.maxValue = set.values.Count-1;
                }
            }
            else if (set.settingsType == UISetting.SettingType.SliderFloat || set.settingsType == UISetting.SettingType.SliderInt)
            {
                EditorGUILayout.PropertyField(obj.FindProperty("slider"));
                EditorGUILayout.PropertyField(obj.FindProperty("valueText"));
                EditorGUILayout.PropertyField(obj.FindProperty("floatSettingObjects"));
            }
            else if (set.settingsType == UISetting.SettingType.Toggle)
            {
                EditorGUILayout.PropertyField(obj.FindProperty("toggle"));
                EditorGUILayout.PropertyField(obj.FindProperty("boolSettingObjects"));
            }
            else if (set.settingsType == UISetting.SettingType.Dropdown)
            {
                EditorGUILayout.PropertyField(obj.FindProperty("dropdown"));
                EditorGUILayout.PropertyField(obj.FindProperty("intSettingObjects"));
            }

            obj.ApplyModifiedProperties();
        }
        else
        {
            UISetting[] set = new UISetting[targets.Length];
            targets.CopyTo(set, 0);
            SerializedObject obj = new SerializedObject(set);

            EditorGUILayout.PropertyField(obj.FindProperty("category"));
            EditorGUILayout.PropertyField(obj.FindProperty("property"));
            
            EditorGUILayout.PropertyField(obj.FindProperty("ignoreFirst"));
            EditorGUILayout.PropertyField(obj.FindProperty("onChanged"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(obj.FindProperty("settingsType"));

            UISetting.SettingType ty = set[0].settingsType;
            int c = 1;
            for (int i = 1; i < set.Length; i++)
            {
                if (set[i].settingsType != ty)
                {
                    break;
                }
                c++;
            }
            if(c == set.Length)
            {
                if (ty == UISetting.SettingType.SliderValue)
                {
                    EditorGUILayout.PropertyField(obj.FindProperty("slider"));
                    EditorGUILayout.PropertyField(obj.FindProperty("valueText"));
                    EditorGUILayout.PropertyField(obj.FindProperty("valueStrings"));
                    EditorGUILayout.PropertyField(obj.FindProperty("localize"));
                    EditorGUILayout.PropertyField(obj.FindProperty("values"));
                }
                else if (ty == UISetting.SettingType.SliderFloat)
                {
                    EditorGUILayout.PropertyField(obj.FindProperty("slider"));
                    EditorGUILayout.PropertyField(obj.FindProperty("valueText"));
                }
                else if (ty == UISetting.SettingType.Toggle)
                {
                    EditorGUILayout.PropertyField(obj.FindProperty("toggle"));
                }
                else if (ty == UISetting.SettingType.Dropdown)
                {
                    EditorGUILayout.PropertyField(obj.FindProperty("dropdown"));
                }
            }

            obj.ApplyModifiedProperties();
        }
    }
}
[CustomPropertyDrawer(typeof(IntSettingObject))]
public class IntSettingObjectDrawer : PropertyDrawer
{
    private int count;
    private int[] values;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        count = property.FindPropertyRelative("showValue").arraySize;
        values = new int[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = property.FindPropertyRelative("showValue").GetArrayElementAtIndex(i).intValue;
        }

        count = EditorGUILayout.IntField("Value Count",count);
        int[] v = values;
        values = new int[count];
        for(int i = 0; i < count; i++)
        {
            if (v.Length > i)
            {
                values[i] = v[i];
            }
            values[i] = EditorGUILayout.IntField("Value " + i, values[i]);
        }

        property.FindPropertyRelative("showValue").arraySize = count;
        for (int i = 0; i < count; i++)
        {
            property.FindPropertyRelative("showValue").GetArrayElementAtIndex(i).intValue = values[i];
        }

        EditorGUILayout.PropertyField(property.FindPropertyRelative("obj"));

        EditorGUI.EndProperty();
    }
}
[CustomPropertyDrawer(typeof(BoolSettingObject))]
public class BoolSettingObjectDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        EditorGUILayout.PropertyField(property.FindPropertyRelative("activeOnTrue"));
        EditorGUILayout.PropertyField(property.FindPropertyRelative("obj"));

        EditorGUI.EndProperty();
    }
}