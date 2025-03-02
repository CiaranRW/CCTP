using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using NUnit.Framework.Internal;

public class EnemySpawnerAuthoring : MonoBehaviour
{
    public float spawnCooldown;
    public List<EnemySO> enemiesSO;

    public class EnemySpawnerBaker : Baker<EnemySpawnerAuthoring>
    {
        public override void Bake(EnemySpawnerAuthoring authoring)
        {

            Entity enemySpawnerAuthoring = GetEntity(TransformUsageFlags.None);

            AddComponent(enemySpawnerAuthoring, new EnemySpawnerComponent
            {
                spawnCooldown = authoring.spawnCooldown,
            });


            List<EnemyData> enemyData = new List<EnemyData>();

            foreach (EnemySO e in authoring.enemiesSO)
            {
                enemyData.Add(new EnemyData
                {
                    level = e.level,
                    prefab = GetEntity(e.prefab, TransformUsageFlags.None)
                });
            }
            AddComponentObject(enemySpawnerAuthoring, new EnemyDataContainer { enemies = enemyData });
        }
    }
}

public struct EnemySpawnerComponent : IComponentData
{
    public float spawnCooldown;
}

