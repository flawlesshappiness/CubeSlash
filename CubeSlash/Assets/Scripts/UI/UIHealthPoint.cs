using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHealthPoint : MonoBehaviour
{
    [SerializeField] private GameObject g_full;
    [SerializeField] private GameObject g_empty;

    public bool Full { get { return g_full.activeInHierarchy; } set { g_full.SetActive(value); g_empty.SetActive(!value); } }
}
