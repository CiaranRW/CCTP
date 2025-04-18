using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using Unity.Physics;

partial struct EnemySpawningSystem : ISystem
{
    private Random random;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();

        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;
        Random random = new Random(seed); // Create random here

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

            float3 newPosition = GetPositionOutsideOfCameraRange(ref state, ref random);

            if (enemySpawner.ValueRO.spawnedEntity == Entity.Null)
            {
                Entity entity = state.EntityManager.Instantiate(entitiesReferences.player);
                enemySpawner.ValueRW.spawnedEntity = entity;
                SystemAPI.SetComponent(entity, LocalTransform.FromPosition(newPosition));
            }
            else
            {
                SystemAPI.SetComponent(enemySpawner.ValueRW.spawnedEntity, new LocalTransform
                {
                    Position = newPosition,
                    Rotation = quaternion.identity,
                    Scale = 1
                });
            }
        }
    }
    private float3 GetPositionOutsideOfCameraRange(ref SystemState state, ref Random random)
    {
        float3 position = random.NextFloat3(new float3(-15, 0, -15), new float3(15, 0, 15));

        if (!SystemAPI.HasSingleton<PhysicsWorldSingleton>())
            return position; // fallback if physics not ready

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

        if (collisionWorld.CastRay(rayInput, out var hit))
        {
            position.y = hit.Position.y;
        }
        else
        {
            position.y = 0;
        }

        return position;
    }
}


