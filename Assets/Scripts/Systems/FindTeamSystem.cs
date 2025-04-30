using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

partial struct FindTeamSysetm : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach (var (localTransform, team, findTeam, navAgent) in
            SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<Team>,
                RefRW<FindTeam>,
                RefRW<NavAgentComponent>>()
            .WithNone<DeadTag>())
        {
            findTeam.ValueRW.timer -= SystemAPI.Time.DeltaTime;

            if (findTeam.ValueRO.timer > 0f)
            {
                continue;
            }

            findTeam.ValueRW.timer = findTeam.ValueRO.timerMax;

            distanceHitList.Clear();
            CollisionFilter collisionfilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                GroupIndex = 0,
            };
            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTeam.ValueRO.range, ref distanceHitList, collisionfilter))
            {
                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);

                    if (targetUnit.faction == findTeam.ValueRO.teamFaction)
                    {
                        team.ValueRW.teamEntity = distanceHit.Entity;
                        break;
                    }
                }
            }
            
        }
    }
}
