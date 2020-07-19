using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Shader/Compute/Sphere Compute", fileName = "New Sphere Compute")]
public class SphereCompute : ComputeAsset
{
    private Camera camera;
    [Range(0, 8)] public int bounces = 4;
    public Texture2D skybox;

    public override void Setup()
    {
        camera = FindObjectOfType<Camera>();
    }

    public override void Render(CommandBuffer commandBuffer)
    {
        shader.SetMatrix("_CameraToWorld", camera.cameraToWorldMatrix);
        shader.SetMatrix("_CameraInverseProjection", camera.projectionMatrix.inverse);

        shader.SetTexture(0, "_SkyboxTexture", skybox);
        shader.SetInt("_Bounces", bounces);

        commandBuffer.DispatchCompute(shader, 0, Mathf.CeilToInt(Screen.width / 8), Mathf.CeilToInt(Screen.height / 8), 1);
    }
}