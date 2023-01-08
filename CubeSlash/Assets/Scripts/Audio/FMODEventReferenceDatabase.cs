using UnityEngine;

[CreateAssetMenu(fileName = "FMODEventReferenceDatabase", menuName = "Game/FMODEventReferenceDatabase", order = 1)]
public class FMODEventReferenceDatabase : ScriptableObject
{
    public FMODEventReference collect_experience;
    public FMODEventReference lose_game;
    public FMODEventReference sfx_chain_zap;
    public FMODEventReference sfx_explode_charge;
    public FMODEventReference sfx_explode_explode;

    private static FMODEventReferenceDatabase instance;

    public static FMODEventReferenceDatabase Load()
    {
        if(instance == null)
        {
            instance = Resources.Load<FMODEventReferenceDatabase>("Databases/" + nameof(FMODEventReferenceDatabase));
        }

        return instance;
    }
}