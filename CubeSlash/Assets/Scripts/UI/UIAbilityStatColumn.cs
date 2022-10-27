using System.Collections.Generic;
using UnityEngine;

public class UIAbilityStatColumn : MonoBehaviour
{
    [SerializeField] private UIAbilityStatVariable template_variable;

    private List<UIAbilityStatVariable> variables = new List<UIAbilityStatVariable>();

    private void Start()
    {
        template_variable.gameObject.SetActive(false);
    }

    public UIAbilityStatVariable CreateVariable()
    {
        var v = Instantiate(template_variable, template_variable.transform.parent);
        v.gameObject.SetActive(true);
        variables.Add(v);
        return v;
    }

    public void CreateSpace()
    {
        var v = CreateVariable();
        v.DividerEnabled = false;
    }

    public UIAbilityStatVariable GetVariable(int idx) => variables[idx];
}