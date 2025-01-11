using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

partial struct EnemySpawningSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        foreach ((
            RefRO<LocalTransform> localTransform,
            RefRW<EnemySpawner > enemySpawner)
            in SystemAPI.Query<
                RefRO<LocalTransform>,
                RefRW<EnemySpawner>>())
        {
            enemySpawner.ValueRW.timer -= SystemAPI.Time.DeltaTime;
            if (enemySpawner.ValueRO.timer > 0f)
            {
                continue;
            }
            enemySpawner.ValueRW.timer = enemySpawner.ValueRO.timerMax;

            Entity BenemyEntity = state.EntityManager.Instantiate(entitiesReferences.BenemyPrefab);
            SystemAPI.SetComponent(BenemyEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));

            Entity RenemyEntity = state.EntityManager.Instantiate(entitiesReferences.RenemyPrefab);
            SystemAPI.SetComponent(RenemyEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
        }
    }
}


