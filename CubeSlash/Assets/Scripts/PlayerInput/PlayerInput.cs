using System.Linq;
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
    public static System.Action<DeviceType> OnCurrentDeviceLost { get; set; }
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
        NAV_UP, NAV_RIGHT, NAV_DOWN, NAV_LEFT, NAV_UP_DOWN, NAV_LEFT_RIGHT, NAV_ALL,
    }

    public static void Initialize()
    {
        InputSystem.onDeviceChange += (device, change) =>
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                    OnDeviceAdded(device);
                    break;

                case InputDeviceChange.Disconnected:
                    OnDeviceDisconnected(device);
                    break;

                case InputDeviceChange.Reconnected:
                    OnDeviceReconnected(device);
                    break;

                case InputDeviceChange.Removed:
                    OnDeviceRemoved(device);
                    break;

                default:

                    break;
            }
        };
    }

    private static void OnDeviceAdded(InputDevice device)
    {
        DeviceConnected(device);
    }

    private static void OnDeviceReconnected(InputDevice device)
    {
        DeviceConnected(device);
    }

    private static void DeviceConnected(InputDevice device)
    {
        LogController.LogMethod(device.name);
        SetDevice(device);
    }

    private static void OnDeviceDisconnected(InputDevice device)
    {
        DeviceLost(device);
    }

    private static void OnDeviceRemoved(InputDevice device)
    {
        DeviceLost(device);
    }

    private static void DeviceLost(InputDevice device)
    {
        LogController.LogMethod(device.name);
        var type = GetDeviceType(device.name);
        if (type == CurrentDevice)
        {
            OnCurrentDeviceLost?.Invoke(CurrentDevice);
        }

        // Find new valid device
        var devices = InputSystem.devices;
        var latestDevice = devices.OrderByDescending(d => d.lastUpdateTime).FirstOrDefault();
        if (latestDevice != null)
        {
            SetDevice(latestDevice);
        }
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

        _controls.Player.Menu.started += context => GameController.Instance.OpenPauseView();

        _controls.Player.Home.started += context => GameController.Instance.HomeButtonPressed();

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
        var type = GetDeviceType(device.name);
        //Debug.Log($"{device.name}: {type}");
        if (type != CurrentDevice)
        {
            SetDevice(device);
        }
    }

    private static void SetDevice(InputDevice device)
    {
        var type = GetDeviceType(device.name);
        CurrentDevice = type;
        OnDeviceChanged?.Invoke(type);
        Debug.Log($"Current device changed to {CurrentDevice}");
    }

    private static PlayerInputDatabase LoadDatabase()
    {
        _database = Resources.Load<PlayerInputDatabase>("Databases/PlayerInputDatabase");
        return _database;
    }

    private static DeviceType GetDeviceType(string name)
    {
        name = name.ToLower();
        if (name.Contains("xinput"))
        {
            return DeviceType.XBOX;
        }
        else if (name.Contains("dualshock"))
        {
            return DeviceType.PLAYSTATION;
        }

        return DeviceType.KEYBOARD;
    }

    public static UIButtonType ButtonToUI(ButtonType type)
    {
        return type switch
        {
            ButtonType.NORTH => UIButtonType.NORTH,
            ButtonType.SOUTH => UIButtonType.SOUTH,
            ButtonType.EAST => UIButtonType.EAST,
            ButtonType.WEST => UIButtonType.WEST,
            _ => UIButtonType.NORTH
        };
    }
}