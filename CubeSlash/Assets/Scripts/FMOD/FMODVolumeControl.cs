using Flawliz.GenericOptions;
using FMOD.Studio;
using UnityEngine;

public class FMODVolumeControl : MonoBehaviour
{
    [SerializeField] private VolumeControl _volume_control;
    [SerializeField] private FMODBusType _bus_type;

    private Bus bus;

    private void Start()
    {
        var path = _bus_type == FMODBusType.Master ? "" : _bus_type.ToString();
        bus = FMODController.Instance.GetBus(path);

        _volume_control.OnPercentageChanged += SetVolume;
    }

    private void OnValidate()
    {
        _volume_control ??= GetComponent<VolumeControl>();
    }

    public void SetVolume(float f)
    {
        bus.setVolume(f);
    }
}