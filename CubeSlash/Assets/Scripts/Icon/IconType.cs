[System.Serializable]
public class IconType : FakeEnum
{
    public IconType(string id) : base(id) { }

    public static readonly IconType start_game = new IconType(nameof(start_game));
    public static readonly IconType settings = new IconType(nameof(settings));
    public static readonly IconType quit = new IconType(nameof(quit));
    public static readonly IconType arrow_back = new IconType(nameof(arrow_back));
    public static readonly IconType customize_body = new IconType(nameof(customize_body));
}