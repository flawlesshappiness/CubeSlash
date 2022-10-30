using UnityEngine;

[CreateAssetMenu(fileName = "FMODEventReferenceDatabase", menuName = "Game/FMODEventReferenceDatabase", order = 1)]
public class FMODEventReferenceDatabase : ScriptableObject
{
    public FMODEventReference collect_experience;

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