using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DeviceController : Singleton
{
    public static DeviceController Instance => Instance<DeviceController>();
    public static DeviceType CurrentDevice { get; private set; } = DeviceType.KEYBOARD;
    public static System.Action<DeviceType> OnDeviceChanged { get; set; }
    public static System.Action<DeviceType> OnCurrentDeviceLost { get; set; }

    protected override void Initialize()
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

    private static void SetDevice(InputDevice device)
    {
        var type = GetDeviceType(device.name);
        CurrentDevice = type;
        OnDeviceChanged?.Invoke(type);
        Debug.Log($"Current device changed to {CurrentDevice}");
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
}

public enum DeviceType
{
    KEYBOARD, XBOX, PLAYSTATION
}