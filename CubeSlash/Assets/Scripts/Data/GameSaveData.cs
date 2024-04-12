using System.Collections.Generic;

[System.Serializable]
public class GameSaveData : SaveDataObject
{
    public int runs_completed = 0;
    public int idx_difficulty_completed = -1;

    public int idx_gamesetup_ability = -1;
    public int idx_gamesetup_charm = -1;
    public int idx_gamesetup_difficulty = 0;

    public GamemodeType gamemode_selected = GamemodeType.Normal;
    public List<GamemodeType> unlocked_gamemodes = new List<GamemodeType> { GamemodeType.Normal };

    public int count_wins = 0;
    public int count_losses = 0;

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

    public override void Clear()
    {
        idx_gamesetup_ability = -1;
        idx_gamesetup_charm = -1;
        idx_gamesetup_difficulty = 0;

        new_abilities.Clear();
        unlocked_abilities.Clear();
        new_bodyparts.Clear();
        unlocked_bodyparts.Clear();
        new_player_bodies.Clear();
        unlocked_player_bodies.Clear();

        unlocked_abilities.Add(Ability.Type.DASH);
        unlocked_abilities.Add(Ability.Type.SPLIT);
        unlocked_bodyparts.Add(BodypartType.eye_A);
        unlocked_player_bodies.Add(PlayerBodyType.Cell);

        gamemode_selected = GamemodeType.Normal;
        unlocked_gamemodes.Clear();
        unlocked_gamemodes.Add(GamemodeType.Normal);
    }
}
