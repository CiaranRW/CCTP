using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 5f;
    public const float SEPARATION_RADIUS = 3f;
    public const float SEPARATION_FORCE_MULTIPLIER = 3f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spatialMap = new NativeParallelMultiHashMap<int, EntityPosition>(1024, Allocator.TempJob);

        foreach (var (localTransform, entity) in SystemAPI.Query<LocalTransform>().WithAll<Blue>().WithEntityAccess())
        {
            float3 position = localTransform.Position;
            int2 cell = GridUtils.WorldToGrid(position);
            int hash = (int)GridUtils.HashCell(cell);
            spatialMap.Add(hash, new EntityPosition { entity = entity, position = position });
        }

        var job = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            spatialMap = spatialMap.AsReadOnly()
        };

        job.ScheduleParallel();

        spatialMap.Dispose(state.Dependency);
    }
}

[BurstCompile]
[WithAll(typeof(Blue))]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    [ReadOnly] public NativeParallelMultiHashMap<int, EntityPosition>.ReadOnly spatialMap;

    public void Execute(Entity entity, ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 position = localTransform.Position;
        float3 moveDirection = unitMover.targetPosition - position;
        moveDirection.y = 0;

        if (math.lengthsq(moveDirection) <= UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }

        float3 targetPosition = unitMover.targetPosition;

        float3 separationForce = CalculateSeparationForce(entity, position, targetPosition);
        //Debug.DrawRay(position, separationForce * 3f, Color.red, 0.1f, false);
        //Debug.DrawRay(position, moveDirection, Color.green, 0.1f, false);

        float3 combined = math.normalize(moveDirection + separationForce);
        //Debug.DrawRay(position, combined * 3f, Color.blue, 0.1f, false);

        float3 forward = math.normalize(new float3(combined.x, 0, combined.z));
        if (!math.any(math.isnan(forward)))
        {
            localTransform.Rotation = quaternion.LookRotationSafe(forward, math.up());
        }

        physicsVelocity.Linear = forward * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }

    private float3 CalculateSeparationForce(Entity self, float3 position, float3 targetPosition)
    {
        float3 force = float3.zero;
        const float MAX_SEPARATION_FORCE = 5f;

        float radiusSq = UnitMoverSystem.SEPARATION_RADIUS * UnitMoverSystem.SEPARATION_RADIUS;

        float3 toTarget = targetPosition - position;
        float targetDistance = math.length(toTarget);

        int2 cell = GridUtils.WorldToGrid(position);

        // Smoother separation modifier: more distance = less force
        float separationModifier = math.saturate(targetDistance / 15f);
        separationModifier = 1f - separationModifier;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int2 neighborCell = cell + new int2(x, y);
                int neighborHash = (int)GridUtils.HashCell(neighborCell);

                if (spatialMap.TryGetFirstValue(neighborHash, out var entry, out var iterator))
                {
                    do
                    {
                        if (entry.entity == self) continue;

                        float distSq = math.distancesq(position, entry.position);
                        if (distSq < radiusSq)
                        {
                            float3 dir = position - entry.position;
                            force += math.normalize(dir) / math.sqrt(distSq + 0.01f);
                        }
                    }
                    while (spatialMap.TryGetNextValue(out entry, ref iterator));
                }
            }
        }

        force *= UnitMoverSystem.SEPARATION_FORCE_MULTIPLIER * separationModifier;
        float forceLengthSq = math.lengthsq(force);
        if (forceLengthSq > MAX_SEPARATION_FORCE * MAX_SEPARATION_FORCE)
        {
            force = math.normalize(force) * MAX_SEPARATION_FORCE;
        }

        return force;
    }
}

public struct EntityPosition
{
    public Entity entity;
    public float3 position;
}
