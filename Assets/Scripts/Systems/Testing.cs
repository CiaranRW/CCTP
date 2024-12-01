using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

/*partial struct Testing : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        int unitCount = 0;

        foreach (RefRW<Red> red
            in SystemAPI.Query<
                RefRW<Red>>())
        {
            unitCount++;
        }

        Debug.Log("unitCount: "+ unitCount);

    }
}*/
