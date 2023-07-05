using System.Linq;

[System.Serializable]
public partial class GameAttributeType : FakeEnum
{
    public GameAttributeType() : base(GetAll(typeof(GameAttributeType)).First().id) { }
    public GameAttributeType(string id) : base(id) { }

    public static readonly GameAttributeType BaseType = new GameAttributeType(nameof(BaseType));
}