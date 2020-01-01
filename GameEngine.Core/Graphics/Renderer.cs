﻿using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Veldrid;
using GameEngine.Core.Camera;

namespace GameEngine.Core.Graphics
{
    public class Renderer
    {
        public DeviceBuffer ViewProjBuffer { get; private set; }
        public DeviceBuffer WorldBuffer { get; private set; }
        public DeviceBuffer CameraBuffer { get; private set; }
        public DeviceBuffer LightingBuffer { get; private set; }
        public Vector3 LightDirection { get; set; }

        private readonly Engine engine;
        private readonly Scene scene;
        private readonly CommandList commandList;

        private SceneCameraInfo cameraInfo;
        private SceneLightingInfo lightingInfo;

        public Renderer(Engine engine, Scene scene)
        {
            this.engine = engine;
            this.scene = scene;
            commandList = engine.CommandList;

            CreateBuffers();
        }

        public void Draw()
        {
            var camera = scene.ActiveCamera;
            commandList.UpdateBuffer(ViewProjBuffer, 0, camera.View);
            commandList.UpdateBuffer(ViewProjBuffer, 64, camera.Projection);

            UpdateStructs();
            commandList.UpdateBuffer(CameraBuffer, 0, cameraInfo);
            commandList.UpdateBuffer(LightingBuffer, 0, lightingInfo);

            foreach (var entity in scene.Entities)
            {
                foreach (var renderable in entity.GetComponentsOfType<IRenderable>())
                {
                    renderable.UpdateBuffers(commandList);

                    renderable.Material.Bind(commandList, this, renderable.LayoutDescription);

                    commandList.UpdateBuffer(WorldBuffer, 0, renderable.WorldTransform);

                    commandList.SetVertexBuffer(0, renderable.VertexBuffer);
                    commandList.SetIndexBuffer(renderable.IndexBuffer, IndexFormat.UInt32);

                    commandList.DrawIndexed(
                        indexCount: renderable.IndexBuffer.SizeInBytes / sizeof(uint),
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0);
                }
            }
        }

        private void CreateBuffers()
        {
            var factory = engine.GraphicsDevice.ResourceFactory;

            ViewProjBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Matrix4x4>() * 2, BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            WorldBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<Matrix4x4>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            CameraBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SceneCameraInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
            LightingBuffer = factory.CreateBuffer(new BufferDescription((uint)Unsafe.SizeOf<SceneLightingInfo>(), BufferUsage.UniformBuffer | BufferUsage.Dynamic));
        }

        private void UpdateStructs()
        {
            cameraInfo.CameraDirection = scene.ActiveCamera.ViewDirection;
            cameraInfo.CameraPosition = scene.ActiveCamera.Position;

            // TODO move these all to public variables
            lightingInfo.LightDirection = LightDirection;
            lightingInfo.LightColor = new RgbaFloat(0.95f, 0.94f, 0.7f, 1);
            lightingInfo.LightIntensity = 2;
            lightingInfo.AmbientLight = new RgbaFloat(0.2f, 0.2f, 0.2f, 1);
            lightingInfo.FogColor = RgbaFloat.CornflowerBlue;
            lightingInfo.FogStartDistance = 50;
            lightingInfo.FogEndDistance = 140;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SceneCameraInfo
        {
            public Vector3 CameraDirection;
            private float _padding1;
            public Vector3 CameraPosition;
            private float _padding2;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct SceneLightingInfo
        {
            public Vector3 LightDirection;
            private float _padding1;
            public RgbaFloat LightColor;
            public float LightIntensity;
            private float _padding2;
            private float _padding3;
            private float _padding4;
            public RgbaFloat AmbientLight;
            public RgbaFloat FogColor;
            public float FogStartDistance;
            private float _padding5;
            private float _padding6;
            private float _padding7;
            public float FogEndDistance;
            private float _padding8;
            private float _padding9;
            private float _padding10;
        }
    }
}
