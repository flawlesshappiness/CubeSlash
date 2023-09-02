namespace Flawliz.GenericOptions
{
    public class VSyncControl : LeftRightBoolControl
    {
        protected override void Awake()
        {
            base.Awake();
            OnValueChanged += _ => UpdateText();
        }

        private void UpdateText()
        {
            var value = GetSelectedValue();
            var text = value ? "On" : "Off";
            Control.SetText(text);
        }
    }
}