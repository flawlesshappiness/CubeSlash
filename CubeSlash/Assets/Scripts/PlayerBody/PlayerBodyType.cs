[System.Serializable]
public class PlayerBodyType : FakeEnum
{
    public PlayerBodyType(string id) : base(id) { }

    public static readonly PlayerBodyType cell_body = new PlayerBodyType(nameof(cell_body));
}