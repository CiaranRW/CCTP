using Unity.Entities;
using UnityEngine;
using TMPro;

public class MonoToSystem : MonoBehaviour
{
    public float Timer = 100f;
    private bool start = true;
    [SerializeField] GameObject guideText;
    [SerializeField] GameObject UI;
    [SerializeField] TMP_Text spawnSpeed;

    private void Start()
    {
        UpdateCooldown();
    }

    private void Update()
    {
        if (start)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                start = false;
                UI.SetActive(true);
                guideText.SetActive(false);
                SetSpawnerEnabled(true);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                start = true;
                SetSpawnerEnabled(false);
            }
        }
        spawnSpeed.text = $"CoolDown: {Mathf.Floor(Timer * 1000f) / 1000f}";
        
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
        }
    }

    public void SetSpawnerEnabled(bool enabled)
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery query = entityManager.CreateEntityQuery(typeof(EnemySpawnerComponent));

        if (!query.IsEmpty)
        {
            Entity entity = query.GetSingletonEntity();
            var data = entityManager.GetComponentData<EnemySpawnerComponent>(entity);
            data.isSpawning = enabled;
            entityManager.SetComponentData(entity, data);
        }
    }

    public void DecreaseCooldown()
    {
        if (Timer  >= 0.2)
        {
            Timer -= 0.1f;
            UpdateCooldown();
        }
        else
        {
            Timer = 0.001f;
        }
    }
    public void IncreaseCooldown()
    {

        if (Timer != 0.001f && Timer <= 0.9)
        {
            Timer += 0.1f;
            UpdateCooldown();
        }
        else if (Timer == 0.001f)
        {
            Timer = 0.1f;
        }
    }
}