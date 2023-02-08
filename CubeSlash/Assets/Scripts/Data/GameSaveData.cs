using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameSaveData : SaveDataObject
{
    public float volume_master = 1;
    public float volume_music = 1;
    public float volume_sfx = 1;

    public int runs_completed = 0;
    public int idx_difficulty_completed = -1;
}
