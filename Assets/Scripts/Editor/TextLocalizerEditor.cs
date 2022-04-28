using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextLocalizerEditWindow : EditorWindow
{
    public static void Open(string key)
    {
        TextLocalizerEditWindow window = new TextLocalizerEditWindow();
        window.titleContent = new GUIContent("Localization");
        window.ShowUtility();
        window.key = key;
    }

    public string key;
    public string value;

    public void OnGUI()
    {
        key = EditorGUILayout.TextField("Key :", key);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Value:", GUILayout.MaxWidth(50));

        EditorStyles.textArea.wordWrap = true;
        value = EditorGUILayout.TextArea(value, EditorStyles.textArea, GUILayout.Height(100), GUILayout.Width(400));
        EditorGUILayout.EndHorizontal();

        if(GUILayout.Button("Add"))
        {
            if(Localization.GetLocalisedValue(key) != string.Empty)
            {
                Localization.Edit(key, value);
            }
            else
            {
                Localization.Add(key, value);
            }
        }

        minSize = new Vector2(460, 250);
        maxSize = minSize;
    }
}
public class TextLocalizerSearchWindow : EditorWindow
{
    public static void Open()
    {
        TextLocalizerSearchWindow window = new TextLocalizerSearchWindow();
        window.titleContent = new GUIContent("Localization Search");

        Vector2 mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
        Rect r = new Rect(mouse.x - 450, mouse.y + 10, 10, 10);
        window.ShowAsDropDown(r, new Vector2(500, 300));
    }

    public string value;
    public Vector2 scroll;
    public Dictionary<string, string> dictionary;

    private void OnEnable()
    {
        dictionary = Localization.GetEdiotrDictionary();
    }

    public void OnGUI()
    {
        EditorGUILayout.BeginHorizontal("Box");
        EditorGUILayout.LabelField("Search :", EditorStyles.boldLabel);
        value = EditorGUILayout.TextField(value);
        EditorGUILayout.EndHorizontal();

        GetSearchResults();
    }

    private void GetSearchResults()
    {
        if(value == null) { return; }

        EditorGUILayout.BeginVertical();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach(KeyValuePair<string, string> element in dictionary)
        {
            if(element.Key.ToLower().Contains(value.ToLower()) || element.Value.ToLower().Contains(value.ToLower()))
            {
                EditorGUILayout.BeginHorizontal("box");
                Texture closeIcon = (Texture)Resources.Load("CloseIcon");

                GUIContent content = new GUIContent(closeIcon);

                if(GUILayout.Button(content, GUILayout.MaxWidth(20), GUILayout.MaxHeight(20)))
                {
                    if(EditorUtility.DisplayDialog("Key entfernen" + element.Key + "?", "Nicht rückgängig", "Ja"))
                    {
                        Localization.Remove(element.Key);
                        AssetDatabase.Refresh();
                        Localization.Init();
                        dictionary = Localization.GetEdiotrDictionary();
                    }
                }

                EditorGUILayout.TextField(element.Key);
                EditorGUILayout.LabelField(element.Value);
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
}