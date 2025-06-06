﻿using System.Numerics;

namespace GameEngine.Core.Graphics
{
    public static class ShapeBuilder
    {
        public static VertexPositionNormalTexCoord[] BuildCubeVertices()
        {
            return new VertexPositionNormalTexCoord[] {
                // Top
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, +0.5f, -0.5f), Vector3.UnitY, Vector2.Zero),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, +0.5f, -0.5f), Vector3.UnitY, Vector2.UnitX),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, +0.5f, +0.5f), Vector3.UnitY, Vector2.One),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, +0.5f, +0.5f), Vector3.UnitY, Vector2.UnitY),
                // Bottom
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, -0.5f, +0.5f), -Vector3.UnitY, Vector2.Zero),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, -0.5f, +0.5f), -Vector3.UnitY, Vector2.UnitX),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, -0.5f, -0.5f), -Vector3.UnitY, Vector2.One),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitY, Vector2.UnitY),
                // Left
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, +0.5f, -0.5f), -Vector3.UnitX, Vector2.Zero),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, +0.5f, +0.5f), -Vector3.UnitX, Vector2.UnitX),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, -0.5f, +0.5f), -Vector3.UnitX, Vector2.One),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitX, Vector2.UnitY),
                // Right
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, +0.5f, +0.5f), Vector3.UnitX, Vector2.Zero),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, +0.5f, -0.5f), Vector3.UnitX, Vector2.UnitX),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, -0.5f, -0.5f), Vector3.UnitX, Vector2.One),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, -0.5f, +0.5f), Vector3.UnitX, Vector2.UnitY),
                // Back
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, +0.5f, -0.5f), -Vector3.UnitZ, Vector2.Zero),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, +0.5f, -0.5f), -Vector3.UnitZ, Vector2.UnitX),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, -0.5f, -0.5f), -Vector3.UnitZ, Vector2.One),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, -0.5f, -0.5f), -Vector3.UnitZ, Vector2.UnitY),
                // Front
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, +0.5f, +0.5f), Vector3.UnitZ, Vector2.Zero),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, +0.5f, +0.5f), Vector3.UnitZ, Vector2.UnitX),
                new VertexPositionNormalTexCoord(new Vector3(+0.5f, -0.5f, +0.5f), Vector3.UnitZ, Vector2.One),
                new VertexPositionNormalTexCoord(new Vector3(-0.5f, -0.5f, +0.5f), Vector3.UnitZ, Vector2.UnitY),
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

        // Taken from https://github.com/caosdoar/spheres/blob/master/src/spheres.cpp
        // Using the spherified cube algorithm
        public static VertexPositionNormalTexCoord[] BuildSphereVertices(int divisions)
        {
            var step = 1.0f / divisions;
            var step3 = new Vector3(step);
            var vertices = new List<VertexPositionNormalTexCoord>();

            for (var face = 0; face < 6; ++face)
            {
                var origin = CubeToSphere.Origins[face];
                var right = CubeToSphere.Rights[face];
                var up = CubeToSphere.Ups[face];

                for (var j = 0; j < divisions + 1; ++j)
                {
                    var j3 = new Vector3(j);

                    for (var i = 0; i < divisions + 1; ++i)
                    {
                        var i3 = new Vector3(i);
                        var p = origin + step3 * (i3 * right + j3 * up);
                        var p2 = p * p;
                        var n = new Vector3(
                            p.X * (float)Math.Sqrt(1.0f - 0.5f * (p2.Y + p2.Z) + p2.Y * p2.Z / 3.0f),
                            p.Y * (float)Math.Sqrt(1.0f - 0.5f * (p2.Z + p2.X) + p2.Z * p2.X / 3.0f),
                            p.Z * (float)Math.Sqrt(1.0f - 0.5f * (p2.X + p2.Y) + p2.X * p2.Y / 3.0f));
                        // TODO fix up UV coordinates
                        vertices.Add(new VertexPositionNormalTexCoord(n, n, new Vector2(n.X, n.Y)));
                    }
                }
            }

            return vertices.ToArray();
        }

        public static uint[] BuildSphereIndices(int divisions)
        {
            var indices = new List<uint>();

            var k = divisions + 1;
            for (var face = 0; face < 6; ++face)
            {
                for (var j = 0; j < divisions; ++j)
                {
                    //var bottom = j < (divisions / 2);
                    for (var i = 0; i < divisions; ++i)
                    {
                        //var left = i < (divisions / 2);
                        var a = (uint)((face * k + j) * k + i);
                        var b = (uint)((face * k + j) * k + i + 1);
                        var c = (uint)((face * k + j + 1) * k + i);
                        var d = (uint)((face * k + j + 1) * k + i + 1);
                        
                        indices.Add(d);
                        indices.Add(c);
                        indices.Add(a);
                        
                        indices.Add(a);
                        indices.Add(b);
                        indices.Add(d);
                    }
                }
            }

            return indices.ToArray();
        }
    }

    public static class CubeToSphere
    {
        public static Vector3[] Origins =
        {
            new Vector3(-1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, -1.0f),
            new Vector3(1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f),
            new Vector3(-1.0f, 1.0f, -1.0f),
            new Vector3(-1.0f, -1.0f, 1.0f)
        };

        public static Vector3[] Rights =
        {
            new Vector3(2.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 2.0f),
            new Vector3(-2.0f, 0.0f, 0.0f),
            new Vector3(0.0f, 0.0f, -2.0f),
            new Vector3(2.0f, 0.0f, 0.0f),
            new Vector3(2.0f, 0.0f, 0.0f)
        };

        public static Vector3[] Ups =
        {
            new Vector3(0.0f, 2.0f, 0.0f),
            new Vector3(0.0f, 2.0f, 0.0f),
            new Vector3(0.0f, 2.0f, 0.0f),
            new Vector3(0.0f, 2.0f, 0.0f),
            new Vector3(0.0f, 0.0f, 2.0f),
            new Vector3(0.0f, 0.0f, -2.0f)
        };
    }
}
