[System.Serializable]
public class UpgradeID : FakeEnum
{
    public UpgradeID(string id) : base(id) { }

    // Player
    public static readonly UpgradeID player_acceleration_1 = new UpgradeID(nameof(player_acceleration_1));
    public static readonly UpgradeID player_acceleration_2 = new UpgradeID(nameof(player_acceleration_2));
    public static readonly UpgradeID player_acceleration_3 = new UpgradeID(nameof(player_acceleration_3));

    public static readonly UpgradeID player_avoid_damage_1 = new UpgradeID(nameof(player_avoid_damage_1));
    public static readonly UpgradeID player_avoid_damage_2 = new UpgradeID(nameof(player_avoid_damage_2));
    public static readonly UpgradeID player_avoid_damage_3 = new UpgradeID(nameof(player_avoid_damage_3));

    public static readonly UpgradeID player_collect_radius_1 = new UpgradeID(nameof(player_collect_radius_1));
    public static readonly UpgradeID player_collect_radius_2 = new UpgradeID(nameof(player_collect_radius_2));
    public static readonly UpgradeID player_collect_radius_3 = new UpgradeID(nameof(player_collect_radius_3));

    public static readonly UpgradeID player_collect_speed = new UpgradeID(nameof(player_collect_speed));
    public static readonly UpgradeID player_convert_health = new UpgradeID(nameof(player_convert_health));

    public static readonly UpgradeID player_health_1 = new UpgradeID(nameof(player_health_1));
    public static readonly UpgradeID player_health_2 = new UpgradeID(nameof(player_health_2));
    public static readonly UpgradeID player_health_3 = new UpgradeID(nameof(player_health_3));

    public static readonly UpgradeID player_armor_1 = new UpgradeID(nameof(player_armor_1));
    public static readonly UpgradeID player_armor_2 = new UpgradeID(nameof(player_armor_2));
    public static readonly UpgradeID player_armor_3 = new UpgradeID(nameof(player_armor_3));

    public static readonly UpgradeID player_regen_kill = new UpgradeID(nameof(player_regen_kill));
    public static readonly UpgradeID player_regen_plant = new UpgradeID(nameof(player_regen_plant));

    public static readonly UpgradeID player_max_speed_1 = new UpgradeID(nameof(player_max_speed_1));
    public static readonly UpgradeID player_max_speed_2 = new UpgradeID(nameof(player_max_speed_2));
    public static readonly UpgradeID player_max_speed_3 = new UpgradeID(nameof(player_max_speed_3));

    public static readonly UpgradeID player_exp_bonus_1 = new UpgradeID(nameof(player_exp_bonus_1));
    public static readonly UpgradeID player_exp_bonus_2 = new UpgradeID(nameof(player_exp_bonus_2));
    public static readonly UpgradeID player_exp_bonus_3 = new UpgradeID(nameof(player_exp_bonus_3));
    public static readonly UpgradeID player_exp_bonus_4 = new UpgradeID(nameof(player_exp_bonus_4));
    public static readonly UpgradeID player_exp_bonus_5 = new UpgradeID(nameof(player_exp_bonus_5));

    public static readonly UpgradeID player_cooldown_reduc_1 = new UpgradeID(nameof(player_cooldown_reduc_1));
    public static readonly UpgradeID player_cooldown_reduc_2 = new UpgradeID(nameof(player_cooldown_reduc_2));
    public static readonly UpgradeID player_cooldown_reduc_3 = new UpgradeID(nameof(player_cooldown_reduc_3));
    public static readonly UpgradeID player_cooldown_reduc_4 = new UpgradeID(nameof(player_cooldown_reduc_4));
    public static readonly UpgradeID player_cooldown_reduc_5 = new UpgradeID(nameof(player_cooldown_reduc_5));

    public static readonly UpgradeID player_collect_cooldown_reduc = new UpgradeID(nameof(player_collect_cooldown_reduc));
    public static readonly UpgradeID player_infinite_drag = new UpgradeID(nameof(player_infinite_drag));

    // Dodge
    public static readonly UpgradeID dodge_distance_1 = new UpgradeID(nameof(dodge_distance_1));
    public static readonly UpgradeID dodge_distance_2 = new UpgradeID(nameof(dodge_distance_2));
    public static readonly UpgradeID dodge_distance_3 = new UpgradeID(nameof(dodge_distance_3));

    public static readonly UpgradeID dodge_cooldown_1 = new UpgradeID(nameof(dodge_cooldown_1));
    public static readonly UpgradeID dodge_cooldown_2 = new UpgradeID(nameof(dodge_cooldown_2));
    public static readonly UpgradeID dodge_cooldown_3 = new UpgradeID(nameof(dodge_cooldown_3));

    // Heal
    public static readonly UpgradeID heal_kill_value_1 = new UpgradeID(nameof(heal_kill_value_1));
    public static readonly UpgradeID heal_kill_value_2 = new UpgradeID(nameof(heal_kill_value_2));
    public static readonly UpgradeID heal_kill_value_3 = new UpgradeID(nameof(heal_kill_value_3));

    // Charm
    public static readonly UpgradeID charm_armor = new UpgradeID(nameof(charm_armor));
    public static readonly UpgradeID charm_health = new UpgradeID(nameof(charm_health));
    public static readonly UpgradeID charm_speed = new UpgradeID(nameof(charm_speed));

    // Chain
    public static readonly UpgradeID chain_cooldown_1 = new UpgradeID(nameof(chain_cooldown_1));
    public static readonly UpgradeID chain_cooldown_2 = new UpgradeID(nameof(chain_cooldown_2));
    public static readonly UpgradeID chain_cooldown_3 = new UpgradeID(nameof(chain_cooldown_3));
    public static readonly UpgradeID chain_radius_1 = new UpgradeID(nameof(chain_radius_1));
    public static readonly UpgradeID chain_radius_2 = new UpgradeID(nameof(chain_radius_2));
    public static readonly UpgradeID chain_radius_3 = new UpgradeID(nameof(chain_radius_3));
    public static readonly UpgradeID chain_targets_1 = new UpgradeID(nameof(chain_targets_1));
    public static readonly UpgradeID chain_targets_2 = new UpgradeID(nameof(chain_targets_2));
    public static readonly UpgradeID chain_targets_3 = new UpgradeID(nameof(chain_targets_3));

    public static readonly UpgradeID chain_mod_dash = new UpgradeID(nameof(chain_mod_dash));
    public static readonly UpgradeID chain_mod_explode = new UpgradeID(nameof(chain_mod_explode));
    public static readonly UpgradeID chain_mod_mines = new UpgradeID(nameof(chain_mod_mines));
    public static readonly UpgradeID chain_mod_split = new UpgradeID(nameof(chain_mod_split));
    public static readonly UpgradeID chain_mod_boomerang = new UpgradeID(nameof(chain_mod_boomerang));

    // Dash
    public static readonly UpgradeID dash_distance_1 = new UpgradeID(nameof(dash_distance_1));
    public static readonly UpgradeID dash_distance_2 = new UpgradeID(nameof(dash_distance_2));
    public static readonly UpgradeID dash_distance_3 = new UpgradeID(nameof(dash_distance_3));
    public static readonly UpgradeID dash_speed_1 = new UpgradeID(nameof(dash_speed_1));
    public static readonly UpgradeID dash_speed_2 = new UpgradeID(nameof(dash_speed_2));
    public static readonly UpgradeID dash_speed_3 = new UpgradeID(nameof(dash_speed_3));
    public static readonly UpgradeID dash_trail_time_1 = new UpgradeID(nameof(dash_trail_time_1));
    public static readonly UpgradeID dash_trail_time_2 = new UpgradeID(nameof(dash_trail_time_2));
    public static readonly UpgradeID dash_trail_time_3 = new UpgradeID(nameof(dash_trail_time_3));

    public static readonly UpgradeID dash_mod_chain = new UpgradeID(nameof(dash_mod_chain));
    public static readonly UpgradeID dash_mod_explode = new UpgradeID(nameof(dash_mod_explode));
    public static readonly UpgradeID dash_mod_mines = new UpgradeID(nameof(dash_mod_mines));
    public static readonly UpgradeID dash_mod_split = new UpgradeID(nameof(dash_mod_split));

    // Explode
    public static readonly UpgradeID explode_charge_time_1 = new UpgradeID(nameof(explode_charge_time_1));
    public static readonly UpgradeID explode_charge_time_2 = new UpgradeID(nameof(explode_charge_time_2));
    public static readonly UpgradeID explode_charge_time_3 = new UpgradeID(nameof(explode_charge_time_3));
    public static readonly UpgradeID explode_charge_time_4 = new UpgradeID(nameof(explode_charge_time_4));
    public static readonly UpgradeID explode_charge_time_5 = new UpgradeID(nameof(explode_charge_time_5));
    public static readonly UpgradeID explode_charge_time_reduc_1 = new UpgradeID(nameof(explode_charge_time_reduc_1));
    public static readonly UpgradeID explode_charge_time_reduc_2 = new UpgradeID(nameof(explode_charge_time_reduc_2));
    public static readonly UpgradeID explode_charge_time_reduc_3 = new UpgradeID(nameof(explode_charge_time_reduc_3));
    public static readonly UpgradeID explode_charge_time_reduc_4 = new UpgradeID(nameof(explode_charge_time_reduc_4));
    public static readonly UpgradeID explode_charge_time_reduc_5 = new UpgradeID(nameof(explode_charge_time_reduc_5));
    public static readonly UpgradeID explode_radius_1 = new UpgradeID(nameof(explode_radius_1));
    public static readonly UpgradeID explode_radius_2 = new UpgradeID(nameof(explode_radius_2));
    public static readonly UpgradeID explode_radius_3 = new UpgradeID(nameof(explode_radius_3));
    public static readonly UpgradeID explode_radius_4 = new UpgradeID(nameof(explode_radius_4));
    public static readonly UpgradeID explode_radius_5 = new UpgradeID(nameof(explode_radius_5));
    public static readonly UpgradeID explode_slow_1 = new UpgradeID(nameof(explode_slow_1));
    public static readonly UpgradeID explode_slow_2 = new UpgradeID(nameof(explode_slow_2));
    public static readonly UpgradeID explode_slow_3 = new UpgradeID(nameof(explode_slow_3));

    public static readonly UpgradeID explode_mod_chain = new UpgradeID(nameof(explode_mod_chain));
    public static readonly UpgradeID explode_mod_dash = new UpgradeID(nameof(explode_mod_dash));
    public static readonly UpgradeID explode_mod_mines = new UpgradeID(nameof(explode_mod_mines));
    public static readonly UpgradeID explode_mod_split = new UpgradeID(nameof(explode_mod_split));
    public static readonly UpgradeID explode_mod_boomerang = new UpgradeID(nameof(explode_mod_boomerang));

    // Mines
    public static readonly UpgradeID mines_count_1 = new UpgradeID(nameof(mines_count_1));
    public static readonly UpgradeID mines_count_2 = new UpgradeID(nameof(mines_count_2));
    public static readonly UpgradeID mines_count_3 = new UpgradeID(nameof(mines_count_3));
    public static readonly UpgradeID mines_fragments_1 = new UpgradeID(nameof(mines_fragments_1));
    public static readonly UpgradeID mines_fragments_2 = new UpgradeID(nameof(mines_fragments_2));
    public static readonly UpgradeID mines_fragments_3 = new UpgradeID(nameof(mines_fragments_3));
    public static readonly UpgradeID mines_size_1 = new UpgradeID(nameof(mines_size_1));
    public static readonly UpgradeID mines_size_2 = new UpgradeID(nameof(mines_size_2));
    public static readonly UpgradeID mines_size_3 = new UpgradeID(nameof(mines_size_3));

    public static readonly UpgradeID mines_mod_chain = new UpgradeID(nameof(mines_mod_chain));
    public static readonly UpgradeID mines_mod_dash = new UpgradeID(nameof(mines_mod_dash));
    public static readonly UpgradeID mines_mod_explode = new UpgradeID(nameof(mines_mod_explode));
    public static readonly UpgradeID mines_mod_split = new UpgradeID(nameof(mines_mod_split));
    public static readonly UpgradeID mines_mod_boomerang = new UpgradeID(nameof(mines_mod_boomerang));

    // Split
    public static readonly UpgradeID split_count_1 = new UpgradeID(nameof(split_count_1));
    public static readonly UpgradeID split_count_2 = new UpgradeID(nameof(split_count_2));
    public static readonly UpgradeID split_count_3 = new UpgradeID(nameof(split_count_3));
    public static readonly UpgradeID split_size_1 = new UpgradeID(nameof(split_size_1));
    public static readonly UpgradeID split_size_2 = new UpgradeID(nameof(split_size_2));
    public static readonly UpgradeID split_size_3 = new UpgradeID(nameof(split_size_3));
    public static readonly UpgradeID split_pierce_1 = new UpgradeID(nameof(split_pierce_1));
    public static readonly UpgradeID split_pierce_2 = new UpgradeID(nameof(split_pierce_2));
    public static readonly UpgradeID split_pierce_3 = new UpgradeID(nameof(split_pierce_3));

    public static readonly UpgradeID split_mod_chain = new UpgradeID(nameof(split_mod_chain));
    public static readonly UpgradeID split_mod_dash = new UpgradeID(nameof(split_mod_dash));
    public static readonly UpgradeID split_mod_explode = new UpgradeID(nameof(split_mod_explode));
    public static readonly UpgradeID split_mod_mines = new UpgradeID(nameof(split_mod_mines));
    public static readonly UpgradeID split_mod_boomerang = new UpgradeID(nameof(split_mod_boomerang));

    // Boomerang
    public static readonly UpgradeID boomerang_distance_1 = new UpgradeID(nameof(boomerang_distance_1));
    public static readonly UpgradeID boomerang_distance_2 = new UpgradeID(nameof(boomerang_distance_2));
    public static readonly UpgradeID boomerang_distance_3 = new UpgradeID(nameof(boomerang_distance_3));
    public static readonly UpgradeID boomerang_distance_4 = new UpgradeID(nameof(boomerang_distance_4));
    public static readonly UpgradeID boomerang_distance_5 = new UpgradeID(nameof(boomerang_distance_5));
    public static readonly UpgradeID boomerang_size_1 = new UpgradeID(nameof(boomerang_size_1));
    public static readonly UpgradeID boomerang_size_2 = new UpgradeID(nameof(boomerang_size_2));
    public static readonly UpgradeID boomerang_size_3 = new UpgradeID(nameof(boomerang_size_3));
    public static readonly UpgradeID boomerang_size_4 = new UpgradeID(nameof(boomerang_size_4));
    public static readonly UpgradeID boomerang_size_5 = new UpgradeID(nameof(boomerang_size_5));
    public static readonly UpgradeID boomerang_catch_cd_1 = new UpgradeID(nameof(boomerang_catch_cd_1));
    public static readonly UpgradeID boomerang_catch_cd_2 = new UpgradeID(nameof(boomerang_catch_cd_2));
    public static readonly UpgradeID boomerang_catch_cd_3 = new UpgradeID(nameof(boomerang_catch_cd_3));
    public static readonly UpgradeID boomerang_catch_cd_4 = new UpgradeID(nameof(boomerang_catch_cd_4));
    public static readonly UpgradeID boomerang_catch_cd_5 = new UpgradeID(nameof(boomerang_catch_cd_5));
    public static readonly UpgradeID boomerang_lifetime_1 = new UpgradeID(nameof(boomerang_lifetime_1));
    public static readonly UpgradeID boomerang_lifetime_2 = new UpgradeID(nameof(boomerang_lifetime_2));
    public static readonly UpgradeID boomerang_lifetime_3 = new UpgradeID(nameof(boomerang_lifetime_3));
    public static readonly UpgradeID boomerang_lifetime_4 = new UpgradeID(nameof(boomerang_lifetime_4));
    public static readonly UpgradeID boomerang_lifetime_5 = new UpgradeID(nameof(boomerang_lifetime_5));
    public static readonly UpgradeID boomerang_linger_time_1 = new UpgradeID(nameof(boomerang_linger_time_1));
    public static readonly UpgradeID boomerang_linger_time_2 = new UpgradeID(nameof(boomerang_linger_time_2));
    public static readonly UpgradeID boomerang_linger_time_3 = new UpgradeID(nameof(boomerang_linger_time_3));
    public static readonly UpgradeID boomerang_linger_time_4 = new UpgradeID(nameof(boomerang_linger_time_4));
    public static readonly UpgradeID boomerang_linger_time_5 = new UpgradeID(nameof(boomerang_linger_time_5));

    public static readonly UpgradeID boomerang_mod_split = new UpgradeID(nameof(boomerang_mod_split));
    public static readonly UpgradeID boomerang_mod_mines = new UpgradeID(nameof(boomerang_mod_mines));
    public static readonly UpgradeID boomerang_mod_chain = new UpgradeID(nameof(boomerang_mod_chain));
    public static readonly UpgradeID boomerang_mod_explode = new UpgradeID(nameof(boomerang_mod_explode));
}