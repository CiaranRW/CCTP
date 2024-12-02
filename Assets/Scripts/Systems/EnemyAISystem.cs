using Unity.Entities;
using UnityEngine;
using Unity.Transforms;
using Unity.Mathematics;

/*public partial struct EnemyAISystem : ISystem
{
    private EntityManager entityManager;
    private Entity enemyEntity;
    //private GameObject[] reds;
    //private GameObject[] blues;

    private void OnUpdate(ref SystemState state)
    {
        entityManager = state.EntityManager;
        //playerEntity = SystemAPI.GetSingletonEntity<EnemyComponent>();
        //enemyEntity = SystemAPI.GetSingletonEntity<EnemyComponent>();

        //reds = GameObject.FindGameObjectsWithTag("Red");
        //blues = GameObject.FindGameObjectsWithTag("Blue");

        foreach (var (enemyComponent, transformComponent) in SystemAPI.Query<EnemyComponent, RefRW<LocalTransform>>())
        {
            float3 direction = entityManager.GetComponentData<LocalTransform>(enemyEntity).Position - transformComponent.ValueRO.Position;
            //float3 direction = nearest.transform.position;
            float angle = math.atan2(direction.y, direction.x) + math.radians(90);
            transformComponent.ValueRW.Rotation = quaternion.Euler(new float3(0, 0, angle));

            transformComponent.ValueRW.Position += math.normalize(direction) * SystemAPI.Time.DeltaTime;

        }
    }
}*/
