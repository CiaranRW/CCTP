using Unity.Entities;
using UnityEngine;

public class EnemySpawningAuthoring : MonoBehaviour
{

    public float timerMax;
    public float randomWalkingDistanceMin;
    public float randomWalkingDistanceMax;
    public bool swap = false;
    public bool spawn = false;
    public Entity spawnedEntity;


    public class Baker : Baker<EnemySpawningAuthoring>
    {
        public override void Bake(EnemySpawningAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new EnemySpawner
            {
                timerMax = authoring.timerMax,
                randomWalkingDistanceMin = authoring.randomWalkingDistanceMin,
                randomWalkingDistanceMax = authoring.randomWalkingDistanceMax,
                swap = authoring.swap,
                spawn = authoring.spawn,
                spawnedEntity = authoring.spawnedEntity
            });
        }
    }

}

public struct EnemySpawner : IComponentData
{
    public float timer;
    public float timerMax;
    public float randomWalkingDistanceMin;
    public float randomWalkingDistanceMax;
    public bool swap;
    public bool spawn;
    public Entity spawnedEntity;
}
