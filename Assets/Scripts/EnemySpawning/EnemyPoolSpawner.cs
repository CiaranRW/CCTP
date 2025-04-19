using Unity.Entities;


[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class EnemyPoolBootstrapSystem : SystemBase
{
    protected override void OnUpdate()
    {
        if (!SystemAPI.HasSingleton<LoadingCompleteTag>()) // <- waits for signal
            return;

        if (SystemAPI.HasSingleton<EnemyPoolInitializedTag>())
            return;

        if (!SystemAPI.TryGetSingletonEntity<EnemySpawnerComponent>(out var spawnerEntity))
            return;

        var enemyDataContainer = EntityManager.GetComponentObject<EnemyDataContainer>(spawnerEntity);

        if (enemyDataContainer.enemies.Count == 0)
            return;


        if (!SystemAPI.HasSingleton<EnemyPoolSize>())
            return;


        var prefab = enemyDataContainer.enemies[0].prefab;

        var poolSize = SystemAPI.GetSingleton<EnemyPoolSize>().Value;

        for (int i = 0; i < poolSize; i++)
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