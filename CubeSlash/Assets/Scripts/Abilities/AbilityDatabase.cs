using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Game/AbilityDatabase", order = 1)]
public class AbilityDatabase : ScriptableObject
{
    public List<Ability> abilities = new List<Ability>();
    public Ability GetAbility(Ability.Type type) => abilities.FirstOrDefault(a => a.Info.type == type);

    public static AbilityDatabase LoadAsset()
    {
        AbilityDatabase db = null;
#if UNITY_EDITOR
        db = AssetDatabase.LoadAssetAtPath<AbilityDatabase>($"Assets/Resources/Databases/{nameof(AbilityDatabase)}.asset");
#endif
        return db;
    }
}