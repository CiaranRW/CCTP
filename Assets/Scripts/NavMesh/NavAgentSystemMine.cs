using UnityEngine;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Experimental.AI;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Physics;
using System.Net;

/*[BurstCompile]
public partial struct NavAgentSystemMine : ISystem
{
    [BurstCompile]

    private void OnUpdate(ref SystemState state)
    {

        foreach (var (navAgent, unitMover, transform, entity) in SystemAPI.Query<RefRW<NavAgentComponent>, RefRW<UnitMover>, RefRW<LocalTransform>>().WithEntityAccess())
        {
            if (navAgent.ValueRO.targetEntity == Entity.Null)
            {
                continue;
            }

            {
                DynamicBuffer<WaypointBuffer> waypointBuffer = state.EntityManager.GetBuffer<WaypointBuffer>(entity);


                if (navAgent.ValueRO.nextPathCalculateTime < SystemAPI.Time.ElapsedTime)
                {
                    if (0 < SystemAPI.Time.ElapsedTime)
                    {
                        navAgent.ValueRW.nextPathCalculateTime += 1;
                        navAgent.ValueRW.pathCalculated = false;
                        CalculatePath(navAgent, unitMover, transform, waypointBuffer, ref state);
                    }
                }
                else
                {
                    if (waypointBuffer.Length != 0)
                    {
                        Move(navAgent, unitMover, transform, waypointBuffer, ref state);
                    }
                }
            }
        }
    }

    [BurstCompile]
    private void Move(RefRW<NavAgentComponent> navAgent, RefRW<UnitMover> unitMover, RefRW<LocalTransform> transform, DynamicBuffer<WaypointBuffer> waypointBuffer,
        ref SystemState state)
    {
        if (math.distance(transform.ValueRO.Position, waypointBuffer[navAgent.ValueRO.currentWaypoint].wayPoint) < 0.4f)
        {
            if (navAgent.ValueRO.currentWaypoint + 1 < waypointBuffer.Length)
            {
                navAgent.ValueRW.currentWaypoint += 1;
            }
        }

        float3 direction = waypointBuffer[navAgent.ValueRO.currentWaypoint].wayPoint - transform.ValueRO.Position;
        float angle = math.degrees(math.atan2(direction.z, direction.x));

        transform.ValueRW.Rotation = math.slerp(
            transform.ValueRW.Rotation,
            quaternion.Euler(new float3(0, angle, 0)),
            SystemAPI.Time.DeltaTime);

        transform.ValueRW.Position += math.normalize(direction) * SystemAPI.Time.DeltaTime * navAgent.ValueRO.moveSpeed;
        //unitMover.ValueRW.targetPosition = waypointBuffer[navAgent.ValueRO.currentWaypoint].wayPoint - transform.ValueRO.Position;
        //unitMover.ValueRW.targetPosition += math.normalize(direction) * SystemAPI.Time.DeltaTime * navAgent.ValueRO.moveSpeed;
    }

    [BurstCompile]
    private void CalculatePath(RefRW<NavAgentComponent> navAgent, RefRW<UnitMover> unitMover, RefRW<LocalTransform> transform, DynamicBuffer<WaypointBuffer> waypointBuffer,
        ref SystemState state)
    {
        NavMeshQuery query = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, 1000);

        float3 fromPosition = transform.ValueRO.Position;
        float3 toPosition = state.EntityManager.GetComponentData<LocalTransform>(navAgent.ValueRO.targetEntity).Position;
        float3 extents = new float3(1, 1, 1);

        NavMeshLocation fromLocation = query.MapLocation(fromPosition, extents, 0);
        NavMeshLocation toLocation = query.MapLocation(toPosition, extents, 0);

        PathQueryStatus status;
        PathQueryStatus returningStatus;
        int maxPathSize = 100;

        if (query.IsValid(fromLocation) && query.IsValid(toLocation))
        {
            status = query.BeginFindPath(fromLocation, toLocation);
            if (status == PathQueryStatus.InProgress)
            {
                status = query.UpdateFindPath(100, out int iterationsPerformed);
                if (status == PathQueryStatus.Success)
                {
                    status = query.EndFindPath(out int pathSize);

                    NativeArray<NavMeshLocation> result = new NativeArray<NavMeshLocation>(pathSize + 1, Allocator.Temp);
                    NativeArray<StraightPathFlags> straightPathFlag = new NativeArray<StraightPathFlags>(maxPathSize, Allocator.Temp);
                    NativeArray<float> vertexSide = new NativeArray<float>(maxPathSize, Allocator.Temp);
                    NativeArray<PolygonId> polygonIds = new NativeArray<PolygonId>(pathSize + 1, Allocator.Temp);
                    int straightPathCount = 0;

                    query.GetPathResult(polygonIds);

                    returningStatus = PathUtils.FindStraightPath
                        (
                        query,
                        fromPosition,
                        toPosition,
                        polygonIds,
                        pathSize,
                        ref result,
                        ref straightPathFlag,
                        ref vertexSide,
                        ref straightPathCount,
                        maxPathSize
                        );

                    if (returningStatus == PathQueryStatus.Success)
                    {
                        waypointBuffer.Clear();

                        foreach (NavMeshLocation location in result)
                        {
                            if (location.position != Vector3.zero)
                            {
                                waypointBuffer.Add(new WaypointBuffer { wayPoint = location.position });
                            }
                        }

                        navAgent.ValueRW.currentWaypoint = 0;
                        navAgent.ValueRW.pathCalculated = true;
                    }
                    straightPathFlag.Dispose();
                    polygonIds.Dispose();
                    vertexSide.Dispose();
                }
            }
        }
        query.Dispose();
    }
}

public struct WaypointBuffer : IBufferElementData
{
    public float3 wayPoint;
}*/