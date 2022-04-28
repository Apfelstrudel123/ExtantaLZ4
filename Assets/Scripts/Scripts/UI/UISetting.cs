using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using System.Collections.Generic;
using Core.GameSettings;

public class UISetting : MonoBehaviour
{
    public enum SettingType
    {
        SliderValue,
        SliderFloat,
        SliderInt,
        Toggle,
        Dropdown
    }

    [HideInInspector] public bool ignoreFirst;
    [HideInInspector] public UnityEvent onChanged;

    [HideInInspector] public string code;
    private Setting setting;

    [HideInInspector] public SettingType settingsType;
    [HideInInspector] public TMP_Text valueText;
    [HideInInspector] public bool localize;
    [HideInInspector] public string[] valueStrings;
    [HideInInspector] public List<int> values;
    [HideInInspector] public Slider slider;
    [HideInInspector] public Toggle toggle;
    [HideInInspector] public TMP_Dropdown dropdown;

    [HideInInspector] public IntSettingObject[] intSettingObjects;
    [HideInInspector] public FloatSettingObject[] floatSettingObjects;
    [HideInInspector] public BoolSettingObject[] boolSettingObjects;

    public void Init()
    {
        setting = Settings.GetSetting(code);
        Get();    
    }

    private void OnEnable()
    {
        Get();
    }

    private string GetText(int value)
    {
        if (localize)
            return Localization.GetLocalisedValue(valueStrings[value]);
        else 
            return valueStrings[value];
    }

    public void Set(float value)
    {
        if (!gameObject.activeInHierarchy || ignoreFirst)
        {
            ignoreFirst = false;
            return;
        }

        int val = (int)value;

        switch (settingsType)
        {
            case SettingType.SliderFloat:
                valueText.text = value.ToString();

                Settings.Set((Setting<float>)setting, value);

                foreach (FloatSettingObject f in floatSettingObjects)
                {
                    if (f.maxShowValue >= value && f.minShowValue <= value)
                    {
                        foreach (GameObject g in f.obj)
                        {
                            g.SetActive(true);
                        }
                    }
                    else
                    {
                        foreach (GameObject g in f.obj)
                        {
                            g.SetActive(false);
                        }
                    }
                }
                break;
            case SettingType.SliderValue:
                valueText.text = GetText(val);
                Settings.Set((Setting<int>)setting, values[val]);

                foreach (IntSettingObject i in intSettingObjects)
                {
                    if (i.showValue.Contains<int>(val))
                    {
                        i.obj.SetActive(true);
                    }
                    else
                    {
                        i.obj.SetActive(false);
                    }
                }
                break;
            case SettingType.SliderInt:
                valueText.text = val.ToString();

                Settings.Set((Setting<int>)setting, val);

                foreach (FloatSettingObject f in floatSettingObjects)
                {
                    if (f.maxShowValue >= val && f.minShowValue <= val)
                    {
                        foreach (GameObject g in f.obj)
                        {
                            g.SetActive(true);
                        }
                    }
                    else
                    {
                        foreach (GameObject g in f.obj)
                        {
                            g.SetActive(false);
                        }
                    }
                }
                break;
        }

        if (onChanged != null)
        {
            onChanged.Invoke();
        }        
    }
    public void Set(bool value)
    {
        if (!gameObject.activeInHierarchy)
        { return; }
        if (ignoreFirst)
        {
            ignoreFirst = false;
            return;
        }

        Settings.Set((Setting<bool>)setting, value);

        if (onChanged != null)
        {
            onChanged.Invoke();
        }

        foreach (BoolSettingObject b in boolSettingObjects)
        {
            if (b.activeOnTrue == value)
            {
                b.obj.SetActive(true);
            }
            else
            {
                b.obj.SetActive(false);
            }
        }
    }
    public void SetDropdown(int value)
    {
        Settings.Set((Setting<int>)setting, value);

        foreach (IntSettingObject i in intSettingObjects)
        {
            if (i.showValue.Contains<int>(value))
            {
                i.obj.SetActive(true);
            }
            else
            {
                i.obj.SetActive(false);
            }
        }
    }

    public void Get()
    {
        if (settingsType == SettingType.SliderValue)
        {

            int value = ((Setting<int>)setting).Get();

            int index = values.IndexOf(value);
            slider.value = index;
            valueText.text = GetText(index);

            foreach (IntSettingObject i in intSettingObjects)
            {
                if (i.showValue.Contains<int>(value))
                {

                    i.obj.SetActive(true);

                }
                else
                {

                    i.obj.SetActive(false);

                }
            }
        }
        else if (settingsType == SettingType.SliderFloat)
        {
            float value = ((Setting<float>)setting).Get();

            slider.value = value;
            valueText.text = (value).ToString();
            foreach (FloatSettingObject f in floatSettingObjects)
            {
                if (f.maxShowValue >= value && f.minShowValue <= value)
                {
                    foreach (GameObject g in f.obj)
                    {
                        g.SetActive(true);
                    }
                }
                else
                {
                    foreach (GameObject g in f.obj)
                    {
                        g.SetActive(false);
                    }
                }
            }
        }
           
        else if (settingsType == SettingType.SliderInt)
        {
            int value = ((Setting<int>)setting).Get();

            slider.value = value;
            valueText.text = (value).ToString();
            foreach (FloatSettingObject f in floatSettingObjects)
            {
                if (f.maxShowValue >= value && f.minShowValue <= value)
                {
                    foreach (GameObject g in f.obj)
                    {
                        g.SetActive(true);
                    }
                }
                else
                {
                    foreach (GameObject g in f.obj)
                    {
                        g.SetActive(false);
                    }
                }
            }
        }
        else if (settingsType == SettingType.Toggle)
        {
            bool value = ((Setting<bool>)setting).Get();

            toggle.isOn = value;
            foreach (BoolSettingObject b in boolSettingObjects)
            {
                if (b.activeOnTrue == value)
                {

                    b.obj.SetActive(true);

                }
                else
                {

                    b.obj.SetActive(false);

                }
            }
        }
        else if (settingsType == SettingType.Dropdown)
        {
            int value = ((Setting<int>)setting).Get();

            dropdown.value = value;
            foreach (IntSettingObject i in intSettingObjects)
            {
                if (i.showValue.Contains<int>(value))
                {

                    i.obj.SetActive(true);

                }
                else
                {

                    i.obj.SetActive(false);

                }
            }
        }
    }
}
[System.Serializable]
public class IntSettingObject
{
    public int[] showValue;
    public GameObject obj;
}
[System.Serializable]
public class BoolSettingObject
{
    public bool activeOnTrue;
    public GameObject obj;
}
[System.Serializable]
public class FloatSettingObject
{
    public float minShowValue;
    public float maxShowValue;
    public GameObject[] obj;
}