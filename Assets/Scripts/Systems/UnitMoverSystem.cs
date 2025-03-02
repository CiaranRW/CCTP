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
    public const float SEPARATION_FORCE_MULTIPLIER = 2f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        NativeList<float3> entityPositions = new NativeList<float3>(Allocator.TempJob);

        foreach (var localTransform in SystemAPI.Query<LocalTransform>())
        {
            entityPositions.Add(localTransform.Position);
        }

        UnitMoverJob unitMoverJob = new UnitMoverJob
        {
            deltaTime = SystemAPI.Time.DeltaTime,
            entityPositions = entityPositions.AsReadOnly()
        };

        unitMoverJob.ScheduleParallel();
        state.Dependency = entityPositions.Dispose(state.Dependency);
    }
}


[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    [ReadOnly] public NativeArray<float3>.ReadOnly entityPositions;

    public void Execute(ref LocalTransform localTransform, in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        float3 separationForce = CalculateSeparationForce(localTransform.Position);

        float3 moveDirection = unitMover.targetPosition - localTransform.Position;
        float reachedTargetDistanceSQ = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSQ)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }

        moveDirection = math.normalize(moveDirection);
        moveDirection += separationForce;
        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation = math.slerp(localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * unitMover.rotationSpeed);

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }

    private float3 CalculateSeparationForce(float3 position)
    {
        float3 separationForce = float3.zero;
        float separationRadius = UnitMoverSystem.SEPARATION_RADIUS;

        foreach (float3 otherPosition in entityPositions)
        {
            if (math.distance(position, otherPosition) == 0f) continue;

            float3 direction = position - otherPosition;
            float distance = math.length(direction);

            if (distance < separationRadius)
            {
                separationForce += math.normalize(direction) / distance;
            }
        }

        return separationForce * UnitMoverSystem.SEPARATION_FORCE_MULTIPLIER;
    }
}