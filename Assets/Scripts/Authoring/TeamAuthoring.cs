using Unity.Entities;
using UnityEngine;

public class TeamAuthoring : MonoBehaviour
{
    public class Baker : Baker<TeamAuthoring> 
    {
        public override void Bake(TeamAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new Team());
        }
    }

}

public struct Team : IComponentData
{
    public Entity teamEntity;
}