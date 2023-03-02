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
    
    public static readonly UpgradeID player_collect_cooldown_reduc = new UpgradeID(nameof(player_collect_cooldown_reduc));
    public static readonly UpgradeID player_infinite_drag = new UpgradeID(nameof(player_infinite_drag));
    public static readonly UpgradeID player_exp_bonus = new UpgradeID(nameof(player_exp_bonus));
    public static readonly UpgradeID player_cooldown_reduc = new UpgradeID(nameof(player_cooldown_reduc));

    // Charm
    public static readonly UpgradeID charm_armor = new UpgradeID(nameof(charm_armor));
    public static readonly UpgradeID charm_health = new UpgradeID(nameof(charm_health));
    public static readonly UpgradeID charm_speed = new UpgradeID(nameof(charm_speed));

    // Chain
    public static readonly UpgradeID chain_delay_1 = new UpgradeID(nameof(chain_delay_1));
    public static readonly UpgradeID chain_delay_2 = new UpgradeID(nameof(chain_delay_2));
    public static readonly UpgradeID chain_radius_1 = new UpgradeID(nameof(chain_radius_1));
    public static readonly UpgradeID chain_radius_2 = new UpgradeID(nameof(chain_radius_2));
    public static readonly UpgradeID chain_strikes = new UpgradeID(nameof(chain_strikes));
    public static readonly UpgradeID chain_targets_1 = new UpgradeID(nameof(chain_targets_1));
    public static readonly UpgradeID chain_targets_2 = new UpgradeID(nameof(chain_targets_2));
    public static readonly UpgradeID chain_targets_3 = new UpgradeID(nameof(chain_targets_3));

    public static readonly UpgradeID chain_mod_charge = new UpgradeID(nameof(chain_mod_charge));
    public static readonly UpgradeID chain_mod_dash = new UpgradeID(nameof(chain_mod_dash));
    public static readonly UpgradeID chain_mod_explode = new UpgradeID(nameof(chain_mod_explode));
    public static readonly UpgradeID chain_mod_mines = new UpgradeID(nameof(chain_mod_mines));
    public static readonly UpgradeID chain_mod_split = new UpgradeID(nameof(chain_mod_split));

    // Charge
    public static readonly UpgradeID charge_beam_back = new UpgradeID(nameof(charge_beam_back));
    public static readonly UpgradeID charge_knock_enemy_1 = new UpgradeID(nameof(charge_knock_enemy_1));
    public static readonly UpgradeID charge_knock_self_1 = new UpgradeID(nameof(charge_knock_self_1));
    public static readonly UpgradeID charge_knock_self_2 = new UpgradeID(nameof(charge_knock_self_2));
    public static readonly UpgradeID charge_suck_exp = new UpgradeID(nameof(charge_suck_exp));
    public static readonly UpgradeID charge_cooldown_reduc_kill = new UpgradeID(nameof(charge_cooldown_reduc_kill));
    public static readonly UpgradeID charge_width_1 = new UpgradeID(nameof(charge_width_1));
    public static readonly UpgradeID charge_width_2 = new UpgradeID(nameof(charge_width_2));
    public static readonly UpgradeID charge_width_3 = new UpgradeID(nameof(charge_width_3));

    public static readonly UpgradeID charge_mod_chain = new UpgradeID(nameof(charge_mod_chain));
    public static readonly UpgradeID charge_mod_dash = new UpgradeID(nameof(charge_mod_dash));
    public static readonly UpgradeID charge_mod_explode = new UpgradeID(nameof(charge_mod_explode));
    public static readonly UpgradeID charge_mod_mines = new UpgradeID(nameof(charge_mod_mines));
    public static readonly UpgradeID charge_mod_split = new UpgradeID(nameof(charge_mod_split));

    // Dash
    public static readonly UpgradeID dash_area_1 = new UpgradeID(nameof(dash_area_1));
    public static readonly UpgradeID dash_area_2 = new UpgradeID(nameof(dash_area_2));
    public static readonly UpgradeID dash_distance_1 = new UpgradeID(nameof(dash_distance_1));
    public static readonly UpgradeID dash_distance_2 = new UpgradeID(nameof(dash_distance_2));
    public static readonly UpgradeID dash_distance_3 = new UpgradeID(nameof(dash_distance_3));
    public static readonly UpgradeID dash_cooldown_reduc_hit = new UpgradeID(nameof(dash_cooldown_reduc_hit));
    public static readonly UpgradeID dash_knock_enemy_1 = new UpgradeID(nameof(dash_knock_enemy_1));
    public static readonly UpgradeID dash_shockwave_distance_1 = new UpgradeID(nameof(dash_shockwave_distance_1));
    public static readonly UpgradeID dash_shockwave_distance_2 = new UpgradeID(nameof(dash_shockwave_distance_2));
    public static readonly UpgradeID dash_shockwave_size_1 = new UpgradeID(nameof(dash_shockwave_size_1));
    public static readonly UpgradeID dash_shockwave_size_2 = new UpgradeID(nameof(dash_shockwave_size_2));
    public static readonly UpgradeID dash_trail = new UpgradeID(nameof(dash_trail));

    public static readonly UpgradeID dash_mod_chain = new UpgradeID(nameof(dash_mod_chain));
    public static readonly UpgradeID dash_mod_charge = new UpgradeID(nameof(dash_mod_charge));
    public static readonly UpgradeID dash_mod_explode = new UpgradeID(nameof(dash_mod_explode));
    public static readonly UpgradeID dash_mod_mines = new UpgradeID(nameof(dash_mod_mines));
    public static readonly UpgradeID dash_mod_split = new UpgradeID(nameof(dash_mod_split));

    // Explode
    public static readonly UpgradeID explode_delay_1 = new UpgradeID(nameof(explode_delay_1));
    public static readonly UpgradeID explode_delay_2 = new UpgradeID(nameof(explode_delay_2));
    public static readonly UpgradeID explode_delay_3 = new UpgradeID(nameof(explode_delay_3));
    public static readonly UpgradeID explode_delay_pull = new UpgradeID(nameof(explode_delay_pull));
    public static readonly UpgradeID explode_knock_enemy_1 = new UpgradeID(nameof(explode_knock_enemy_1));
    public static readonly UpgradeID explode_knock_enemy_2 = new UpgradeID(nameof(explode_knock_enemy_2));
    public static readonly UpgradeID explode_radius_1 = new UpgradeID(nameof(explode_radius_1));
    public static readonly UpgradeID explode_radius_2 = new UpgradeID(nameof(explode_radius_2));
    public static readonly UpgradeID explode_invulnerable_1 = new UpgradeID(nameof(explode_invulnerable_1));
    public static readonly UpgradeID explode_minis_1 = new UpgradeID(nameof(explode_minis_1));
    public static readonly UpgradeID explode_minis_2 = new UpgradeID(nameof(explode_minis_2));
    public static readonly UpgradeID explode_minis_3 = new UpgradeID(nameof(explode_minis_3));

    public static readonly UpgradeID explode_mod_chain = new UpgradeID(nameof(explode_mod_chain));
    public static readonly UpgradeID explode_mod_charge = new UpgradeID(nameof(explode_mod_charge));
    public static readonly UpgradeID explode_mod_dash = new UpgradeID(nameof(explode_mod_dash));
    public static readonly UpgradeID explode_mod_mines = new UpgradeID(nameof(explode_mod_mines));
    public static readonly UpgradeID explode_mod_split = new UpgradeID(nameof(explode_mod_split));

    // Mines
    public static readonly UpgradeID mines_count_1 = new UpgradeID(nameof(mines_count_1));
    public static readonly UpgradeID mines_count_2 = new UpgradeID(nameof(mines_count_2));
    public static readonly UpgradeID mines_count_3 = new UpgradeID(nameof(mines_count_3));
    public static readonly UpgradeID mines_fragments_1 = new UpgradeID(nameof(mines_fragments_1));
    public static readonly UpgradeID mines_fragments_2 = new UpgradeID(nameof(mines_fragments_2));
    public static readonly UpgradeID mines_fragments_3 = new UpgradeID(nameof(mines_fragments_3));
    public static readonly UpgradeID mines_lifetime_1 = new UpgradeID(nameof(mines_lifetime_1));
    public static readonly UpgradeID mines_lifetime_2 = new UpgradeID(nameof(mines_lifetime_2));
    public static readonly UpgradeID mines_seeking = new UpgradeID(nameof(mines_seeking));
    public static readonly UpgradeID mines_size_1 = new UpgradeID(nameof(mines_size_1));
    public static readonly UpgradeID mines_size_2 = new UpgradeID(nameof(mines_size_2));

    public static readonly UpgradeID mines_mod_chain = new UpgradeID(nameof(mines_mod_chain));
    public static readonly UpgradeID mines_mod_charge = new UpgradeID(nameof(mines_mod_charge));
    public static readonly UpgradeID mines_mod_dash = new UpgradeID(nameof(mines_mod_dash));
    public static readonly UpgradeID mines_mod_explode = new UpgradeID(nameof(mines_mod_explode));
    public static readonly UpgradeID mines_mod_split = new UpgradeID(nameof(mines_mod_split));

    // Split
    public static readonly UpgradeID split_count_1 = new UpgradeID(nameof(split_count_1));
    public static readonly UpgradeID split_count_2 = new UpgradeID(nameof(split_count_2));
    public static readonly UpgradeID split_count_3 = new UpgradeID(nameof(split_count_3));
    public static readonly UpgradeID split_hit_split = new UpgradeID(nameof(split_hit_split));
    public static readonly UpgradeID split_knock_enemy_1 = new UpgradeID(nameof(split_knock_enemy_1));
    public static readonly UpgradeID split_size_1 = new UpgradeID(nameof(split_size_1));
    public static readonly UpgradeID split_size_2 = new UpgradeID(nameof(split_size_2));
    public static readonly UpgradeID split_speed_1 = new UpgradeID(nameof(split_speed_1));
    public static readonly UpgradeID split_speed_2 = new UpgradeID(nameof(split_speed_2));
    public static readonly UpgradeID split_burst_1 = new UpgradeID(nameof(split_burst_1));
    public static readonly UpgradeID split_bounce_1 = new UpgradeID(nameof(split_bounce_1));
    public static readonly UpgradeID split_bounce_2 = new UpgradeID(nameof(split_bounce_2));
    public static readonly UpgradeID split_bounce_3 = new UpgradeID(nameof(split_bounce_3));

    public static readonly UpgradeID split_mod_chain = new UpgradeID(nameof(split_mod_chain));
    public static readonly UpgradeID split_mod_charge = new UpgradeID(nameof(split_mod_charge));
    public static readonly UpgradeID split_mod_dash = new UpgradeID(nameof(split_mod_dash));
    public static readonly UpgradeID split_mod_explode = new UpgradeID(nameof(split_mod_explode));
    public static readonly UpgradeID split_mod_mines = new UpgradeID(nameof(split_mod_mines));
}