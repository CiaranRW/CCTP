using Unity.Entities;
using UnityEngine;

public class HumanSpawnerAuthoring : MonoBehaviour
{
    public float minTime = 45f;
    public float maxTime = 120f;
    public class Baker : Baker<HumanSpawnerAuthoring>
    {
        public override void Bake(HumanSpawnerAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);

            MoveTimer moveTimer = new MoveTimer
            {
                timer = Random.Range(authoring.minTime, authoring.maxTime), 
                minTime = authoring.minTime,
                maxTime = authoring.maxTime
            };

            AddComponent(entity, moveTimer);
        }
    }
}

public struct MoveTimer : IComponentData
{
    public float timer;
    public float minTime;
    public float maxTime;
}