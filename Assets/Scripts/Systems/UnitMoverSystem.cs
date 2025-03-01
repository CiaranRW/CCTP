using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics;
using System;

partial struct UnitMoverSystem : ISystem
{

    public const float REACHED_TARGET_POSITION_DISTANCE_SQ = 2f;

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        UnitMoverJob unitMoverJob = new UnitMoverJob 
        {
            deltaTime = SystemAPI.Time.DeltaTime,
        };
        unitMoverJob.ScheduleParallel();
        /*
        foreach ((
            RefRW<LocalTransform> localTransform,
            RefRO<UnitMover> unitMover,
            RefRW<PhysicsVelocity> physicsVelocity)
            in SystemAPI.Query<
                RefRW<LocalTransform>,
                RefRO<UnitMover>,
                RefRW<PhysicsVelocity>>())
        {

            float3 moveDirection = unitMover.ValueRO.targetPosition - localTransform.ValueRO.Position;
            moveDirection = math.normalize(moveDirection);

            localTransform.ValueRW.Rotation =
                math.slerp(localTransform.ValueRO.Rotation,
                    quaternion.LookRotation(moveDirection, math.up()),
                    SystemAPI.Time.DeltaTime * unitMover.ValueRO.rotationSpeed);
            
            physicsVelocity.ValueRW.Linear = moveDirection * unitMover.ValueRO.moveSpeed;
            physicsVelocity.ValueRW.Angular = float3.zero;
        }
        */
    }
}

[BurstCompile]
public partial struct UnitMoverJob : IJobEntity
{
    public float deltaTime;
    public void Execute(ref LocalTransform localTransform,in UnitMover unitMover, ref PhysicsVelocity physicsVelocity)
    {
        //var neighbours = GetNeighbours(ref localTransform);
/*
        if (neighbours.Length > 0)
        {
            CalculateSeperationForce(ref localTransform, neighbours);
            
        }*/

        float3 moveDirection = unitMover.targetPosition - localTransform.Position;

        float reachedTargetDistanceSQ = UnitMoverSystem.REACHED_TARGET_POSITION_DISTANCE_SQ;
        if (math.lengthsq(moveDirection) <= reachedTargetDistanceSQ)
        {
            physicsVelocity.Linear = float3.zero;
            physicsVelocity.Angular = float3.zero;
            return;
        }
        moveDirection = math.normalize(moveDirection);

        localTransform.Rotation =
            math.slerp(localTransform.Rotation,
            quaternion.LookRotation(moveDirection, math.up()),
            deltaTime * unitMover.rotationSpeed);

        physicsVelocity.Linear = moveDirection * unitMover.moveSpeed;
        physicsVelocity.Angular = float3.zero;
    }

/*    private void CalculateSeperationForce(ref LocalTransform localTransform, UnityEngine.Collider[] neighbours)
    {
        Vector3 m_seperationforce = Vector3.zero;
        foreach (var neighbour in neighbours) 
        {
            Vector3 test = localTransform.Position;
            var dir = neighbour.transform.position - test;
            var distance = dir.magnitude;
            var away = -dir.normalized;

            if (distance > 0)
            {
                m_seperationforce += away / distance;
            }
        }
    }*/

/*    private UnityEngine.Collider[] GetNeighbours(ref LocalTransform localTransform)
    {
        var enemyMask = LayerMask.GetMask("Units");
        return Physics.OverlapSphere(localTransform.Position, 1f, enemyMask);
    }*/
}
