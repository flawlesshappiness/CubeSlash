using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Game/EnemyDatabase", order = 1)]
public class EnemyDatabase : Database<EnemySettings>
{
    public EnemySettings Get(EnemyType type) => collection.FirstOrDefault(x => x.type == type);
}