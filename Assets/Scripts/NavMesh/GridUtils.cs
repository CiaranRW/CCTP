using Unity.Mathematics;

public static class GridUtils
{
    public const float CELL_SIZE = 5f;

    public static int2 WorldToGrid(float3 position)
    {
        return new int2(
            (int)math.floor(position.x / CELL_SIZE),
            (int)math.floor(position.z / CELL_SIZE));
    }

    public static uint HashCell(int2 cell)
    {
        uint x = (uint)cell.x;
        uint y = (uint)cell.y;
        return math.hash(new uint2(x, y));
    }
}