using System.Collections.Generic;

[System.Serializable]
public class PlayerBodySaveData : SaveDataObject
{
    public List<BodypartSavaData> bodyparts = new List<BodypartSavaData>();
}

[System.Serializable]
public class BodypartSavaData
{
    public BodypartType type;
    public float position;
}