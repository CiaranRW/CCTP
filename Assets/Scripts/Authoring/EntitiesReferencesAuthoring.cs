using Unity.Entities;
using UnityEngine;

public class EntitiesReferencesAuthoring : MonoBehaviour
{

    public GameObject BenemyPrefab;
    public GameObject RenemyPrefab;

    public class Baker : Baker<EntitiesReferencesAuthoring> 
    {
        public override void Bake(EntitiesReferencesAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EntitiesReferences
            {
                BenemyPrefab = GetEntity(authoring.BenemyPrefab, TransformUsageFlags.Dynamic),
                RenemyPrefab = GetEntity(authoring.RenemyPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }

}


public struct EntitiesReferences : IComponentData
{
    public Entity BenemyPrefab;
    public Entity RenemyPrefab;
}