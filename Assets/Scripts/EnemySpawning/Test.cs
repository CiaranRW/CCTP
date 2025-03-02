using Unity.Entities;
using UnityEngine;

public class Test : MonoBehaviour
{
    public float Timer = 100f;

    private void Start()
    {
        UpdateCooldown();
    }

    public void UpdateCooldown()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(EnemySpawnerComponent));

        if (!query.IsEmpty)
        {
            Entity entity = query.GetSingletonEntity();
            var componentData = entityManager.GetComponentData<EnemySpawnerComponent>(entity);

            componentData.spawnCooldown = Timer;
            entityManager.SetComponentData(entity, componentData);

            //Debug.Log("Updated spawnCooldown to " + Timer);
        }
    }

    public void DecreaseCooldown()
    {
        Timer -= 0.05f;
        UpdateCooldown();
    }
    public void IncreaseCooldown()
    {
        Timer += 0.05f;
        UpdateCooldown();
    }
}