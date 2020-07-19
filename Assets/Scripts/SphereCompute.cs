using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Shader/Compute/Sphere Compute", fileName = "New Sphere Compute")]
public class SphereCompute : ComputeAsset
{
    public Vector3 lightRotation = new Vector3(30, 25, 0);
    [Range(0, 4)] public float lightIntensity = 1;
    [Range(2, 8)] public int bounces = 4;
    public Texture2D skybox;

    public Vector2 sphereRadius = new Vector2(3.0f, 8.0f);
    public uint spheresMax = 100;
    public float spherePlacementRadius = 100.0f;
    private ComputeBuffer sphereBuffer;
    private List<Sphere> spheres;

    public struct Sphere
    {
        public Vector3 position;
        public float radius;
        public Vector3 albedo;
        public Vector3 specular;
    }

    public override void Setup()
    {
        spheres = new List<Sphere>();
        // Add a number of random spheres
        for (int i = 0; i < spheresMax; i++)
        {
            Sphere sphere = new Sphere();
            // Radius and radius
            sphere.radius = sphereRadius.x + Random.value * (sphereRadius.y - sphereRadius.x);
            Vector2 randomPos = Random.insideUnitCircle * spherePlacementRadius;
            sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);
            // Reject spheres that are intersecting others
            foreach (Sphere other in spheres)
            {
                float minDist = sphere.radius + other.radius;
                if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist)
                    goto SkipSphere;
            }
            // Albedo and specular color
            Color color = Random.ColorHSV();
            bool metal = Random.value < 0.5f;
            sphere.albedo = metal ? Vector3.zero : new Vector3(color.r, color.g, color.b);
            sphere.specular = metal ? new Vector3(color.r, color.g, color.b) : Vector3.one * 0.04f;
            // Add the sphere to the list
            spheres.Add(sphere);
        SkipSphere:
            continue;
        }
    }

    public override void Render(CommandBuffer commandBuffer, Camera camera)
    {
        Cleanup();

        sphereBuffer = new ComputeBuffer(spheres.Count, 40);
        sphereBuffer.SetData(spheres);

        int kernelHandle = shader.FindKernel("CSMain");

        shader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        shader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);

        shader.SetTexture(kernelHandle, "_SkyboxTexture", skybox);
        shader.SetInt("_Bounces", bounces);

        shader.SetBuffer(kernelHandle, "_Spheres", sphereBuffer);
        shader.SetInt("_SphereCount", spheres.Count);

        Vector3 lightDirection = Quaternion.Euler(lightRotation) * Vector3.forward;
        shader.SetVector("_DirectionalLight", new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, lightIntensity));

        commandBuffer.DispatchCompute(shader, kernelHandle, Mathf.CeilToInt(Screen.width / 8), Mathf.CeilToInt(Screen.height / 8), 1);
    }

    public override void Cleanup()
    {
        if (sphereBuffer != null)
        {
            sphereBuffer.Dispose();
        }
    }
}