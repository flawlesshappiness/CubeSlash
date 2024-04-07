using UnityEngine;
using UnityEngine.InputSystem;

public class RebindLoader : MonoBehaviour
{
    [SerializeField] private InputActionAsset asset;

    private void Start()
    {
        var data = RebindData.Data;

        foreach (var rebind in data.Rebinds)
        {
            var id = rebind.Key;
            var path = rebind.Value;
            var action = asset.FindAction(id);
            if (action == null) continue;

            action.ApplyBindingOverride(path);
        }
    }
}