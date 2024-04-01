public partial class GameAttributeType : FakeEnum
{
    // CHAIN
    public static readonly GameAttributeType chain_fragments = new GameAttributeType(nameof(chain_fragments));
    public static readonly GameAttributeType chain_strikes = new GameAttributeType(nameof(chain_strikes));
    public static readonly GameAttributeType chain_chains = new GameAttributeType(nameof(chain_chains));
    public static readonly GameAttributeType chain_cooldown = new GameAttributeType(nameof(chain_cooldown));
    public static readonly GameAttributeType chain_explosion_radius = new GameAttributeType(nameof(chain_explosion_radius));
    public static readonly GameAttributeType chain_radius = new GameAttributeType(nameof(chain_radius));
    public static readonly GameAttributeType chain_boomerang = new GameAttributeType(nameof(chain_boomerang));
    public static readonly GameAttributeType chain_zap_delay = new GameAttributeType(nameof(chain_zap_delay));
    public static readonly GameAttributeType chain_zap_multiplier = new GameAttributeType(nameof(chain_zap_multiplier));

    // EXPLODE
    public static readonly GameAttributeType explode_chain = new GameAttributeType(nameof(explode_chain));
    public static readonly GameAttributeType explode_charge_time = new GameAttributeType(nameof(explode_charge_time));
    public static readonly GameAttributeType explode_charge_time_perc = new GameAttributeType(nameof(explode_charge_time_perc));
    public static readonly GameAttributeType explode_charge_time_reduc = new GameAttributeType(nameof(explode_charge_time_reduc));
    public static readonly GameAttributeType explode_cooldown = new GameAttributeType(nameof(explode_cooldown));
    public static readonly GameAttributeType explode_fragments = new GameAttributeType(nameof(explode_fragments));
    public static readonly GameAttributeType explode_minis = new GameAttributeType(nameof(explode_minis));
    public static readonly GameAttributeType explode_radius = new GameAttributeType(nameof(explode_radius));
    public static readonly GameAttributeType explode_slow = new GameAttributeType(nameof(explode_slow));
    public static readonly GameAttributeType explode_split = new GameAttributeType(nameof(explode_split));
    public static readonly GameAttributeType explode_delayed = new GameAttributeType(nameof(explode_delayed));

    // EXPLODE (ROLLOUT)
    public static readonly GameAttributeType rollout_cooldown = new GameAttributeType(nameof(rollout_cooldown));
    public static readonly GameAttributeType rollout_count = new GameAttributeType(nameof(rollout_count));
    public static readonly GameAttributeType rollout_radius = new GameAttributeType(nameof(rollout_radius));
    public static readonly GameAttributeType rollout_radius_delta = new GameAttributeType(nameof(rollout_radius_delta));
    public static readonly GameAttributeType rollout_offset = new GameAttributeType(nameof(rollout_offset));
    public static readonly GameAttributeType rollout_delay = new GameAttributeType(nameof(rollout_delay));
    public static readonly GameAttributeType rollout_line_count = new GameAttributeType(nameof(rollout_line_count)); // SPLIT
    public static readonly GameAttributeType rollout_behind = new GameAttributeType(nameof(rollout_behind)); // BOOMERANG
    public static readonly GameAttributeType rollout_minis = new GameAttributeType(nameof(rollout_minis)); // MINES
    public static readonly GameAttributeType rollout_zap = new GameAttributeType(nameof(rollout_zap)); // CHAIN

    // MINES
    public static readonly GameAttributeType mines_cooldown = new GameAttributeType(nameof(mines_cooldown));
    public static readonly GameAttributeType mines_double_shell = new GameAttributeType(nameof(mines_double_shell));
    public static readonly GameAttributeType mines_chain = new GameAttributeType(nameof(mines_chain));
    public static readonly GameAttributeType mines_explode = new GameAttributeType(nameof(mines_explode));
    public static readonly GameAttributeType mines_shell_count = new GameAttributeType(nameof(mines_shell_count));
    public static readonly GameAttributeType mines_fragment_count = new GameAttributeType(nameof(mines_fragment_count));
    public static readonly GameAttributeType mines_fragment_lifetime = new GameAttributeType(nameof(mines_fragment_lifetime));
    public static readonly GameAttributeType mines_fragment_pierce = new GameAttributeType(nameof(mines_fragment_pierce));
    public static readonly GameAttributeType mines_fragment_curve = new GameAttributeType(nameof(mines_fragment_curve));

    // SPLIT
    public static readonly GameAttributeType split_piercing_count = new GameAttributeType(nameof(split_piercing_count));
    public static readonly GameAttributeType split_arc = new GameAttributeType(nameof(split_arc));
    public static readonly GameAttributeType split_chain = new GameAttributeType(nameof(split_chain));
    public static readonly GameAttributeType split_cooldown = new GameAttributeType(nameof(split_cooldown));
    public static readonly GameAttributeType split_count = new GameAttributeType(nameof(split_count));
    public static readonly GameAttributeType split_explode = new GameAttributeType(nameof(split_explode));
    public static readonly GameAttributeType split_fragments = new GameAttributeType(nameof(split_fragments));
    public static readonly GameAttributeType split_size = new GameAttributeType(nameof(split_size));
    public static readonly GameAttributeType split_boomerang = new GameAttributeType(nameof(split_boomerang));
    public static readonly GameAttributeType split_lifetime = new GameAttributeType(nameof(split_lifetime));

    // BOOMERANG
    public static readonly GameAttributeType boomerang_cooldown = new GameAttributeType(nameof(boomerang_cooldown));
    public static readonly GameAttributeType boomerang_count = new GameAttributeType(nameof(boomerang_count));
    public static readonly GameAttributeType boomerang_size = new GameAttributeType(nameof(boomerang_size));
    public static readonly GameAttributeType boomerang_distance = new GameAttributeType(nameof(boomerang_distance));
    public static readonly GameAttributeType boomerang_catch_cooldown = new GameAttributeType(nameof(boomerang_catch_cooldown));
    public static readonly GameAttributeType boomerang_lifetime = new GameAttributeType(nameof(boomerang_lifetime));
    public static readonly GameAttributeType boomerang_explode = new GameAttributeType(nameof(boomerang_explode));
    public static readonly GameAttributeType boomerang_fragment = new GameAttributeType(nameof(boomerang_fragment));
    public static readonly GameAttributeType boomerang_chain = new GameAttributeType(nameof(boomerang_chain));
    public static readonly GameAttributeType boomerang_linger_time = new GameAttributeType(nameof(boomerang_linger_time));

    // ORBIT
    public static readonly GameAttributeType orbit_projectile_time = new GameAttributeType(nameof(orbit_projectile_time));
    public static readonly GameAttributeType orbit_projectile_size = new GameAttributeType(nameof(orbit_projectile_size));
    public static readonly GameAttributeType orbit_projectile_count = new GameAttributeType(nameof(orbit_projectile_count));
    public static readonly GameAttributeType orbit_ring_count = new GameAttributeType(nameof(orbit_ring_count));
    public static readonly GameAttributeType orbit_ring_radius = new GameAttributeType(nameof(orbit_ring_radius));

    public static readonly GameAttributeType orbit_explode = new GameAttributeType(nameof(orbit_explode));
    public static readonly GameAttributeType orbit_chain = new GameAttributeType(nameof(orbit_chain));
    public static readonly GameAttributeType orbit_boomerang = new GameAttributeType(nameof(orbit_boomerang));
    public static readonly GameAttributeType orbit_split = new GameAttributeType(nameof(orbit_split));
    public static readonly GameAttributeType orbit_mines = new GameAttributeType(nameof(orbit_mines));

    // PLAYER
    public static readonly GameAttributeType player_armor = new GameAttributeType(nameof(player_armor));
    public static readonly GameAttributeType player_health = new GameAttributeType(nameof(player_health));
    public static readonly GameAttributeType player_velocity = new GameAttributeType(nameof(player_velocity));
    public static readonly GameAttributeType player_acceleration = new GameAttributeType(nameof(player_acceleration));
    public static readonly GameAttributeType player_avoid_damage_chance = new GameAttributeType(nameof(player_avoid_damage_chance));
    public static readonly GameAttributeType player_global_cooldown_multiplier = new GameAttributeType(nameof(player_global_cooldown_multiplier));
    public static readonly GameAttributeType player_exp_multiplier = new GameAttributeType(nameof(player_exp_multiplier));

    // DODGE
    public static readonly GameAttributeType dodge_cooldown = new GameAttributeType(nameof(dodge_cooldown));
    public static readonly GameAttributeType dodge_distance = new GameAttributeType(nameof(dodge_distance));
    public static readonly GameAttributeType dodge_knockback = new GameAttributeType(nameof(dodge_knockback));

    // HEAL
    public static readonly GameAttributeType heal_kill_value = new GameAttributeType(nameof(heal_kill_value));
    public static readonly GameAttributeType heal_max_value = new GameAttributeType(nameof(heal_max_value));
    public static readonly GameAttributeType heal_cooldown = new GameAttributeType(nameof(heal_cooldown));
}