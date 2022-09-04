using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PauseView : View
{
    public static bool Exists { get; private set; }

    [SerializeField] private Button btnContinue;
    [SerializeField] private Button btnOptions;
    [SerializeField] private Button btnMainMenu;

    private void OnEnable()
    {
        Exists = true;
        GameController.Instance.PauseLock.AddLock(nameof(PauseView));
    }

    private void OnDisable()
    {
        Exists = false;
        GameController.Instance.PauseLock.RemoveLock(nameof(PauseView));
    }

    private void Start()
    {
        btnContinue.onClick.AddListener(ClickContinue);
        btnOptions.onClick.AddListener(ClickOptions);
        btnMainMenu.onClick.AddListener(ClickMainMenu);

        EventSystemController.Instance.SetDefaultSelection(btnContinue.gameObject);
    }

    private void ClickContinue()
    {
        Close(0);
    }

    private void ClickOptions()
    {

    }

    private void ClickMainMenu()
    {
        Close(0);
        GameController.Instance.ReturnToMainMenu();
    }
}