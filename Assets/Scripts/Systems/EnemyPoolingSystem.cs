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

        foreach (var entity in entitiesToDisable)
        {
            DisableComponents(entity);
        }

        entitiesToDisable.Dispose();
    }

    private void DisableComponents(Entity entity)
    {
        if (EntityManager.HasComponent<UnitMover>(entity))
        {
            var mover = EntityManager.GetComponentData<UnitMover>(entity);
            mover.moveSpeed = 0f;
            EntityManager.SetComponentData(entity, mover);
        }

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
