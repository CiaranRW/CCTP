using UnityEngine;
using TMPro;
using Unity.Entities;

public class PoolSizeSelector : MonoBehaviour
{
    public TMP_InputField inputField;
    private Entity poolSizeEntity;
    private bool isValid = false;
    public GameObject loadingscreen;


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
            if (poolSize <= 3000 && poolSize >= 0)
            {
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                entityManager.SetComponentData(poolSizeEntity, new EnemyPoolSize { Value = poolSize });
                isValid = true;
            }
            else
            {
                isValid = false;
            }
        }
        else
        {
            Debug.Log("Invalid pool size.");
            isValid = false;
        }
    }

    public void UpdatePool()
    {
        if (isValid)
        {
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            var loadingReadyEntity = entityManager.CreateEntity(typeof(LoadingCompleteTag));
            loadingscreen.SetActive(false);
        }
        else
        {
            Debug.Log("please insert a valid number");
        }

    }
}
public struct EnemyPoolSize : IComponentData
{
    public int Value;
}

public struct LoadingCompleteTag : IComponentData { }