using System.Collections.Generic;
using Unity.Entities;

public class EnemyDataContainer : IComponentData
{
    public List<EnemyData> enemies;
}

public struct EnemyData
{
    public int level; // Maybe add elite enemies.
    public Entity prefab;
    public float health;
    public float speed;
    public float damage;
    //public int team;
}
