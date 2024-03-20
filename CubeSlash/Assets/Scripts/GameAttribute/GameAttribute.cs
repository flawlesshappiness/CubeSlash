using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class GameAttribute
{
    public GameAttributeType type = new GameAttributeType();
    public GameAttributeValue base_value = new GameAttributeValue();

    public string text;
    public bool high_is_negative;
    public bool display_as_percentage;

    public System.Action OnValueModified;

    private GameAttributeValue _modified_value;
    private List<GameAttributeModifier> _modifiers = new List<GameAttributeModifier>();

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
        att._modifiers = _modifiers.ToList();
        att.display_as_percentage = display_as_percentage;
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
        var modifier_sum = new GameAttributeModifier();
        _modifiers.ForEach(m => modifier_sum.Add(m));

        _modified_value = new GameAttributeValue(base_value);
        _modified_value.display_as_percentage = display_as_percentage;
        modifier_sum.Modify(_modified_value);
        _is_updated_after_modified = true;
    }

    public void AddModifier(GameAttributeModifier modifier)
    {
        _modifiers.Add(modifier);
        _is_updated_after_modified = false;
        OnValueModified?.Invoke();
    }

    public void RemoveModifier(GameAttributeModifier modifier)
    {
        _modifiers.Remove(modifier);
        _is_updated_after_modified = false;
        OnValueModified?.Invoke();
    }
}