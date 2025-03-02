using Unity.Entities;
using UnityEngine;

public class FindTeamAuthoring : MonoBehaviour
{
    public float range;
    public Factions teamFaction;
    public float timerMax;
    public class Baker : Baker<FindTeamAuthoring>
    {
        public override void Bake(FindTeamAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new FindTeam
            {
                range = authoring.range,
                teamFaction = authoring.teamFaction,
                timerMax = authoring.timerMax,
            });
        }
    }
}

public struct FindTeam : IComponentData
{
    public float range;
    public Factions teamFaction;
    public float timer;
    public float timerMax;
}