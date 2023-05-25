using System.Collections.Generic;

[System.Serializable]
public class PlayerBodySaveData : SaveDataObject
{
    public PlayerBodyType body_type = PlayerBodyType.cell_body;
    public int body_skin = 0;
    public Ability.Type primary_ability = Ability.Type.SPLIT;
    public List<BodypartSavaData> bodyparts = new List<BodypartSavaData>();
}

[System.Serializable]
public class BodypartSavaData
{
    public BodypartType type;
    public float position;
    public float size;
}