using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(SimulationSystemGroup))]
public partial struct GridAssignmentSystem : ISystem
{
    public void OnUpdate(ref SystemState state)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach (var (transform, entity) in SystemAPI.Query<RefRO<LocalTransform>>().WithEntityAccess())
        {
            int2 cell = GridUtils.WorldToGrid(transform.ValueRO.Position);

            if (SystemAPI.HasComponent<GridPosition>(entity))
            {
                state.EntityManager.SetComponentData(entity, new GridPosition { cell = cell });
            }
            else
            {
                ecb.AddComponent(entity, new GridPosition { cell = cell });
            }
        }

        ecb.Playback(state.EntityManager);
        ecb.Dispose();
    }
}