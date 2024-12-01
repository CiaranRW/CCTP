using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct FindTargetSysetm : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        PhysicsWorldSingleton physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
        CollisionWorld collisionWorld = physicsWorldSingleton.CollisionWorld;
        NativeList<DistanceHit> distanceHitList = new NativeList<DistanceHit>(Allocator.Temp);

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRO<FindTarget> findTarget,
            RefRW<Target> target)

            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRO<FindTarget>,
                RefRW<Target>>())
        {
            distanceHitList.Clear();
            CollisionFilter collisionfilter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << GameAssets.UNITS_LAYER,
                GroupIndex = 0,
            };
            //Debug.Log("-");
            if (collisionWorld.OverlapSphere(localTransform.ValueRO.Position, findTarget.ValueRO.range, ref distanceHitList, collisionfilter))
            {
                foreach (DistanceHit distanceHit in distanceHitList)
                {
                    Unit targetUnit = SystemAPI.GetComponent<Unit>(distanceHit.Entity);
                    if (targetUnit.faction == findTarget.ValueRO.targetFaction)
                    {
                        target.ValueRW.targetEntity = distanceHit.Entity;
                        //Debug.Log(distanceHit.Entity);
                        break;
                    }
                }
            }
            
        }
    }
}
