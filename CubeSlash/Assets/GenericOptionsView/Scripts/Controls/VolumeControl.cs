namespace Flawliz.GenericOptions
{
    public class VolumeControl : LeftRightIntControl
    {
        protected override int Min => 0;
        protected override int Max => 100;

        private void Start()
        {
            OnValueChanged += UpdateText;
        }

        private void UpdateText(int i)
        {
            var value = GetSelectedValue();
            Control.SetText($"{value}%");
        }
    }
}