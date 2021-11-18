using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Compute/Sphere Compute")]
public class SphereCompute : ComputeAsset {
    public int randomSeed;

    [Space] public Vector3 lightRotation = new Vector3(30, 25, 0);
    [Range(0, 4)] public float lightIntensity = 1;
    [Range(2, 8)] public int bounces = 4;
    public Texture2D skybox;

    [Space] public Vector2 sphereRadius = new Vector2(3.0f, 8.0f);
    public uint spheresMax = 100;
    public float spherePlacementRadius = 100.0f;
    private ComputeBuffer sphereBuffer;
    private List<Sphere> spheres;

    public struct Sphere {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    }

    public override void Setup() {
        Random.InitState(randomSeed);

        spheres = new List<Sphere>();

        for (int i = 0; i < spheresMax; i++) {
            Sphere sphere = new Sphere();

            sphere.radius = sphereRadius.x + Random.value * (sphereRadius.y - sphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * spherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

            foreach (Sphere other in spheres) {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist) {
                    goto SkipSphere;
                }
            }

            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;

            spheres.Add(sphere);
        SkipSphere:
            continue;
        }
    }

    public override void Render(CommandBuffer commandBuffer, int kernelHandle) {
        Cleanup();

        Camera camera = Camera.main;

        sphereBuffer = new ComputeBuffer(spheres.Count, 40);
        commandBuffer.SetBufferData(sphereBuffer, spheres);

        commandBuffer.SetComputeMatrixParam(shader, "_CameraToWorld", camera.cameraToWorldMatrix);
        commandBuffer.SetComputeMatrixParam(shader, "_CameraInverseProjection", camera.projectionMatrix.inverse);

        Vector3 lightDirection = Quaternion.Euler(lightRotation) * Vector3.forward;
        commandBuffer.SetComputeVectorParam(shader, "_DirectionalLight", new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, lightIntensity));

        commandBuffer.SetComputeTextureParam(shader, kernelHandle, "_SkyboxTexture", skybox);
        commandBuffer.SetComputeIntParam(shader, "_Bounces", bounces);

        commandBuffer.SetComputeBufferParam(shader, kernelHandle, "_Spheres", sphereBuffer);
        commandBuffer.SetComputeIntParam(shader, "_SphereCount", spheres.Count);

        //commandBuffer.SetComputeVectorParam(shader, "_PixelOffset", new Vector2(Random.value, Random.value));
    }

    public override void Cleanup() {
        sphereBuffer?.Dispose();
    }
}