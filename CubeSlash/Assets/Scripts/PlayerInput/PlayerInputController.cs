using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public enum ActionType
{
    Submit, Cancel, Dash, Ability, Heal, Pause
}

public class PlayerInputController : Singleton
{
    public static PlayerInputController Instance => Instance<PlayerInputController>();

    public PlayerInput PlayerInput { get; private set; }
    public string CurrentControlScheme => DeviceController.Instance.CurrentDevice == DeviceType.KEYBOARD ? "Keyboard" : "Gamepad";
    public PlayerInputAction Submit { get; private set; } = new PlayerInputAction(nameof(Submit));
    public PlayerInputAction Cancel { get; private set; } = new PlayerInputAction(nameof(Cancel));
    public PlayerInputAction Dash { get; private set; } = new PlayerInputAction(nameof(Dash));
    public PlayerInputAction Ability { get; private set; } = new PlayerInputAction(nameof(Ability));
    public PlayerInputAction Heal { get; private set; } = new PlayerInputAction(nameof(Heal));
    public PlayerInputAction Pause { get; private set; } = new PlayerInputAction(nameof(Pause));
    public PlayerInputAction Debug { get; private set; } = new PlayerInputAction(nameof(Debug));
    public PlayerInputValue<Vector2> Move { get; private set; } = new PlayerInputValue<Vector2>(nameof(Move));
    public PlayerInputValue<float> MouseDelta { get; private set; } = new PlayerInputValue<float>(nameof(MouseDelta));

    protected override void Initialize()
    {
        base.Initialize();

        PlayerInput = EventSystem.current.GetComponent<PlayerInput>();

        InitializeInputActions();
        MouseVibilityUpdate();
    }

    private void InitializeInputActions()
    {
        // Enable all action maps
        foreach (var map in PlayerInput.actions.actionMaps)
        {
            map.Enable();
        }

        // List of PlayerInputAction
        var actions = new List<PlayerInputBase>
        {
            Dash, Ability, Heal, Pause, Debug,
            Move,

            // UI
            Submit, Cancel,
            MouseDelta,
        };

        // Initialize all PlayerInputAction in list
        foreach (var action in actions)
        {
            action.Initialize(PlayerInput);
            action.OnDevice += DeviceController.Instance.SetDevice;
        }
    }

    private Coroutine MouseVibilityUpdate()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var mouse_time = Time.time;
            while (true)
            {
                if (MouseDelta.Value > 0)
                {
                    Cursor.visible = true;
                    mouse_time = Time.time + 3;
                }

                if (Time.time > mouse_time)
                {
                    Cursor.visible = false;
                }

                yield return null;
            }
        }
    }
}

public class PlayerInputBase
{
    public Action<InputDevice> OnDevice;

    public PlayerInputBase(string input_name)
    {
        InputName = input_name;
    }

    public string InputName { get; private set; }
    protected InputAction InputAction { get; private set; }

    public virtual void Initialize(PlayerInput input)
    {
        InputAction = input.actions[InputName];
    }
}

public class PlayerInputAction : PlayerInputBase
{
    public Action Pressed, Released;

    public PlayerInputAction(string input_name) : base(input_name)
    {
    }

    public bool Held { get; private set; }

    public override void Initialize(PlayerInput input)
    {
        base.Initialize(input);
        InputAction.started += ActionStarted;
        InputAction.canceled += ActionEnded;
    }

    private void ActionStarted(InputAction.CallbackContext ctx)
    {
        Held = true;
        Pressed?.Invoke();
        OnDevice?.Invoke(ctx.control.device);
    }

    private void ActionEnded(InputAction.CallbackContext ctx)
    {
        Held = false;
        Released?.Invoke();
    }
}

public class PlayerInputValue<T> : PlayerInputBase
    where T : struct
{
    public PlayerInputValue(string input_name) : base(input_name)
    {
    }

    public T Value => ReadValue();

    private T ReadValue()
    {
        if (InputAction == null)
            return default;

        return InputAction.ReadValue<T>();
    }
}