using System.Collections.Generic;

[System.Serializable]
public class GameSaveData : SaveDataObject
{
    public Dictionary<FMODBusType, float> volumes = new Dictionary<FMODBusType, float>()
    {
        { FMODBusType.Master, 0.5f },
        { FMODBusType.Music, 1f },
        { FMODBusType.SFX, 1f },
        { FMODBusType.UI, 1f },
    };

    public int runs_completed = 0;
    public int idx_difficulty_completed = -1;

    public int idx_gamesetup_ability = -1;
    public int idx_gamesetup_charm = -1;
    public int idx_gamesetup_difficulty = 0;

    public List<Ability.Type> new_abilities = new List<Ability.Type>();
    public List<Ability.Type> unlocked_abilities = new List<Ability.Type>()
    {
        Ability.Type.DASH,
        Ability.Type.SPLIT
    };

    public List<BodypartType> new_bodyparts = new List<BodypartType>();
    public List<BodypartType> unlocked_bodyparts = new List<BodypartType>()
    {
        BodypartType.eye_A,
    };

    public List<PlayerBodyType> new_player_bodies = new List<PlayerBodyType>();
    public List<PlayerBodyType> unlocked_player_bodies = new List<PlayerBodyType>()
    {
        PlayerBodyType.Cell,
    };
}
