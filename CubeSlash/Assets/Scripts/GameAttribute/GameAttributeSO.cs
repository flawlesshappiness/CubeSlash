using UnityEngine;

[CreateAssetMenu(fileName = nameof(GameAttributeSO), menuName = "Game/" + nameof(GameAttributeSO), order = 1)]
public class GameAttributeSO : ScriptableObject
{
    public GameAttribute attribute;
}