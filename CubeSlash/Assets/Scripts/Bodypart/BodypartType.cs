[System.Serializable]
public class BodypartType : FakeEnum
{
    public BodypartType(string id) : base(id) { }

    public static readonly BodypartType ability_dash_A = new BodypartType(nameof(ability_dash_A));
    public static readonly BodypartType ability_split_A = new BodypartType(nameof(ability_split_A));
    public static readonly BodypartType ability_explode_A = new BodypartType(nameof(ability_explode_A));
    public static readonly BodypartType ability_mines_A = new BodypartType(nameof(ability_mines_A));
    public static readonly BodypartType ability_chain_A = new BodypartType(nameof(ability_chain_A));
    public static readonly BodypartType ability_boomerang_A = new BodypartType(nameof(ability_boomerang_A));
    public static readonly BodypartType bone_A = new BodypartType(nameof(bone_A));
    public static readonly BodypartType bone_B = new BodypartType(nameof(bone_B));
    public static readonly BodypartType crystal_A = new BodypartType(nameof(crystal_A));
    public static readonly BodypartType crystal_B = new BodypartType(nameof(crystal_B));
    public static readonly BodypartType crystal_C = new BodypartType(nameof(crystal_C));
    public static readonly BodypartType crystal_D = new BodypartType(nameof(crystal_D));
    public static readonly BodypartType eye_A = new BodypartType(nameof(eye_A));
    public static readonly BodypartType eye_B = new BodypartType(nameof(eye_B));
    public static readonly BodypartType eyestalk_A = new BodypartType(nameof(eyestalk_A));
    public static readonly BodypartType puff_A = new BodypartType(nameof(puff_B));
    public static readonly BodypartType puff_B = new BodypartType(nameof(puff_B));
    public static readonly BodypartType tail_A = new BodypartType(nameof(tail_A));
    public static readonly BodypartType tentacle_A = new BodypartType(nameof(tentacle_A));
    public static readonly BodypartType fin_A = new BodypartType(nameof(fin_A));
    public static readonly BodypartType fin_B = new BodypartType(nameof(fin_B));
    public static readonly BodypartType mushroom_A = new BodypartType(nameof(mushroom_A));
    public static readonly BodypartType volatile_A = new BodypartType(nameof(volatile_A));
    public static readonly BodypartType whiskers_A = new BodypartType(nameof(whiskers_A));
}