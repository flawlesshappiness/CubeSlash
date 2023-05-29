[System.Serializable]
public class PlayerBodyType : FakeEnum
{
    public PlayerBodyType(string id) : base(id) { }

    public static readonly PlayerBodyType cell_body = new PlayerBodyType(nameof(cell_body));
    public static readonly PlayerBodyType plant_body = new PlayerBodyType(nameof(plant_body));
    public static readonly PlayerBodyType meat_body = new PlayerBodyType(nameof(meat_body));
}