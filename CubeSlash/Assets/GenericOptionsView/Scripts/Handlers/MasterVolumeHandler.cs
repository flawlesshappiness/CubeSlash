using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class MasterVolumeHandler : GenericOptionsHandler
    {
        [SerializeField] private VolumeControl _volume;

        private void OnValidate()
        {
            _volume ??= GetComponent<VolumeControl>();
        }

        private void Start()
        {
            _volume.SetPercentage(Root.Data.MasterVolume);
            _volume.OnPercentageChanged += v => Root.Data.MasterVolume = v;
            _volume.OnPercentageChanged += v => Root.SetHasChanges();
        }

        protected override void OnDataChanged(OptionsData data)
        {
            base.OnDataChanged(data);
            _volume.SetPercentage(data.MasterVolume);
        }

        public override void RestoreDefault()
        {
            base.RestoreDefault();
            _volume.SetPercentage(0.5f);
        }
    }
}