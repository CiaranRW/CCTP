using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

partial class EnemyPoolingSystem : SystemBase
{
    private EntityQuery deadEntitiesQuery;

    protected override void OnCreate()
    {
        // Query to fetch all entities with DeadTag, Health and LocalTransform
        deadEntitiesQuery = GetEntityQuery(
            ComponentType.ReadOnly<DeadTag>(),
            ComponentType.ReadWrite<Health>(),
            ComponentType.ReadWrite<LocalTransform>()
        );
    }

    protected override void OnUpdate()
    {
        // Directly access entities marked as dead
        Entities
            .WithStoreEntityQueryInField(ref deadEntitiesQuery)
            .ForEach((Entity entity, ref Health health, ref LocalTransform transform) =>
            {
                if (health.healthAmount <= 0)
                {
                    // Recycle the entity by resetting health and transform
                    health.healthAmount = 100;
                    transform.Position = new float3(0, 0, 0); // Reset position
                    transform.Rotation = quaternion.identity; // Reset rotation
                    transform.Scale = 1; // Reset scale

                    // Leave the DeadTag so the respawn system can reuse this entity
                }
            }).Run(); // Execute immediately on the main thread
    }
}