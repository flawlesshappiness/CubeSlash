using UnityEngine;

namespace Flawliz.GenericOptions
{
    public class SFXVolumeHandler : GenericOptionsHandler
    {
        [SerializeField] private VolumeControl _volume;

        private void OnValidate()
        {
            _volume ??= GetComponent<VolumeControl>();
        }

        private void Start()
        {
            _volume.SetPercentage(Root.Data.SFXVolume);
            _volume.OnPercentageChanged += v => Root.Data.SFXVolume = v;
            _volume.OnPercentageChanged += v => Root.SetHasChanges();
        }

        protected override void OnDataChanged(OptionsData data)
        {
            base.OnDataChanged(data);
            _volume.SetPercentage(data.SFXVolume);
        }

        public override void RestoreDefault()
        {
            base.RestoreDefault();
            _volume.SetPercentage(1f);
        }
    }
}