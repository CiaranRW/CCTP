using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;
    public const float SEPARATION_RADIUS = 2f;
    public const float SEPARATION_FORCE_MULTIPLIER = 2f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spatialMap = new NativeParallelMultiHashMap<int, float3>(1024, Allocator.TempJob);

        // Loop directly through entities with the ZombieTag
        foreach (var (localTransform, unitMover, physicsVelocity) in SystemAPI.Query<LocalTransform, UnitMover, PhysicsVelocity>().WithAll<Blue>())
        {
            float3 position = localTransform.Position;
            int2 cell = GridUtils.WorldToGrid(position);
            int hash = (int)GridUtils.HashCell(cell);

            spatialMap.Add(hash, position);
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
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;

    [ReadOnly] public NativeParallelMultiHashMap<int, float3>.ReadOnly spatialMap;

    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 position = localTransform.Position;
        float3 moveDirection = unitMover.targetPosition - position;
        moveDirection.y = 0;
        float reachedTargetDistanceSQ = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;

        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSQ)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }

        float3 targetPosition = unitMover.targetPosition;

        // Calculate separation force
        float3 separationForce = CalculateSeparationForce(position, targetPosition);

        moveDirection = math.normalize(moveDirection + separationForce);
        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * unitMover.rotationSpeed);

        float3 forward = math.normalize(new float3(moveDirection.x, 0, moveDirection.z));
        if (!math.any(math.isnan(forward)))
        {
            localTransform.Rotation = quaternion.LookRotationSafe(forward, math.up());
        }

        physicsVelocity.Linear = new float3(moveDirection.x, 0f, moveDirection.z) * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }

    private float3 CalculateSeparationForce(float3 position, float3 targetPosition)
    {
        float3 force = float3.zero;
        const float MIN_SEPARATION_DISTANCE = 0.25f;
        const float MAX_SEPARATION_FORCE = 3f;

        float radiusSq = UnitMoverSystem.SEPARATION_RADIUS * UnitMoverSystem.SEPARATION_RADIUS;

        float3 toTarget = targetPosition - position;
        float targetDistanceSq = math.lengthsq(toTarget);
        float distanceToTarget = math.sqrt(targetDistanceSq);

        int2 cell = GridUtils.WorldToGrid(position);

        // Dynamic separation modifier based on proximity to target
        float separationModifier = math.saturate(distanceToTarget / 2f);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int2 neighborCell = cell + new int2(x, y);
                int neighborHash = (int)GridUtils.HashCell(neighborCell);

                if (spatialMap.TryGetFirstValue(neighborHash, out var otherPosition, out var iterator))
                {
                    do
                    {
                        if (math.all(otherPosition == position)) continue;

                        float distSq = math.distancesq(position, otherPosition);

                        if (distSq > MIN_SEPARATION_DISTANCE * MIN_SEPARATION_DISTANCE && distSq < radiusSq)
                        {
                            float3 direction = position - otherPosition;
                            force += math.normalize(direction) / math.sqrt(distSq + 0.01f);
                        }
                    }
                    while (spatialMap.TryGetNextValue(out otherPosition, ref iterator));
                }
            }
        }

        // Apply separation modifier and clamp force
        force *= UnitMoverSystem.SEPARATION_FORCE_MULTIPLIER * separationModifier;
        float forceLengthSq = math.lengthsq(force);
        if (forceLengthSq > MAX_SEPARATION_FORCE * MAX_SEPARATION_FORCE)
        {
            force = math.normalize(force) * MAX_SEPARATION_FORCE;
        }

        return force;
    }


    public partial struct BuildSpatialMapJob : IJobEntity
    {
        public NativeParallelMultiHashMap<int, float3>.ParallelWriter spatialMap;

        public void Execute(in LocalTransform transform)
        {
            int2 cell = GridUtils.WorldToGrid(transform.Position);
            int hash = (int)math.hash(new int2(cell.x, cell.y));
            spatialMap.Add(hash, transform.Position);
        }
    }
}