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

        EntityCommandBuffer entityCommandBuffer =
            SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

        uint seed = (uint)(SystemAPI.Time.ElapsedTime * 1000) + 1;
        Random random = new Random(seed);

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

            if (enemySpawner.ValueRO.spawnedEntity == Entity.Null)
            {
                Entity entity = state.EntityManager.Instantiate(entitiesReferences.player);
                enemySpawner.ValueRW.spawnedEntity = entity;
                SystemAPI.SetComponent(entity, LocalTransform.FromPosition(0, 0, 0));
            }
            else
            {
                float3 randomPosition = new float3(
                random.NextFloat(-15f, 15f),
                0f,
                random.NextFloat(-10f, 10f)
);
                SystemAPI.SetComponent(enemySpawner.ValueRW.spawnedEntity, LocalTransform.FromPosition(randomPosition));
            }


/*            if (enemySpawner.ValueRO.swap == false)
            {
                Entity BenemyEntity = state.EntityManager.Instantiate(entitiesReferences.BenemyPrefab);
                SystemAPI.SetComponent(BenemyEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
                //SystemAPI.SetComponent(BenemyEntity, LocalTransform.FromPosition(-10, 0, 0));

                entityCommandBuffer.AddComponent(BenemyEntity, new RandomWalking
                {
                    originalPosiiton = localTransform.ValueRO.Position,
                    targetPosition = localTransform.ValueRO.Position,
                    distanceMin = enemySpawner.ValueRO.randomWalkingDistanceMin,
                    distanceMax = enemySpawner.ValueRO.randomWalkingDistanceMax,
                    random = new Random((uint)BenemyEntity.Index),
                });

                enemySpawner.ValueRW.swap = true;
            }*/
/*            else if (enemySpawner.ValueRO.swap == true)
            {

                Entity RenemyEntity = state.EntityManager.Instantiate(entitiesReferences.RenemyPrefab);
                SystemAPI.SetComponent(RenemyEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
                //SystemAPI.SetComponent(RenemyEntity, LocalTransform.FromPosition(10,0,0));


                entityCommandBuffer.AddComponent(RenemyEntity, new RandomWalking
                {
                    originalPosiiton = localTransform.ValueRO.Position,
                    targetPosition = localTransform.ValueRO.Position,
                    distanceMin = enemySpawner.ValueRO.randomWalkingDistanceMin,
                    distanceMax = enemySpawner.ValueRO.randomWalkingDistanceMax,
                    random = new Random((uint)RenemyEntity.Index),
                });

                
                enemySpawner.ValueRW.swap = false;
            }*/


        }
    }
}


