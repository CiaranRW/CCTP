using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

partial struct UnitMoverSystem : ISystem
{
    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;
    public const float SEPARATION_RADIUS = 3f;
    public const float SEPARATION_FORCE_MULTIPLIER = 2f; // Your constants for movement

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var spatialMap = new NativeParallelMultiHashMap<int, float3>(1024, Allocator.TempJob);

        foreach (var localTransform in SystemAPI.Query<LocalTransform>())
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
        float reachedTargetDistanceSQ = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;

        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSQ)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }

        float3 separationForce = CalculateSeparationForce(position);

        moveDirection = math.normalize(moveDirection + separationForce);
        localTransform.Rotation = math.slerp(
            localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * unitMover.rotationSpeed);

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }

    private float3 CalculateSeparationForce(float3 position)
    {
        float3 force = float3.zero;
        float radiusSq = UnitMoverSystem.SEPARATION_RADIUS * UnitMoverSystem.SEPARATION_RADIUS;

        int2 cell = GridUtils.WorldToGrid(position);
        int hash = (int)GridUtils.HashCell(cell);

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                int2 neighborCell = cell + new int2(x, y);
                int neighborHash = (int)GridUtils.HashCell(neighborCell);

                if (spatialMap.TryGetFirstValue((int)neighborHash, out var otherPosition, out var iterator))
                {
                    do
                    {
                        if (math.all(otherPosition == position)) continue;

                        float distSq = math.distancesq(position, otherPosition);
                        if (distSq < radiusSq)
                        {
                            // Apply separation force
                            float3 direction = position - otherPosition;
                            force += math.normalize(direction) / math.sqrt(distSq);
                        }
                    }
                    while (spatialMap.TryGetNextValue(out otherPosition, ref iterator));
                }
            }
        }

        return force * UnitMoverSystem.SEPARATION_FORCE_MULTIPLIER;
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
