using System;
using System.Collections.Generic;

using OpenTK;

using OpenTK_PathTracer.GameObjects;

namespace OpenTK_PathTracer
{
    class Grid
    {
        public interface IGridCompatible
        {
            public abstract Vector3 Min { get; }
            public abstract Vector3 Max { get; }
            public abstract bool IntersectsAABB(AABB aabb);
        }

        public class Cell
        {
            public static readonly int GPUInstanceSize = Vector4.SizeInBytes * 2;

            public AABB AABB;
            public int Start = -1;
            public int End = -1;
            public Cell(AABB aabb)
            {
                AABB = aabb;
            }

            readonly Vector4[] gpuData = new Vector4[2];
            public Vector4[] GetGPUFriendlyData()
            {
                gpuData[0].Xyz = AABB.Min;
                gpuData[0].W = this.Start;

                gpuData[1].Xyz = AABB.Max;
                gpuData[1].W = this.End;

                return gpuData;
            }
        }


        public int Width, Height, Depth;
        public Grid(int width, int height, int depth)
        {
            Width = width; Height = height; Depth = depth;
        }

        public List<Cell> Cells = new List<Cell>();
        public Vector3 CellSize { get; private set; } = new Vector3(-1);
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public int[] Indecis { get; private set; }

        public AABB RootAABB { get; private set; }
        public void Update(List<GameObject> gameObjects)
        {
            Cells.Clear();

            Min = new Vector3(float.MaxValue);
            Max = new Vector3(float.MinValue);
            for (int i = 0; i < gameObjects.Count; i++)
            {
                Min = Vec3Min(Min, gameObjects[i].Min);
                Max = Vec3Max(Max, gameObjects[i].Max);
            }
            RootAABB = new AABB((Min + Max) * 0.5f, Max - Min);

            CellSize = Vector3.Divide(Max - Min, new Vector3(Width, Height, Depth));
            List<int> indecis = new List<int>(gameObjects.Count);
            for (float z = Min.Z + CellSize.Z / 2; z < Max.Z; z += CellSize.Z)
            {
                for (float y = Min.Y + CellSize.Y / 2; y < Max.Y; y += CellSize.Y)
                {
                    for (float x = Min.X + CellSize.X / 2; x < Max.X; x += CellSize.X)
                    {
                        Cell cell = new Cell(new AABB(new Vector3(x, y, z), CellSize));
                        cell.Start = indecis.Count;
                        for (int i = 0; i < gameObjects.Count; i++)
                            if (gameObjects[i].IntersectsAABB(cell.AABB))
                                indecis.Add(i);
                        cell.End = indecis.Count;
                        
                        Cells.Add(cell);
                    }
                }
            }
            Indecis = indecis.ToArray();
        }

        private static Vector3 Vec3Min(Vector3 a, Vector3 b) => new Vector3(MathF.Min(a.X, b.X), MathF.Min(a.Y, b.Y), MathF.Min(a.Z, b.Z));
        private static Vector3 Vec3Max(Vector3 a, Vector3 b) => new Vector3(MathF.Max(a.X, b.X), MathF.Max(a.Y, b.Y), MathF.Max(a.Z, b.Z));

        public bool GetGridPosition(Vector3 worldPos, out Vector3 gridPos)
        {
            // round((worldPos - Min - CellSize / 2) / CellSize);
            Vector3 result = new Vector3(MathF.Round((worldPos.X - (Min.X + CellSize.X / 2)) / CellSize.X), MathF.Round((worldPos.Y - (Min.Y + CellSize.Y / 2)) / CellSize.Y), MathF.Round((worldPos.Z - (Min.Z + CellSize.Z / 2)) / CellSize.Z));
            gridPos = result;
            return IsValidGridPosition(gridPos);
        }
           
        public int GetIndex(Vector3 gridPosition)
        {
            int oneX = 1;
            int oneY = Width;
            int oneZ = Width * Height;
            return (int)(gridPosition.Z * oneZ + gridPosition.Y * oneY + gridPosition.X * oneX);
        }

        public bool IsValidGridPosition(Vector3 gridPos) => gridPos.X >= 0 && gridPos.X < Width && gridPos.Y >= 0 && gridPos.Y < Height && gridPos.Z >= 0 && gridPos.Z < Depth;
    }
}