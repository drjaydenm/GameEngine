using System.Numerics;

namespace GameEngine.Core.Graphics
{
    public static class ShapeBuilder
    {
        public static Vector3[] BuildCubeVertices()
        {
            return new Vector3[] {
                // Top
                new Vector3(-0.5f, +0.5f, -0.5f),
                new Vector3(+0.5f, +0.5f, -0.5f),
                new Vector3(+0.5f, +0.5f, +0.5f),
                new Vector3(-0.5f, +0.5f, +0.5f),
                // Bottom
                new Vector3(-0.5f, -0.5f, +0.5f),
                new Vector3(+0.5f, -0.5f, +0.5f),
                new Vector3(+0.5f, -0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                // Left
                new Vector3(-0.5f, +0.5f, -0.5f),
                new Vector3(-0.5f, +0.5f, +0.5f),
                new Vector3(-0.5f, -0.5f, +0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                // Right
                new Vector3(+0.5f, +0.5f, +0.5f),
                new Vector3(+0.5f, +0.5f, -0.5f),
                new Vector3(+0.5f, -0.5f, -0.5f),
                new Vector3(+0.5f, -0.5f, +0.5f),
                // Back
                new Vector3(+0.5f, +0.5f, -0.5f),
                new Vector3(-0.5f, +0.5f, -0.5f),
                new Vector3(-0.5f, -0.5f, -0.5f),
                new Vector3(+0.5f, -0.5f, -0.5f),
                // Front
                new Vector3(-0.5f, +0.5f, +0.5f),
                new Vector3(+0.5f, +0.5f, +0.5f),
                new Vector3(+0.5f, -0.5f, +0.5f),
                new Vector3(-0.5f, -0.5f, +0.5f)
            };
        }

        public static uint[] BuildCubeIndicies()
        {
            return new uint[]
            {
                0,1,2, 0,2,3,
                4,5,6, 4,6,7,
                8,9,10, 8,10,11,
                12,13,14, 12,14,15,
                16,17,18, 16,18,19,
                20,21,22, 20,22,23
            };
        }
    }
}
