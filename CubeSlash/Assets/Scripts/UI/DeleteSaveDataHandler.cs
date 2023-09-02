using Flawliz.GenericOptions;
using UnityEngine;

public class DeleteSaveDataHandler : GenericOptionsHandler
{
    [SerializeField] private ButtonControl _control;

    private void OnValidate()
    {
        _control ??= GetComponentInChildren<ButtonControl>();
    }

    private void Start()
    {
        _control.OnSubmitEvent += Click;
    }

    private void Click()
    {
        Root.ShowConfirmWindow(
            "Delete save data?",
            "Yes",
            "No",
            DeleteSaveData,
            null);
    }

    private void DeleteSaveData()
    {
        SaveDataController.Instance.ClearSaveData();
        Player.Instance.Clear();
    }
}