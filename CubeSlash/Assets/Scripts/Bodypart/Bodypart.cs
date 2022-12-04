using UnityEngine;

public class Bodypart : MonoBehaviour
{
    [Range(0,1)]
    public float priority_position;

    protected Ability Ability { get; private set; }

    public virtual void Initialize(Ability ability)
    {
        this.Ability = ability;
        ability.onTrigger += OnAbilityTrigger;
    }

    private void OnDisable()
    {
        Ability.onTrigger -= OnAbilityTrigger;
    }

    protected virtual void OnAbilityTrigger()
    {

    }
}