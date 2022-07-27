using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartView : View
{
    [SerializeField] private Button btn_play;
    [SerializeField] private Button btn_options;
    [SerializeField] private Button btn_quit;

    private void Start()
    {
        btn_play.onClick.AddListener(ClickPlay);
    }

    private void ClickPlay()
    {
        GameController.Instance.StartGame();
    }
}