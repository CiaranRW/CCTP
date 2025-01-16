using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using static UnityEngine.EventSystems.EventTrigger;

public class NavAgentAuthoring : MonoBehaviour
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float moveSpeed;

    private class AuthoringBaker : Baker<NavAgentAuthoring> 
    {
        public override void Bake(NavAgentAuthoring authoring)
        {
            Entity authoringEntity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent(authoringEntity, new NavAgentComponent
            {
                targetEntity = GetEntity(authoring.targetTransform, TransformUsageFlags.Dynamic),
                moveSpeed = authoring.moveSpeed,
            });
            AddBuffer<WaypointBuffer>(authoringEntity);
        }
    }

}

public struct NavAgentComponent : IComponentData
{
    public Entity targetEntity;
    public bool pathCalculated;
    public int currentWaypoint;
    public float moveSpeed;
    public float nextPathCalculateTime;

}
