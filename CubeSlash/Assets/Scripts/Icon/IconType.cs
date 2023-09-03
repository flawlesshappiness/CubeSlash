[System.Serializable]
public class IconType : FakeEnum
{
    public IconType(string id) : base(id) { }

    public static readonly IconType start_game = new IconType(nameof(start_game));
    public static readonly IconType settings = new IconType(nameof(settings));
    public static readonly IconType quit = new IconType(nameof(quit));
    public static readonly IconType arrow_back = new IconType(nameof(arrow_back));
    public static readonly IconType customize = new IconType(nameof(customize));
    public static readonly IconType select_ability = new IconType(nameof(select_ability));
    public static readonly IconType credits = new IconType(nameof(credits));

    public static readonly IconType customize_option_body = new IconType(nameof(customize_option_body));
    public static readonly IconType customize_add_part = new IconType(nameof(customize_add_part));
    public static readonly IconType customize_remove_part = new IconType(nameof(customize_remove_part));
    public static readonly IconType customize_move_part = new IconType(nameof(customize_move_part));
}