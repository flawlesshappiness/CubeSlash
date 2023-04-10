[System.Serializable]
public class StatID : FakeEnum
{
    public StatID(string id) : base(id) { }

    // Player
    public static readonly StatID player_health = new StatID(nameof(player_health));
    public static readonly StatID player_armor = new StatID(nameof(player_armor));
    public static readonly StatID player_body_size_perc = new StatID(nameof(player_body_size_perc));
    public static readonly StatID player_avoid_damage_chance = new StatID(nameof(player_avoid_damage_chance));
    public static readonly StatID player_regen_kill = new StatID(nameof(player_regen_kill));
    public static readonly StatID player_regen_plant = new StatID(nameof(player_regen_plant));
    public static readonly StatID player_convert_health = new StatID(nameof(player_convert_health));
    public static readonly StatID player_velocity_flat = new StatID(nameof(player_velocity_flat));
    public static readonly StatID player_velocity_perc = new StatID(nameof(player_velocity_perc));
    public static readonly StatID player_acceleration_flat = new StatID(nameof(player_acceleration_flat));
    public static readonly StatID player_acceleration_perc = new StatID(nameof(player_acceleration_perc));
    public static readonly StatID player_infinite_drag = new StatID(nameof(player_infinite_drag));
    public static readonly StatID player_collect_radius_perc = new StatID(nameof(player_collect_radius_perc));
    public static readonly StatID player_collect_speed_perc = new StatID(nameof(player_collect_speed_perc));
    public static readonly StatID player_collect_cooldown_flat = new StatID(nameof(player_collect_cooldown_flat));
    public static readonly StatID player_cooldown_multiplier = new StatID(nameof(player_cooldown_multiplier));
    public static readonly StatID player_exp_multiplier = new StatID(nameof(player_exp_multiplier));

    // Chain
    public static readonly StatID chain_cooldown_flat = new StatID(nameof(chain_cooldown_flat));
    public static readonly StatID chain_cooldown_perc = new StatID(nameof(chain_cooldown_perc));
    public static readonly StatID chain_radius_perc = new StatID(nameof(chain_radius_perc));
    public static readonly StatID chain_chains = new StatID(nameof(chain_chains));
    public static readonly StatID chain_strikes = new StatID(nameof(chain_strikes));
    public static readonly StatID chain_chain_strikes = new StatID(nameof(chain_chain_strikes));
    public static readonly StatID chain_hits_explode = new StatID(nameof(chain_hits_explode));
    public static readonly StatID chain_fragments = new StatID(nameof(chain_fragments));
    public static readonly StatID chain_trail = new StatID(nameof(chain_trail));

    // Dash
    public static readonly StatID dash_cooldown_flat = new StatID(nameof(dash_cooldown_flat));
    public static readonly StatID dash_cooldown_perc = new StatID(nameof(dash_cooldown_perc));
    public static readonly StatID dash_distance_perc = new StatID(nameof(dash_distance_perc));
    public static readonly StatID dash_speed_perc = new StatID(nameof(dash_speed_perc));
    public static readonly StatID dash_trail_time_perc = new StatID(nameof(dash_trail_time_perc));
    public static readonly StatID dash_trail_radius_perc = new StatID(nameof(dash_trail_radius_perc));
    public static readonly StatID dash_trail_explode = new StatID(nameof(dash_trail_explode));
    public static readonly StatID dash_trail_fragment = new StatID(nameof(dash_trail_fragment));
    public static readonly StatID dash_trail_split = new StatID(nameof(dash_trail_split));
    public static readonly StatID dash_trail_chain = new StatID(nameof(dash_trail_chain));

    // Explode
    public static readonly StatID explode_cooldown_flat = new StatID(nameof(explode_cooldown_flat));
    public static readonly StatID explode_cooldown_perc = new StatID(nameof(explode_cooldown_perc));
    public static readonly StatID explode_radius_perc = new StatID(nameof(explode_radius_perc));
    public static readonly StatID explode_chain = new StatID(nameof(explode_chain));
    public static readonly StatID explode_fragments = new StatID(nameof(explode_fragments));
    public static readonly StatID explode_minis = new StatID(nameof(explode_minis));
    public static readonly StatID explode_slow_linger = new StatID(nameof(explode_slow_linger));
    public static readonly StatID explode_charge_time = new StatID(nameof(explode_charge_time));
    public static readonly StatID explode_slow_area_perc = new StatID(nameof(explode_slow_area_perc));
    public static readonly StatID explode_slow_perc = new StatID(nameof(explode_slow_perc));

    // Mines
    public static readonly StatID mines_cooldown_flat = new StatID(nameof(mines_cooldown_flat));
    public static readonly StatID mines_cooldown_perc = new StatID(nameof(mines_cooldown_perc));
    public static readonly StatID mines_shell_count = new StatID(nameof(mines_shell_count));
    public static readonly StatID mines_fragment_count = new StatID(nameof(mines_fragment_count));
    public static readonly StatID mines_size_perc = new StatID(nameof(mines_size_perc));
    public static readonly StatID mines_fragment_size_perc = new StatID(nameof(mines_fragment_size_perc));
    public static readonly StatID mines_shell_lifetime_perc = new StatID(nameof(mines_shell_lifetime_perc));
    public static readonly StatID mines_fragment_lifetime_perc = new StatID(nameof(mines_fragment_lifetime_perc));
    public static readonly StatID mines_fragment_chain = new StatID(nameof(mines_fragment_chain));
    public static readonly StatID mines_fragment_explode = new StatID(nameof(mines_fragment_explode));
    public static readonly StatID mines_double_shell = new StatID(nameof(mines_double_shell));
    public static readonly StatID mines_trail = new StatID(nameof(mines_trail));

    // Split
    public static readonly StatID split_cooldown_flat = new StatID(nameof(split_cooldown_flat));
    public static readonly StatID split_cooldown_perc = new StatID(nameof(split_cooldown_perc));
    public static readonly StatID split_count = new StatID(nameof(split_count));
    public static readonly StatID split_arc_perc = new StatID(nameof(split_arc_perc));
    public static readonly StatID split_size_perc = new StatID(nameof(split_size_perc));
    public static readonly StatID split_projectile_fragments = new StatID(nameof(split_projectile_fragments));
    public static readonly StatID split_explode = new StatID(nameof(split_explode));
    public static readonly StatID split_chain = new StatID(nameof(split_chain));
    public static readonly StatID split_piercing_count = new StatID(nameof(split_piercing_count));
    public static readonly StatID split_trail = new StatID(nameof(split_trail));
}