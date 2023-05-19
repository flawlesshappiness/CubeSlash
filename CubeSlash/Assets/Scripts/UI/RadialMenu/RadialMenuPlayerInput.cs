using System;
using UnityEngine;

public class RadialMenuPlayerInput : MonoBehaviour
{
    [SerializeField][Range(0f, 1f)] private float deadzone = 0.25f;
    private RadialMenu menu;

    private bool in_deadzone;

    private void Start()
    {
        menu = GetComponent<RadialMenu>();

        PlayerInput.Controls.UI.Submit.started += _ => menu.BeginSubmit();
    }

    private void Update()
    {
        if (menu == null) return;

        var input = PlayerInput.MoveDirection;
        if (input.magnitude < deadzone)
        {
            if (!in_deadzone)
            {
                in_deadzone = true;
                menu.SetCurrentElement(null);
            }
        }
        else
        {
            in_deadzone = false;
            menu.SelectElement(input);
        }
    }
}