using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Upgrade", menuName = "Game/Upgrade", order = 1)]
public class Upgrade : ScriptableObject
{
    public string id;
    public string name;
    public Sprite icon;
    public Ability.Type ability;
    public List<Effect> effects = new List<Effect>();
    
    [System.Serializable]
    public class Effect
    {
        public enum TypeEffect { POSITIVE, NEGATIVE }
        public TypeEffect type_effect;
        public AbilityVariable variable = new AbilityVariable();
    }
}