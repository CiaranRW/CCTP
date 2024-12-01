using UnityEngine;
using Unity.Entities;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using UnityEngine.UIElements;

public partial class EnemySpawnerSystem : SystemBase
{
    private EnemySpawnerComponent enemySpawnerComponent;
    private EnemyDataContainer EnemyDataContainerComponent;
    private Entity enemySpawnerEntity;
    private float nextSpawnTime;
    private Random random;

    protected override void OnCreate()
    {
        random = Random.CreateFromIndex((uint)enemySpawnerComponent.GetHashCode()); //random number
    }

    protected override void OnUpdate()
    {
        if (!SystemAPI.TryGetSingletonEntity<EnemySpawnerComponent>(out enemySpawnerEntity))
        {
            return;
        }

        enemySpawnerComponent = EntityManager.GetComponentData<EnemySpawnerComponent>(enemySpawnerEntity);
        EnemyDataContainerComponent = EntityManager.GetComponentObject<EnemyDataContainer>(enemySpawnerEntity);

        if (SystemAPI.Time.ElapsedTime > nextSpawnTime) 
        {
            SpawnEnemy(); //spawn
        }
    }

    private void SpawnEnemy()
    {
        int level = 2;
        List<EnemyData> availableEnemies = new List<EnemyData>();

        foreach (EnemyData enemyData in EnemyDataContainerComponent.enemies)
        {
            if (enemyData.level <= level) 
            {
                availableEnemies.Add(enemyData); //under level 2 then spawn
            }
        }

        int index = random.NextInt(availableEnemies.Count);

        Entity newEnemy = EntityManager.Instantiate(availableEnemies[index].prefab); //finds random prefab then spawns in enemy
        EntityManager.SetComponentData(newEnemy, new LocalTransform
        {
            Position = getPositionOutsideofCameraRange(),
            Rotation = quaternion.identity,
            Scale = 1
        });

        EntityManager.AddComponentData(newEnemy,new EnemyComponent { currentHealth = availableEnemies[index].health }); //giving the enemy health
        //EntityManager.AddComponentData(newEnemy, new EnemyComponent { currentTeam = availableEnemies[index].team });

        nextSpawnTime = (float)SystemAPI.Time.ElapsedTime + enemySpawnerComponent.spawnCooldown;
    }
    /*private float3 getPositionOutsideofCameraRange()
    {
        float3 position = new float3(random.NextFloat2(-enemySpawnerComponent.cameraSize * 2, enemySpawnerComponent.cameraSize * 2), 0);

        while (position.x < enemySpawnerComponent.cameraSize.x && position.x > -enemySpawnerComponent.cameraSize.x
            && position.y < enemySpawnerComponent.cameraSize.y && position.y > -enemySpawnerComponent.cameraSize.y)
        {
            position = new float3(random.NextFloat2(-enemySpawnerComponent.cameraSize * 2, enemySpawnerComponent.cameraSize * 2), 0);
        }

        position += new float3(Camera.main.transform.position.x, 0, Camera.main.transform.position.y);
        //position.y = 0;

        return position;
    }*/

    private float3 getPositionOutsideofCameraRange()
    {
        float3 position = new float3(random.NextFloat3(-10, 10));

        position.y = 0;

        return position;
    }
}


