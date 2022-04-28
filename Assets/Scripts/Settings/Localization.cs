using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
public class CSVLoader
{
    private TextAsset file;
    private char lineSeperator = '\n';
    private char surround = '"';
    private string[] fieldSeperator = { "\",\"" };

    public void LoadCSV()
    {
        file = Resources.Load<TextAsset>("localization");
    }

    public Dictionary<string, string> GetDictionaryValues(string attributeID)
    {
        Dictionary<string, string> dictionary = new Dictionary<string, string>();

        string[] lines = file.text.Split(lineSeperator);

        int attributeIndex = -1;
        string[] headers = lines[0].Split(fieldSeperator, StringSplitOptions.None);

        for (int i = 0; i < headers.Length; i++)
        {
            if (headers[i].Contains(attributeID))
            {
                attributeIndex = i;
                break;
            }
        }

        Regex csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];

            string[] fields = csvParser.Split(line);

            for (int f = 0; f < fields.Length; f++)
            {
                fields[f] = fields[f].TrimStart(' ', surround);
                fields[f] = fields[f].TrimEnd(surround);
            }

            if (fields.Length > attributeIndex)
            {
                var key = fields[0];

                if (dictionary.ContainsKey(key)) { continue; }

                var value = fields[attributeIndex];
                dictionary.Add(key, value);
            }
        }
        return dictionary;
    }
#if UNITY_EDITOR
    public void Add(string key, string value)
    {
        string appended = string.Format("\n\"{0}\",\"{1}\",\"\"", key, value);

        File.AppendAllText("Assets/Data/Resources/localization.csv", appended);
        UnityEditor.AssetDatabase.Refresh();
    }

    public void Remove(string key)
    {
        string[] lines = file.text.Split(lineSeperator);

        string[] keys = new string[lines.Length];

        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            keys[i] = line.Split(fieldSeperator, StringSplitOptions.None)[0];
        }

        int index = -1;

        for (int i = 0; i < keys.Length; i++)
        {
            if (keys[i].Contains(key))
            {
                index = i;
                break;
            }
        }

        if (index > -1)
        {
            string[] newLines = lines.Where(w => w != lines[index]).ToArray();
            string replaced = string.Join(lineSeperator.ToString(), newLines);
            File.WriteAllText("Assets/Data/Resources/localization.csv", replaced);
        }
    }

    public void Edit(string key, string value)
    {
        Remove(key);
        Add(key, value);
    }
#endif
}

public static class Localization
{
    public enum Language
    {
        en,
        ger,
    }

    public static Language language = Language.en;

    private static Dictionary<string, string>[] localised = new Dictionary<string, string>[2];

    public static bool isInit;
    public static CSVLoader csvLoader;

    public static void Init()
    {
        csvLoader = new CSVLoader();
        csvLoader.LoadCSV();

        UpdateDictionaries();

        isInit = true;
    }

    public static void UpdateDictionaries()
    {
        for (int i = 0; i < 2; i++)
        {
            localised[i] = csvLoader.GetDictionaryValues(((Language)i).ToString());
        }
    }

    public static Dictionary<string, string> GetEdiotrDictionary()
    {
        if (!isInit) { Init(); }
        return localised[(int)Language.en];
    }

    public static string GetLocalisedValue(string key)
    {
        if (!isInit) { Init(); }

        localised[(int)language].TryGetValue(key, out string value);
        return value;
    }
#if UNITY_EDITOR
    public static void Add(string key, string value)
    {
        if (value.Contains("\""))
        {
            value.Replace('"', '\"');
        }

        if (csvLoader == null)
        {
            csvLoader = new CSVLoader();
        }

        csvLoader.LoadCSV();
        csvLoader.Add(key, value);
        csvLoader.LoadCSV();
        UpdateDictionaries();
    }

    public static void Edit(string key, string value)
    {
        if (value.Contains("\""))
        {
            value.Replace('"', '\"');
        }

        if (csvLoader == null)
        {
            csvLoader = new CSVLoader();
        }

        csvLoader.LoadCSV();
        csvLoader.Edit(key, value);
        csvLoader.LoadCSV();
        UpdateDictionaries();
    }

    public static void Remove(string key)
    {
        if (csvLoader == null)
        {
            csvLoader = new CSVLoader();
        }

        csvLoader.LoadCSV();
        csvLoader.Remove(key);
        csvLoader.LoadCSV();
        UpdateDictionaries();
    }
#endif
}

[Serializable]
public struct LocalisedString
{
    public string key;
    public LocalisedString(string key)
    {
        this.key = key;
    }

    public string value
    {
        get
        {
            return Localization.GetLocalisedValue(key);
        }
    }

    public static implicit operator LocalisedString(string key)
    {
        return new LocalisedString(key);
    }
}