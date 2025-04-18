using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

public partial class EnemyPoolingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // First, collect entities to process in a list
        var entitiesToDisable = new NativeList<Entity>(Allocator.Temp);

        foreach (var (health, transform, entity) in
            SystemAPI.Query<RefRO<Health>, RefRO<LocalTransform>>()
                     .WithAll<DeadTag>()
                     .WithEntityAccess())
        {
            entitiesToDisable.Add(entity);
        }

        // Now safely modify the entities outside the query loop
        foreach (var entity in entitiesToDisable)
        {
            DisableComponents(entity);
        }

        entitiesToDisable.Dispose();
    }

    // This method will be used to disable components on pooled entities
    private void DisableComponents(Entity entity)
    {
        // Disable movement
        if (EntityManager.HasComponent<UnitMover>(entity))
        {
            var mover = EntityManager.GetComponentData<UnitMover>(entity);
            mover.moveSpeed = 0f; // Actually stop it
            EntityManager.SetComponentData(entity, mover);
        }

        // Remove team affiliation
        if (EntityManager.HasComponent<Blue>(entity))
        {
            EntityManager.RemoveComponent<Blue>(entity);
        }

        EntityManager.SetComponentData(entity, new LocalTransform
        {
            Position = new float3(0, -100, 0),
            Rotation = quaternion.identity,
            Scale = 1
        });

        EntityManager.SetComponentData(entity, new PhysicsCollider { Value = default });
    }
}
