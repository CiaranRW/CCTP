using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Burst;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class EnemyPoolBootstrapSystem : SystemBase
{
    private bool hasSpawned = false;
    protected override void OnUpdate()
    {
        // Only run if the init tag doesn't exist
        if (SystemAPI.HasSingleton<EnemyPoolInitializedTag>())
            return;

        if (!SystemAPI.TryGetSingletonEntity<EnemySpawnerComponent>(out var spawnerEntity))
            return;

        var enemyDataContainer = EntityManager.GetComponentObject<EnemyDataContainer>(spawnerEntity);

        if (enemyDataContainer.enemies.Count == 0)
            return;

        var prefab = enemyDataContainer.enemies[0].prefab;


        for (int i = 0; i < 1000; i++)
        {
            Entity entity = EntityManager.Instantiate(prefab);

            EntityManager.AddComponent<DeadTag>(entity);       
        }
        var initEntity = EntityManager.CreateEntity();
        EntityManager.AddComponent<EnemyPoolInitializedTag>(initEntity);
    }
}
public struct EnemyPoolInitializedTag : IComponentData 
{
    
}