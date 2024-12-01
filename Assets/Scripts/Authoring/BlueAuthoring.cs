using Unity.Entities;
using UnityEngine;

public class BlueAuthoring : MonoBehaviour
{
    public class Baker : Baker<BlueAuthoring> 
    {
        public override void Bake(BlueAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Blue());
        }
    }

    
}

public struct Blue : IComponentData
{

}
