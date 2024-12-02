using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

partial struct MeleeAttackSystem : ISystem
{ 
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach ((
            RefRO <LocalTransform > LocalTransform,
            RefRW<MeleeAttack> meleeAttack,
            RefRO<Target> target,
            RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<MeleeAttack>,
                RefRO<Target>,
                RefRW<UnitMover>>())
        {
            if (target.ValueRO.targetEntity == Entity.Null) 
            {
                continue;
            }

            //meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            //if (meleeAttack.ValueRO.timer > 0 ) 
            //{
            //    continue;
            //}
            //meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;
            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(target.ValueRO.targetEntity);
            float meleeAttackDistanceSq = 2f;
            if (math.distancesq(LocalTransform.ValueRO.Position, targetLocalTransform.Position) > meleeAttackDistanceSq)
            {
                //too far
                unitMover.ValueRW.targetPosition = targetLocalTransform.Position;
            }
            else
            {
                //too close
                unitMover.ValueRW.targetPosition = LocalTransform.ValueRO.Position;

                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                if (meleeAttack.ValueRO.timer > 0)
                {
                    continue;
                }
                meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;
            }


        }
    }


}
