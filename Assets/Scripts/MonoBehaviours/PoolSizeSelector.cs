using UnityEngine;
using TMPro;
using Unity.Entities;

public class PoolSizeSelector : MonoBehaviour
{
    public TMP_InputField inputField;
    private Entity poolSizeEntity;


    void Start()
    {
        inputField.onValueChanged.AddListener(OnValueChanged);

        var world = World.DefaultGameObjectInjectionWorld;
        var entityManager = world.EntityManager;

        poolSizeEntity = entityManager.CreateEntity(typeof(EnemyPoolSize));
        entityManager.SetComponentData(poolSizeEntity, new EnemyPoolSize { Value = 500 });


    }

    void OnValueChanged(string value)
    {
        if (int.TryParse(value, out int poolSize))
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            entityManager.SetComponentData(poolSizeEntity, new EnemyPoolSize { Value = poolSize });
        }
        else
        {
            Debug.Log("Invalid pool size.");
        }
    }

    public void UpdatePool()
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var loadingReadyEntity = entityManager.CreateEntity(typeof(LoadingCompleteTag));
    }
}
public struct EnemyPoolSize : IComponentData
{
    public int Value;
}

public struct LoadingCompleteTag : IComponentData { }