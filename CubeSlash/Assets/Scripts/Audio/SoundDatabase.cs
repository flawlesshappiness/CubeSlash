using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FMODEventReferenceDatabase", menuName = "Game/FMODEventReferenceDatabase", order = 1)]
public class SoundDatabase : Database<SoundEffectEntry>
{
    private static SoundDatabase Load() => Load<SoundDatabase>();

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        var types = FakeEnum.GetAll(typeof(SoundEffectType));
        foreach (var type in types)
        {
            var entry = collection.FirstOrDefault(entry => entry.type == type);
            if (entry == null)
            {
                collection.Add(new SoundEffectEntry
                {
                    type = type as SoundEffectType,
                    sfx = new FMODEventReference()
                });
            }
        }

        foreach (var entry in collection)
        {
            if (entry.type != null)
            {
                entry.name = entry.type.ToString();
                if (string.IsNullOrEmpty(entry.sfx.reference.Path))
                {
                    entry.name += " [EMPTY]";
                }
            }
        }
    }
#endif

    public static SoundEffectEntry GetEntry(SoundEffectType type)
    {
        var db = Load();
        var entry = db.collection.FirstOrDefault(entry => entry.type == type);
        return entry;
    }
}

[System.Serializable]
public class SoundEffectEntry
{
    [HideInInspector] public string name;
    public SoundEffectType type;
    public FMODEventReference sfx;
}

[System.Serializable]
public class SoundEffectType : FakeEnum
{
    public SoundEffectType(string id) : base(id) { }

    // Misc
    public static readonly SoundEffectType sfx_gain_health = new SoundEffectType(nameof(sfx_gain_health));
    public static readonly SoundEffectType sfx_chain_zap = new SoundEffectType(nameof(sfx_chain_zap));
    public static readonly SoundEffectType sfx_collect_experience = new SoundEffectType(nameof(sfx_collect_experience));
    public static readonly SoundEffectType sfx_explode_charge = new SoundEffectType(nameof(sfx_explode_charge));
    public static readonly SoundEffectType sfx_explode_release = new SoundEffectType(nameof(sfx_explode_release));
    public static readonly SoundEffectType sfx_player_damage = new SoundEffectType(nameof(sfx_player_damage));
    public static readonly SoundEffectType sfx_ability_cooldown = new SoundEffectType(nameof(sfx_ability_cooldown));
    public static readonly SoundEffectType sfx_dud_death = new SoundEffectType(nameof(sfx_dud_death));
    public static readonly SoundEffectType sfx_avoid_damage = new SoundEffectType(nameof(sfx_avoid_damage));
    public static readonly SoundEffectType sfx_ability_off_cooldown = new SoundEffectType(nameof(sfx_ability_off_cooldown));

    // Charge
    public static readonly SoundEffectType sfx_charge_start = new SoundEffectType(nameof(sfx_charge_start));
    public static readonly SoundEffectType sfx_charge_idle = new SoundEffectType(nameof(sfx_charge_idle));
    public static readonly SoundEffectType sfx_charge_shoot = new SoundEffectType(nameof(sfx_charge_shoot));

    // Dash
    public static readonly SoundEffectType sfx_dash_start = new SoundEffectType(nameof(sfx_dash_start));
    public static readonly SoundEffectType sfx_dash_impact = new SoundEffectType(nameof(sfx_dash_impact));
    public static readonly SoundEffectType sfx_dash_projectile = new SoundEffectType(nameof(sfx_dash_projectile));

    // Mines
    public static readonly SoundEffectType sfx_mines_spawn = new SoundEffectType(nameof(sfx_mines_spawn));
    public static readonly SoundEffectType sfx_mines_explode = new SoundEffectType(nameof(sfx_mines_explode));

    // Split
    public static readonly SoundEffectType sfx_split_shoot = new SoundEffectType(nameof(sfx_split_shoot));

    // Boomerang
    public static readonly SoundEffectType sfx_boomerang_shoot = new SoundEffectType(nameof(sfx_boomerang_shoot));
    public static readonly SoundEffectType sfx_boomerang_catch = new SoundEffectType(nameof(sfx_boomerang_catch));

    // Enemy
    public static readonly SoundEffectType sfx_enemy_bone_teleport_appear = new SoundEffectType(nameof(sfx_enemy_bone_teleport_appear));
    public static readonly SoundEffectType sfx_enemy_bone_teleport_disappear = new SoundEffectType(nameof(sfx_enemy_bone_teleport_disappear));
    public static readonly SoundEffectType sfx_enemy_bone_wall_appear = new SoundEffectType(nameof(sfx_enemy_bone_wall_appear));
    public static readonly SoundEffectType sfx_enemy_bone_wall_disappear = new SoundEffectType(nameof(sfx_enemy_bone_wall_disappear));
    public static readonly SoundEffectType sfx_enemy_shoot = new SoundEffectType(nameof(sfx_enemy_shoot));
    public static readonly SoundEffectType sfx_enemy_volatile_warning = new SoundEffectType(nameof(sfx_enemy_volatile_warning));
    public static readonly SoundEffectType sfx_enemy_angler_charge_short = new SoundEffectType(nameof(sfx_enemy_angler_charge_short));
    public static readonly SoundEffectType sfx_enemy_angler_charge_long = new SoundEffectType(nameof(sfx_enemy_angler_charge_long));
    public static readonly SoundEffectType sfx_enemy_angler_bite = new SoundEffectType(nameof(sfx_enemy_angler_bite));
    public static readonly SoundEffectType sfx_enemy_plant_warn = new SoundEffectType(nameof(sfx_enemy_plant_warn));
    public static readonly SoundEffectType sfx_enemy_plant_spawn = new SoundEffectType(nameof(sfx_enemy_plant_spawn));
    public static readonly SoundEffectType sfx_enemy_boss_proximity = new SoundEffectType(nameof(sfx_enemy_boss_proximity));
    public static readonly SoundEffectType sfx_enemy_death = new SoundEffectType(nameof(sfx_enemy_death));
    public static readonly SoundEffectType sfx_enemy_boss_scream = new SoundEffectType(nameof(sfx_enemy_boss_scream));
    public static readonly SoundEffectType sfx_enemy_crystal_shield = new SoundEffectType(nameof(sfx_enemy_crystal_shield));
    public static readonly SoundEffectType sfx_enemy_crystal_unshield = new SoundEffectType(nameof(sfx_enemy_crystal_unshield));
    public static readonly SoundEffectType sfx_enemy_crystal_break = new SoundEffectType(nameof(sfx_enemy_crystal_break));
    public static readonly SoundEffectType sfx_enemy_root = new SoundEffectType(nameof(sfx_enemy_root));
    public static readonly SoundEffectType sfx_enemy_maw_attack = new SoundEffectType(nameof(sfx_enemy_maw_attack));

    // UI
    public static readonly SoundEffectType sfx_ui_level_up = new SoundEffectType(nameof(sfx_ui_level_up));
    public static readonly SoundEffectType sfx_ui_move = new SoundEffectType(nameof(sfx_ui_move));
    public static readonly SoundEffectType sfx_ui_submit = new SoundEffectType(nameof(sfx_ui_submit));
    public static readonly SoundEffectType sfx_ui_unlock_upgrade = new SoundEffectType(nameof(sfx_ui_unlock_upgrade));
    public static readonly SoundEffectType sfx_ui_unlock_ability = new SoundEffectType(nameof(sfx_ui_unlock_ability));
    public static readonly SoundEffectType sfx_ui_refund = new SoundEffectType(nameof(sfx_ui_refund));
    public static readonly SoundEffectType sfx_ui_refund_hold = new SoundEffectType(nameof(sfx_ui_refund_hold));
    public static readonly SoundEffectType sfx_ui_tally = new SoundEffectType(nameof(sfx_ui_tally));
    public static readonly SoundEffectType sfx_ui_stats_appear = new SoundEffectType(nameof(sfx_ui_stats_appear));
    public static readonly SoundEffectType sfx_ui_marima_001 = new SoundEffectType(nameof(sfx_ui_marima_001));
    public static readonly SoundEffectType sfx_ui_marima_002 = new SoundEffectType(nameof(sfx_ui_marima_002));
    public static readonly SoundEffectType sfx_ui_fadiano_001 = new SoundEffectType(nameof(sfx_ui_fadiano_001));

    // BGM
    public static readonly SoundEffectType bgm_menu = new SoundEffectType(nameof(bgm_menu));
    public static readonly SoundEffectType bgm_lose_game = new SoundEffectType(nameof(bgm_lose_game));
    public static readonly SoundEffectType bgm_win_game = new SoundEffectType(nameof(bgm_win_game));
    public static readonly SoundEffectType bgm_start_game = new SoundEffectType(nameof(bgm_start_game));
    public static readonly SoundEffectType bgm_ocean = new SoundEffectType(nameof(bgm_ocean));
    public static readonly SoundEffectType bgm_leaves = new SoundEffectType(nameof(bgm_leaves));
    public static readonly SoundEffectType bgm_kelp = new SoundEffectType(nameof(bgm_kelp));
    public static readonly SoundEffectType bgm_roots = new SoundEffectType(nameof(bgm_roots));
    public static readonly SoundEffectType bgm_abyss = new SoundEffectType(nameof(bgm_abyss));
    public static readonly SoundEffectType bgm_jelly = new SoundEffectType(nameof(bgm_jelly));
    public static readonly SoundEffectType bgm_crystal = new SoundEffectType(nameof(bgm_crystal));
}