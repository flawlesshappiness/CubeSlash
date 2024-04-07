using Flawliz.GenericOptions;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindUIHandler : GenericOptionsHandler
{
    [SerializeField] private InputActionReference input_action;

    public override void RestoreDefault()
    {
        base.RestoreDefault();
        input_action.action.RemoveAllBindingOverrides();
    }
}