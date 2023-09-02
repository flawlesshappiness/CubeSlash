namespace Flawliz.GenericOptions
{
    public class FramerateControl : LeftRightIndexControl<int>
    {
        public override int[] Values => _values;

        private int[] _values = new int[] { 30, 60, 120, 144, int.MaxValue };

        protected override void Awake()
        {
            base.Awake();
            OnIndexChanged += _ => UpdateText();
        }

        private void UpdateText()
        {
            var value = GetSelectedValue();
            var text = value switch
            {
                30 => "30",
                60 => "60",
                120 => "120",
                144 => "144",
                _ => "Uncapped"
            };
            Control.SetText(text);
        }
    }
}