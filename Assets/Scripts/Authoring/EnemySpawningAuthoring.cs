using Unity.Entities;
using UnityEngine;

public class EnemySpawningAuthoring : MonoBehaviour
{

    public float timerMax;

    public class Baker : Baker<EnemySpawningAuthoring>
    {
        public override void Bake(EnemySpawningAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemySpawner
            {
                timerMax = authoring.timerMax,
            });
        }
    }

}

public struct EnemySpawner : IComponentData
{
    public float timer;
    public float timerMax;
}
