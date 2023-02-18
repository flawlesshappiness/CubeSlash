[System.Serializable]
public class StatID : FakeEnum
{
    public StatID(string id) : base(id) { }

    // Player
    public static readonly StatID player_health = new StatID(nameof(player_health));
    public static readonly StatID player_armor = new StatID(nameof(player_armor));
    public static readonly StatID player_body_size_perc = new StatID(nameof(player_body_size_perc));
    public static readonly StatID player_avoid_damage_chance = new StatID(nameof(player_avoid_damage_chance));
    public static readonly StatID player_regen_plant = new StatID(nameof(player_regen_plant));
    public static readonly StatID player_convert_health = new StatID(nameof(player_convert_health));
    public static readonly StatID player_velocity_flat = new StatID(nameof(player_velocity_flat));
    public static readonly StatID player_velocity_perc = new StatID(nameof(player_velocity_perc));
    public static readonly StatID player_acceleration_flat = new StatID(nameof(player_acceleration_flat));
    public static readonly StatID player_acceleration_perc = new StatID(nameof(player_acceleration_perc));
    public static readonly StatID player_cooldown_reduc_perc = new StatID(nameof(player_cooldown_reduc_perc));
    public static readonly StatID player_infinite_drag = new StatID(nameof(player_infinite_drag));
    public static readonly StatID player_collect_radius_perc = new StatID(nameof(player_collect_radius_perc));
    public static readonly StatID player_collect_speed_perc = new StatID(nameof(player_collect_speed_perc));
    public static readonly StatID player_collect_cooldown_flat = new StatID(nameof(player_collect_cooldown_flat));
    public static readonly StatID player_exp_bonus_perc = new StatID(nameof(player_exp_bonus_perc));

    // Chain
    public static readonly StatID chain_cooldown_flat = new StatID(nameof(chain_cooldown_flat));
    public static readonly StatID chain_cooldown_perc = new StatID(nameof(chain_cooldown_perc));
    public static readonly StatID chain_radius_perc = new StatID(nameof(chain_radius_perc));
    public static readonly StatID chain_chains = new StatID(nameof(chain_chains));
    public static readonly StatID chain_strikes = new StatID(nameof(chain_strikes));
    public static readonly StatID chain_hits_explode = new StatID(nameof(chain_hits_explode));
    public static readonly StatID chain_store_zaps = new StatID(nameof(chain_store_zaps));

    // Charge
    public static readonly StatID charge_cooldown_flat = new StatID(nameof(charge_cooldown_flat));
    public static readonly StatID charge_cooldown_perc = new StatID(nameof(charge_cooldown_perc));
    public static readonly StatID charge_width_perc = new StatID(nameof(charge_width_perc));
    public static readonly StatID charge_knock_self_perc = new StatID(nameof(charge_knock_self_perc));
    public static readonly StatID charge_knock_enemy_perc = new StatID(nameof(charge_knock_enemy_perc));
    public static readonly StatID charge_cooldown_kill_reduc_perc = new StatID(nameof(charge_cooldown_kill_reduc_perc));
    public static readonly StatID charge_beam_count = new StatID(nameof(charge_beam_count));
    public static readonly StatID charge_beam_arc_perc = new StatID(nameof(charge_beam_arc_perc));
    public static readonly StatID charge_suck_exp = new StatID(nameof(charge_suck_exp));
    public static readonly StatID charge_beam_back = new StatID(nameof(charge_beam_back));
    public static readonly StatID charge_trail = new StatID(nameof(charge_trail));
    public static readonly StatID charge_kill_explode = new StatID(nameof(charge_kill_explode));
    public static readonly StatID charge_kill_zap = new StatID(nameof(charge_kill_zap));
    public static readonly StatID charge_enemy_fragment = new StatID(nameof(charge_enemy_fragment));

    // Dash
    public static readonly StatID dash_cooldown_flat = new StatID(nameof(dash_cooldown_flat));
    public static readonly StatID dash_cooldown_perc = new StatID(nameof(dash_cooldown_perc));
    public static readonly StatID dash_distance_perc = new StatID(nameof(dash_distance_perc));
    public static readonly StatID dash_speed_perc = new StatID(nameof(dash_speed_perc));
    public static readonly StatID dash_radius_impact_perc = new StatID(nameof(dash_radius_impact_perc));
    public static readonly StatID dash_radius_knock_enemy_perc = new StatID(nameof(dash_radius_knock_enemy_perc));
    public static readonly StatID dash_force_knock_enemy_perc = new StatID(nameof(dash_force_knock_enemy_perc));
    public static readonly StatID dash_force_knock_self_perc = new StatID(nameof(dash_force_knock_self_perc));
    public static readonly StatID dash_trail = new StatID(nameof(dash_trail));
    public static readonly StatID dash_hit_cooldown_reduc = new StatID(nameof(dash_hit_cooldown_reduc));
    public static readonly StatID dash_shockwave_count = new StatID(nameof(dash_shockwave_count));
    public static readonly StatID dash_shockwave_speed_perc = new StatID(nameof(dash_shockwave_speed_perc));
    public static readonly StatID dash_shockwave_size_perc = new StatID(nameof(dash_shockwave_size_perc));
    public static readonly StatID dash_shockwave_distance_perc = new StatID(nameof(dash_shockwave_distance_perc));
    public static readonly StatID dash_shockwave_bounce = new StatID(nameof(dash_shockwave_bounce));
    public static readonly StatID dash_shockwave_only = new StatID(nameof(dash_shockwave_only));
    public static readonly StatID dash_impact_explode = new StatID(nameof(dash_impact_explode));
    public static readonly StatID dash_shockwave_linger = new StatID(nameof(dash_shockwave_linger));

    // Explode
    public static readonly StatID explode_cooldown_flat = new StatID(nameof(explode_cooldown_flat));
    public static readonly StatID explode_cooldown_perc = new StatID(nameof(explode_cooldown_perc));
    public static readonly StatID explode_delay_perc = new StatID(nameof(explode_delay_perc));
    public static readonly StatID explode_radius_perc = new StatID(nameof(explode_radius_perc));
    public static readonly StatID explode_force_knock_enemy_perc = new StatID(nameof(explode_force_knock_enemy_perc));
    public static readonly StatID explode_delay_pull = new StatID(nameof(explode_delay_pull));
    public static readonly StatID explode_projectile = new StatID(nameof(explode_projectile));
    public static readonly StatID explode_front = new StatID(nameof(explode_front));
    public static readonly StatID explode_chain = new StatID(nameof(explode_chain));
    public static readonly StatID explode_fragments = new StatID(nameof(explode_fragments));

    // Mines
    public static readonly StatID mines_cooldown_flat = new StatID(nameof(mines_cooldown_flat));
    public static readonly StatID mines_cooldown_perc = new StatID(nameof(mines_cooldown_perc));
    public static readonly StatID mines_shell_count = new StatID(nameof(mines_shell_count));
    public static readonly StatID mines_fragment_count = new StatID(nameof(mines_fragment_count));
    public static readonly StatID mines_size_perc = new StatID(nameof(mines_size_perc));
    public static readonly StatID mines_shell_lifetime_perc = new StatID(nameof(mines_shell_lifetime_perc));
    public static readonly StatID mines_fragment_lifetime_perc = new StatID(nameof(mines_fragment_lifetime_perc));
    public static readonly StatID mines_seeking = new StatID(nameof(mines_seeking));
    public static readonly StatID mines_turn_speed_perc = new StatID(nameof(mines_turn_speed_perc));
    public static readonly StatID mines_fragment_chain = new StatID(nameof(mines_fragment_chain));
    public static readonly StatID mines_fragment_explode = new StatID(nameof(mines_fragment_explode));
    public static readonly StatID mines_fragment_only = new StatID(nameof(mines_fragment_only));
    public static readonly StatID mines_double_shell = new StatID(nameof(mines_double_shell));

    // Split
    public static readonly StatID split_cooldown_flat = new StatID(nameof(split_cooldown_flat));
    public static readonly StatID split_cooldown_perc = new StatID(nameof(split_cooldown_perc));
    public static readonly StatID split_count = new StatID(nameof(split_count));
    public static readonly StatID split_speed_perc = new StatID(nameof(split_speed_perc));
    public static readonly StatID split_arc_perc = new StatID(nameof(split_arc_perc));
    public static readonly StatID split_size_perc = new StatID(nameof(split_size_perc));
    public static readonly StatID split_count_bursts = new StatID(nameof(split_count_bursts));
    public static readonly StatID split_projectile_fragments = new StatID(nameof(split_projectile_fragments));
    public static readonly StatID split_radius_knock_enemy_perc = new StatID(nameof(split_radius_knock_enemy_perc));
    public static readonly StatID split_force_knock_enemy_perc = new StatID(nameof(split_force_knock_enemy_perc));
    public static readonly StatID split_penetrate = new StatID(nameof(split_penetrate));
    public static readonly StatID split_explode = new StatID(nameof(split_explode));
    public static readonly StatID split_chain = new StatID(nameof(split_chain));
    public static readonly StatID split_hit_cooldown_reduc = new StatID(nameof(split_hit_cooldown_reduc));
    public static readonly StatID split_projectile_linger = new StatID(nameof(split_projectile_linger));
}