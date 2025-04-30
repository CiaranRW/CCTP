using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using System.Collections.Generic;
using Unity.Physics;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
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
    [BurstCompile]
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
    [BurstCompile]
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

        if (!deadEntitiesQuery.IsEmpty)
        {
            var deadEntities = deadEntitiesQuery.ToEntityArray(Allocator.TempJob);

            if (deadEntities.Length > 0)
            {
                recycledEntity = deadEntities[0];
            }

            deadEntities.Dispose();
        }

        if (recycledEntity != Entity.Null)
        {
            ResetRecycledEntity(recycledEntity);
            return;
        }

        Entity newEnemy = EntityManager.Instantiate(selectedEnemyPrefab);
        ResetRecycledEntity(newEnemy);
    }
    [BurstCompile]
    private void ResetRecycledEntity(Entity entity)
    {
        EntityManager.SetComponentData(entity, new Health { healthAmount = 100 });
        EntityManager.SetComponentData(entity, new LocalTransform
        {
            Position = GetPositionOutsideOfCameraRange(),
            Rotation = quaternion.identity,
            Scale = 1
        });

        if (EntityManager.HasComponent<UnitMover>(entity))
        {
            var unitMover = EntityManager.GetComponentData<UnitMover>(entity);
            unitMover.moveSpeed = 0.2f;
            EntityManager.SetComponentData(entity, unitMover);
        }

        if (!EntityManager.HasComponent<Blue>(entity))
        {
            EntityManager.AddComponent<Blue>(entity);
        }


        EntitiesReferences references = SystemAPI.GetSingleton<EntitiesReferences>();

        Entity prefab = references.zombiePrefab;
        PhysicsCollider originalCollider = EntityManager.GetComponentData<PhysicsCollider>(prefab);

        EntityManager.SetComponentData(entity, originalCollider);

        EntityManager.RemoveComponent<DeadTag>(entity);
    }

    private float3 GetPositionOutsideOfCameraRange()
    {
        float3 position = float3.zero;
        const int maxAttempts = 10;

        for (int i = 0; i < maxAttempts; i++)
        {
            float3 randomPos = random.NextFloat3(new float3(-50, 0, -50), new float3(50, 0, 50));
            Vector3 navQueryPos = new Vector3(randomPos.x, 0.5f, randomPos.z);

            if (UnityEngine.AI.NavMesh.SamplePosition(navQueryPos, out var hit, 50f, UnityEngine.AI.NavMesh.AllAreas))
            {
                position = new float3(hit.position.x, hit.position.y, hit.position.z);
                Debug.DrawRay(navQueryPos, Vector3.up * 2f, Color.green, 2f);
                break;
            }
            else
            {
                Debug.DrawRay(navQueryPos, Vector3.up * 2f, Color.red, 2f);
            }
        }

        if (position.Equals(float3.zero))
        {
            position = random.NextFloat3(new float3(-50, 0, -50), new float3(50, 0, 50));
        }

        if (!SystemAPI.HasSingleton<PhysicsWorldSingleton>())
            return position;

        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

        var rayInput = new RaycastInput
        {
            Start = position + new float3(0, 50, 0),
            End = position + new float3(0, -50, 0),
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u,
                GroupIndex = 0
            }
        };

        if (collisionWorld.CastRay(rayInput, out var groundHit))
        {
            position.y = groundHit.Position.y + 0.1f;
        }

        return position;
    }
}