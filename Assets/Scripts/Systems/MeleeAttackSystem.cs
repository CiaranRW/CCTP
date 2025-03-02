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
            RefRO<Team> team,
            RefRW<UnitMover> unitMover)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<MeleeAttack>,
                RefRO<Team>,
                RefRW<UnitMover>>())
        {
            if (team.ValueRO.teamEntity == Entity.Null) 
            {
                continue;
            }

            LocalTransform targetLocalTransform = SystemAPI.GetComponent<LocalTransform>(team.ValueRO.teamEntity);
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

                RefRW<Health> targetHealth = SystemAPI.GetComponentRW<Health>(team.ValueRO.teamEntity);
                targetHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                //targetHealth.ValueRW.OnHealthChanged = true
                //int damageAmount = 1;
                //targetHealth.ValueRW.healthAmount -= damageAmount;
            }
        }
    }


}
