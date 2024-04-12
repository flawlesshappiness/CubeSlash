using System.Linq;

public class GamemodeController : Singleton
{
    public static GamemodeController Instance => Instance<GamemodeController>();
    public GamemodeDatabase DB => Database.Load<GamemodeDatabase>();
    public GamemodeSettings SelectedGameMode => GetGamemode(Save.Game.gamemode_selected);
    public GamemodeSettings GetGamemode(GamemodeType type) => DB.collection.FirstOrDefault(x => x.type == type);

    protected override void Initialize()
    {
        base.Initialize();
        ValidateSaveData();
    }

    public void SetGamemode(GamemodeSettings gamemode)
    {
        LogController.LogMethod(gamemode.type.ToString());
        Save.Game.gamemode_selected = gamemode.type;
        SaveDataController.Instance.Save<GameSaveData>();
    }

    public bool TryUnlockGamemode(GamemodeType type)
    {
        if (Save.Game.unlocked_gamemodes.Contains(type)) return false;
        Save.Game.unlocked_gamemodes.Add(type);
        return true;
    }

    public bool IsGamemodeUnlocked(GamemodeType type)
    {
        return Save.Game.unlocked_gamemodes.Contains(type);
    }

    public void ValidateSaveData()
    {
        // Won on easy
        if (Save.Game.idx_difficulty_completed > -1)
        {
            TryUnlockGamemode(GamemodeType.Medium);
            TryUnlockGamemode(GamemodeType.DoubleBoss);
        }

        // Won on medium
        if (Save.Game.idx_difficulty_completed > 0)
        {
            TryUnlockGamemode(GamemodeType.Hard);
        }

        // Distinct
        Save.Game.unlocked_gamemodes = Save.Game.unlocked_gamemodes
            .Distinct()
            .ToList();

        SaveDataController.Instance.Save<GameSaveData>();
    }
}