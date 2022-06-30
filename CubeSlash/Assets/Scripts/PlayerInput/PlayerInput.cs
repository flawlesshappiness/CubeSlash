using UnityEngine;
using UnityEngine.InputSystem;

public static class PlayerInput
{
    private static PlayerControls _controls;
    public static PlayerControls Controls { get { return _controls ?? CreateControls(); } }
    public static PlayerInputDatabase _database;
    public static PlayerInputDatabase Database { get { return _database ?? LoadDatabase(); } }

    public static System.Action<ButtonType> OnAbilityButtonDown { get; set; }
    public static System.Action<ButtonType> OnAbilityButtonUp { get; set; }
    public static System.Action<DeviceType> OnDeviceChanged { get; set; }
    public static DeviceType CurrentDevice { get; private set; } = DeviceType.KEYBOARD;
    public static Vector2 MoveDirection { get { return Controls.Player.Move.ReadValue<Vector2>(); } }

    public enum DeviceType
    {
        KEYBOARD, XBOX, PLAYSTATION
    }

    public enum ButtonType
    {
        NORTH, EAST, SOUTH, WEST,
    }

    public enum UIButtonType
    {
        NORTH, EAST, SOUTH, WEST,
        NAV_UP, NAV_RIGHT, NAV_DOWN, NAV_LEFT, NAV_UP_DOWN, NAV_LEFT_RIGHT, NAV_ALL
    }

    private static PlayerControls CreateControls()
    {
        _controls = new PlayerControls();

        _controls.Player.North.started += context => OnAbilityButton(context, ButtonType.NORTH);
        _controls.Player.North.canceled += context => OnAbilityButton(context, ButtonType.NORTH);

        _controls.Player.East.started += context => OnAbilityButton(context, ButtonType.EAST);
        _controls.Player.East.canceled += context => OnAbilityButton(context, ButtonType.EAST);

        _controls.Player.South.started += context => OnAbilityButton(context, ButtonType.SOUTH);
        _controls.Player.South.canceled += context => OnAbilityButton(context, ButtonType.SOUTH);

        _controls.Player.West.started += context => OnAbilityButton(context, ButtonType.WEST);
        _controls.Player.West.canceled += context => OnAbilityButton(context, ButtonType.WEST);

        _controls.Player.Enable();
        _controls.UI.Enable();
        return _controls;
    }

    private static void OnAbilityButton(InputAction.CallbackContext context, ButtonType type)
    {
        OnDeviceInput(context.control.device);

        if (context.started)
        {
            OnAbilityButtonDown?.Invoke(type);
        }
        else if (context.canceled)
        {
            OnAbilityButtonUp?.Invoke(type);
        }
    }

    private static void OnDeviceInput(InputDevice device)
    {
        var type = DeviceType.KEYBOARD;
        if (device.name.ToLower().Contains("xinput"))
        {
            type = DeviceType.XBOX;
        }
        else if (device.name.ToLower().Contains("dualshock"))
        {
            type = DeviceType.PLAYSTATION;
        }

        if(type != CurrentDevice)
        {
            CurrentDevice = type;
            OnDeviceChanged?.Invoke(type);
            Debug.Log($"Current device changed to {CurrentDevice}");
        }
    }

    private static PlayerInputDatabase LoadDatabase()
    {
        _database = Resources.Load<PlayerInputDatabase>("Databases/PlayerInputDatabase");
        return _database;
    }
}