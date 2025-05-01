using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using Unity.Physics;
using System.Linq;

partial struct EnemySpawningSystem : ISystem
{

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        int redEntityCount = 0;

        foreach (var entity in SystemAPI.Query<RefRO<Red>>())
        {
            redEntityCount++;
        }

        if (redEntityCount >= 20)
            return;

        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;
        Random random = new Random(seed);

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<EnemySpawner> enemySpawner)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<EnemySpawner>>())
        {
            enemySpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (enemySpawner.ValueRO.timer > 0f)
                continue;

            enemySpawner.ValueRW.timer = enemySpawner.ValueRO.timerMax;


            float3 spawnPosition = GetPositionOutsideOfCameraRange(ref state, ref random);

            Entity entity = state.EntityManager.Instantiate(entitiesReferences.humanPrefab);
            SystemAPI.SetComponent(entity, new LocalTransform
            {
                Position = spawnPosition,
                Rotation = quaternion.identity,
                Scale = 1f
            });

            if (state.EntityManager.HasComponent<MoveTimer>(entity))
            {
                MoveTimer moveTimer = state.EntityManager.GetComponentData<MoveTimer>(entity);
                moveTimer.timer = random.NextFloat(moveTimer.minTime, moveTimer.maxTime);
                state.EntityManager.SetComponentData(entity, moveTimer);
            }


        }
    }
    private float3 GetPositionOutsideOfCameraRange(ref SystemState state, ref Random random)
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
