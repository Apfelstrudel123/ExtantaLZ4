using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    [SerializeField] private GameObject content;
    public void Open()
    {
        content.SetActive(true);
    }

    public void Close()
    {
        content.SetActive(false);
    }
}