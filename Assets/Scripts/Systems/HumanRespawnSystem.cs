using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

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
        float3 position = random.NextFloat3(new float3(-50, 0, -50), new float3(50, 0, 50));

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

        if (collisionWorld.CastRay(rayInput, out var hit))
        {
            position.y = hit.Position.y + 0.2f;
        }
        else
        {
            position.y = 0;
        }

        return position;
    }
}