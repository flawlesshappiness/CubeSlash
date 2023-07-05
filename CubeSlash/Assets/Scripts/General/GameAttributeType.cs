public partial class GameAttributeType : FakeEnum
{
    // CHAIN
    public static readonly GameAttributeType chain_fragments = new GameAttributeType(nameof(chain_fragments));
    public static readonly GameAttributeType chain_strikes = new GameAttributeType(nameof(chain_strikes));
    public static readonly GameAttributeType chain_chains = new GameAttributeType(nameof(chain_chains));
    public static readonly GameAttributeType chain_cooldown = new GameAttributeType(nameof(chain_cooldown));
    public static readonly GameAttributeType chain_explosion_radius = new GameAttributeType(nameof(chain_explosion_radius));
    public static readonly GameAttributeType chain_radius = new GameAttributeType(nameof(chain_radius));

    // EXPLODE
    public static readonly GameAttributeType explode_chain = new GameAttributeType(nameof(explode_chain));
    public static readonly GameAttributeType explode_charge_time = new GameAttributeType(nameof(explode_charge_time));
    public static readonly GameAttributeType explode_cooldown = new GameAttributeType(nameof(explode_cooldown));
    public static readonly GameAttributeType explode_fragments = new GameAttributeType(nameof(explode_fragments));
    public static readonly GameAttributeType explode_minis = new GameAttributeType(nameof(explode_minis));
    public static readonly GameAttributeType explode_radius = new GameAttributeType(nameof(explode_radius));
    public static readonly GameAttributeType explode_slow = new GameAttributeType(nameof(explode_slow));
    public static readonly GameAttributeType explode_split = new GameAttributeType(nameof(explode_split));

    // MINES
    public static readonly GameAttributeType mines_cooldown = new GameAttributeType(nameof(mines_cooldown));
    public static readonly GameAttributeType mines_double_shell = new GameAttributeType(nameof(mines_double_shell));
    public static readonly GameAttributeType mines_chain = new GameAttributeType(nameof(mines_chain));
    public static readonly GameAttributeType mines_explode = new GameAttributeType(nameof(mines_explode));
    public static readonly GameAttributeType mines_shell_count = new GameAttributeType(nameof(mines_shell_count));
    public static readonly GameAttributeType mines_fragment_count = new GameAttributeType(nameof(mines_fragment_count));

    // SPLIT
    public static readonly GameAttributeType split_piercing_count = new GameAttributeType(nameof(split_piercing_count));
    public static readonly GameAttributeType split_arc = new GameAttributeType(nameof(split_arc));
    public static readonly GameAttributeType split_chain = new GameAttributeType(nameof(split_chain));
    public static readonly GameAttributeType split_cooldown = new GameAttributeType(nameof(split_cooldown));
    public static readonly GameAttributeType split_count = new GameAttributeType(nameof(split_count));
    public static readonly GameAttributeType split_explode = new GameAttributeType(nameof(split_explode));
    public static readonly GameAttributeType split_fragments = new GameAttributeType(nameof(split_fragments));
    public static readonly GameAttributeType split_size = new GameAttributeType(nameof(split_size));

    // PLAYER
    public static readonly GameAttributeType player_armor = new GameAttributeType(nameof(player_armor));
    public static readonly GameAttributeType player_health = new GameAttributeType(nameof(player_health));
    public static readonly GameAttributeType player_velocity = new GameAttributeType(nameof(player_velocity));
    public static readonly GameAttributeType player_acceleration = new GameAttributeType(nameof(player_acceleration));
    public static readonly GameAttributeType player_avoid_damage_chance = new GameAttributeType(nameof(player_avoid_damage_chance));
    public static readonly GameAttributeType player_global_cooldown_multiplier = new GameAttributeType(nameof(player_global_cooldown_multiplier));
    public static readonly GameAttributeType player_exp_multiplier = new GameAttributeType(nameof(player_exp_multiplier));
}