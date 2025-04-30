using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

partial struct FindTargetSysetm : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach (var (localTransform, findTarget, target, findTeam, unitMover, navAgent, entity) in
             SystemAPI.Query<
                 RefRO<LocalTransform>,
                 RefRW<FindTarget>,
                 RefRW<Target>,
                 RefRW<FindTeam>,
                 RefRW<UnitMover>,
                 RefRW<NavAgentComponent>>()
                      .WithEntityAccess()
                      .WithNone<DeadTag>())
        {
            findTarget.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (findTarget.ValueRO.timer > 0f)
            {
                continue;
            }

            findTarget.ValueRW.timer = findTarget.ValueRO.timerMax;

            distanceHitList.Clear();
            CollisionFilter collisionfilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                GroupIndex = 0,
            };
            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref distanceHitList, collisionfilter))
            {
                Entity closestTarget = Entity.Null;
                float closestDistance = float.MaxValue;

                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);

                    if (targetUnit.faction == findTarget.ValueRO.targetFaction)
                    {
                        LocalTransform targetTransform = SystemAPI.GetComponent<LocalTransform>(distanceHit.Entity);
                        float distance = math.distance(localTransform.ValueRO.Position, targetTransform.Position);

                        if (distance < closestDistance)
                        {
                            closestDistance = distance;
                            closestTarget = distanceHit.Entity;
                        }
                    }
                }
                if (closestTarget != Entity.Null)
                {
                    target.ValueRW.targetEntity = closestTarget;
                    navAgent.ValueRW.targetEntity = closestTarget;

                    LocalTransform targetTransform = SystemAPI.GetComponent<LocalTransform>(closestTarget);
                    unitMover.ValueRW.targetPosition = targetTransform.Position;
                }
            }
        }
    }
}