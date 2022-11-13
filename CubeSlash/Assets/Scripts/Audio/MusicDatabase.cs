using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(MusicDatabase), menuName = "Game/" + nameof(MusicDatabase), order = 1)]
public class MusicDatabase : Database
{
    public FMODEventReference bgm_start_game;
    public List<FMODEventReference> bgms_progress = new List<FMODEventReference>();
}