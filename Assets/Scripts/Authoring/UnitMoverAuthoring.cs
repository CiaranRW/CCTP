using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class UnitMoverAuthoring : MonoBehaviour
{
    public float moveSpeed;
    public float rotationSpeed;


    public class Baker : Baker<UnitMoverAuthoring>
    {
        public override void Bake(UnitMoverAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new UnitMover{
                moveSpeed = authoring.moveSpeed,
                rotationSpeed = authoring.rotationSpeed,
            });
        }
    }
}


public struct UnitMover : IComponentData
{
    public float moveSpeed;
    public float rotationSpeed;
    public float3 targetPosition;
}

public struct GridPosition : IComponentData
{
    public int2 cell;
}

public struct GridHash
{
    public int3 Cell;
    public int Hash;

    public static int3 PositionToCell(float3 position, float cellSize)
    {
        return new int3(math.floor(position / cellSize));
    }

    public static int HashFromCell(int3 cell)
    {
        return cell.x * 73856093 ^ cell.y * 19349663 ^ cell.z * 83492791;
    }
}