using UnityEngine;
using UnityEngine.UI;

public class UIAbilityVariablePoint : MonoBehaviour
{
    [SerializeField] private GameObject g_filled;
    [SerializeField] private GameObject g_empty;
    [SerializeField] private GameObject g_disabled;
    public bool Filled { set { g_filled.SetActive(value); g_empty.SetActive(!value); } }
    public bool Disabled { set { g_disabled.SetActive(value); } }
}