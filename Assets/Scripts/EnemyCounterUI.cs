using UnityEngine;
using Unity.Entities;
using UnityEngine.UI;
using TMPro;

public class EnemyCounterUI : MonoBehaviour
{
    public TMP_Text enemyCountText;

    EntityManager entitiyManager;
    EntityQuery entityQuery;

    private void Start()
    {
        entitiyManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        entityQuery = entitiyManager.CreateEntityQuery(typeof(Blue));
    }

    // Update is called once per frame
    void Update()
    {
        int count = entityQuery.CalculateEntityCount();
        enemyCountText.text = $"Enemies: {count}";
        //Debug.Log(count);
    }
}
