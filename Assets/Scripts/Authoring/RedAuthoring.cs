using Unity.Entities;
using UnityEngine;

public class RedAuthoring : MonoBehaviour
{

    public class Baker : Baker<RedAuthoring>
    {
        public override void Bake(RedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Red());
        }
    }


}

public struct Red : IComponentData
{

}

