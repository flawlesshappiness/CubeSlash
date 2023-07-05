[System.Serializable]
public class GameAttribute
{
    public GameAttributeType type = new GameAttributeType();
    public GameAttributeValue base_value = new GameAttributeValue();

    public string text;
    public bool high_is_negative;

    public System.Action OnValueModified;

    private GameAttributeValue _modified_value;
    private GameAttributeModifier _modifier = new GameAttributeModifier();

    private bool _is_updated_after_modified;

    public GameAttributeValue ModifiedValue { get { return GetModifiedValue(); } }

    public GameAttribute()
    {

    }

    public GameAttribute Clone()
    {
        var att = new GameAttribute();
        att.type = type;
        att.base_value = base_value;
        att.text = text;
        att.high_is_negative = high_is_negative;
        att._modifier = _modifier.Clone();
        return att;
    }

    public GameAttributeValue GetModifiedValue()
    {
        if (!_is_updated_after_modified)
        {
            UpdateModifiedValue();
        }

        return _modified_value;
    }

    private void UpdateModifiedValue()
    {
        _modified_value = new GameAttributeValue(base_value);
        _modifier.Modify(_modified_value);
        _is_updated_after_modified = true;
    }

    public void AddModifier(GameAttributeModifier modifier)
    {
        _modifier.Add(modifier);
        _is_updated_after_modified = false;
        OnValueModified?.Invoke();
    }
}