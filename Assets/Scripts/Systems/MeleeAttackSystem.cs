using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct MeleeAttackSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        float huddleDistanceSq = 0.2f * 0.2f;

        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<MeleeAttack> meleeAttack,
            RefRO<Team> team,
            RefRW<UnitMover> unitMover,
            Entity selfEntity)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<MeleeAttack>,
                RefRO<Team>,
                RefRW<UnitMover>>()
                .WithEntityAccess())
        {
            if (meleeAttack.ValueRO.timer > 0)
            {
                meleeAttack.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                continue;
            }

            var targetFound = false;

            foreach ((
                RefRO<LocalTransform> otherTransform,
                RefRO<Team> otherTeam,
                RefRW<Health> otherHealth,
                Entity otherEntity)
                in SystemAPI.Query<
                    RefRO<LocalTransform>,
                    RefRO<Team>,
                    RefRW<Health>>()
                    .WithNone<DeadTag>()
                    .WithEntityAccess())
            {
                if (selfEntity == otherEntity) continue; // Prevent self-attack
                if (team.ValueRO.teamEntity != otherTeam.ValueRO.teamEntity) continue; // Only attack same team

                float distSq = math.distancesq(localTransform.ValueRO.Position, otherTransform.ValueRO.Position);
                if (distSq <= huddleDistanceSq)
                {
                    // Attack!
                    otherHealth.ValueRW.healthAmount -= meleeAttack.ValueRO.damageAmount;
                    meleeAttack.ValueRW.timer = meleeAttack.ValueRO.timerMax;
                    targetFound = true;
                    break;
                }
            }

            if (!targetFound)
            {
                // Optional: move to random nearby position or stay idle
                unitMover.ValueRW.targetPosition = localTransform.ValueRO.Position;
            }
        }
    }
}