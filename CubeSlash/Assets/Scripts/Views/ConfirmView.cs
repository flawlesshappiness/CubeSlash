using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmView : View
{
    [SerializeField] private TMP_Text tmp_text;
    [SerializeField] private SelectableMenuItem btn_confirm, btn_cancel;
    [SerializeField] private RectTransform panel_normal, panel_wrong;

    public event System.Action onConfirm, onCancel;

    public string Text { set { tmp_text.text = value; } }

    private RectTransform active_panel;

    public static ConfirmView Show(string text, System.Action onConfirm, System.Action onCancel = null)
    {
        var view = ViewController.Instance.ShowView<ConfirmView>(0, nameof(ConfirmView));
        view.onConfirm = onConfirm;
        view.onCancel = onCancel;

        view.Text = text;
        return view;
    }

    private void Start()
    {
        btn_confirm.onSubmit += ClickConfirm;
        btn_cancel.onSubmit += ClickCancel;

        if(active_panel == null)
        {
            SetPanel(panel_normal);
        }
    }

    private void ClickConfirm()
    {
        onConfirm?.Invoke();
        Close(0);
    }

    private void ClickCancel()
    {
        onCancel?.Invoke();
        Close(0);
    }

    public void SetWrongPanel()
    {
        SetPanel(panel_wrong);
    }

    private void SetPanel(RectTransform panel)
    {
        panel_normal.gameObject.SetActive(false);
        panel_wrong.gameObject.SetActive(false);
        active_panel = panel;
        active_panel.gameObject.SetActive(true);
    }
}