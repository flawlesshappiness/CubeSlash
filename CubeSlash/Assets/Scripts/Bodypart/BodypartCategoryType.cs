[System.Serializable]
public class BodypartCategoryType : FakeEnum
{
    public BodypartCategoryType(string id) : base(id) { }

    public static readonly BodypartCategoryType eyes = new BodypartCategoryType(nameof(eyes));
}