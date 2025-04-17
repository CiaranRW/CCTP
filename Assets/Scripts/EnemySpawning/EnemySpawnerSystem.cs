using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using System.Collections.Generic;

public partial class EnemySpawnerSystem : SystemBase
{
    private Entity enemySpawnerEntity;
    private EnemySpawnerComponent enemySpawnerComponent;
    private EnemyDataContainer enemyDataContainerComponent;
    private float nextSpawnTime;
    private Random random;

    protected override void OnCreate()
    {
        random = Random.CreateFromIndex(1);
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.TryGetSingletonEntity<EnemySpawnerComponent>(out enemySpawnerEntity))
        {
            return;
        }

        enemySpawnerComponent = EntityManager.GetComponentData<EnemySpawnerComponent>(enemySpawnerEntity);

        if (!enemySpawnerComponent.isSpawning)
        {
            return;
        }

        enemyDataContainerComponent = EntityManager.GetComponentObject<EnemyDataContainer>(enemySpawnerEntity);

        if (SystemAPI.Time.ElapsedTime > nextSpawnTime)
        {
            SpawnEnemy();
            nextSpawnTime = (float)SystemAPI.Time.ElapsedTime + enemySpawnerComponent.spawnCooldown;
        }
    }

    private void SpawnEnemy()
    {
        int level = 2;
        List<EnemyData> availableEnemies = new List<EnemyData>();

        foreach (EnemyData enemyData in enemyDataContainerComponent.enemies)
        {
            if (enemyData.level <= level)
            {
                availableEnemies.Add(enemyData);
            }
        }


        if (availableEnemies.Count == 0)
        {
            return;
        }

        int index = random.NextInt(availableEnemies.Count);
        Entity selectedEnemyPrefab = availableEnemies[index].prefab;

        EntityQuery deadEntitiesQuery = SystemAPI.QueryBuilder()
                    .WithAll<DeadTag, Health, LocalTransform>()
                    .Build();

        Entity recycledEntity = Entity.Null;

        // Check if there are any available dead entities to reuse
        if (!deadEntitiesQuery.IsEmpty)
        {
            recycledEntity = deadEntitiesQuery.GetSingletonEntity();
        }

        if (recycledEntity != Entity.Null)
        {
            ResetRecycledEntity(recycledEntity);
            return; // Recycled, no need to instantiate a new one
        }

        // No recycled entity found, instantiate a new one
        Entity newEnemy = EntityManager.Instantiate(selectedEnemyPrefab);
        ResetRecycledEntity(newEnemy); // Reset components for new or recycled entity
    }

    private void ResetRecycledEntity(Entity entity)
    {
        // Reset health and position (the components you want to reset)
        EntityManager.SetComponentData(entity, new Health { healthAmount = 100 });
        EntityManager.SetComponentData(entity, new LocalTransform
        {
            Position = GetPositionOutsideOfCameraRange(),
            Rotation = quaternion.identity,
            Scale = 1
        });

        // Remove the DeadTag to make it ready for future spawning
        EntityManager.RemoveComponent<DeadTag>(entity);
    }




    private float3 GetPositionOutsideOfCameraRange()
    {
        float3 position = new float3(random.NextFloat3(-15, 15));
        position.y = 0;

        return position;
    }
}