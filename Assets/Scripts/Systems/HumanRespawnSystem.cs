using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public partial struct HumanRespawnSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        foreach ((RefRW<LocalTransform> localTransform, RefRW<MoveTimer> moveTimer) in
                 SystemAPI.Query<RefRW<LocalTransform>, RefRW<MoveTimer>>()
                 .WithAll<Red>())
        {
            moveTimer.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;
            Random random = new Random(seed);

            if (moveTimer.ValueRW.timer <= 0f)
            {
                moveTimer.ValueRW.timer = random.NextFloat(moveTimer.ValueRW.minTime, moveTimer.ValueRW.maxTime);
                localTransform.ValueRW.Position = GetPositionOutsideOfCameraRange(ref state, ref random);
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