using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

public struct EnemySpawnerComponent : IComponentData
{
    public float spawnCooldown;
    //public float2 cameraSize;
}
