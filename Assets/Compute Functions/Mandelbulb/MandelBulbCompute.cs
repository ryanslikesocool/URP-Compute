using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Shader/Compute/Mandelbulb Compute", fileName = "New Mandelbulb Compute")]
public class MandelBulbCompute : ComputeAsset
{
    [Range(1, 50)]
    public float fractalPower = 10;
    public float darkness = 70;

    [Header("Colour mixing")]
    [Range(0, 1)] public float blackAndWhite;
    public Color colorA = Color.blue;
    public Color colorB = Color.red;

    [Space] public Vector3 lightRotation = new Vector3(30, 25, 0);

    public override void Render(CommandBuffer commandBuffer, int kernelHandle)
    {
        Camera camera = Camera.current;
        if (camera == null) { camera = GameObject.FindObjectOfType<Camera>(); }

        commandBuffer.SetComputeFloatParam(shader, "power", Mathf.Max(fractalPower, 1.01f));
        commandBuffer.SetComputeFloatParam(shader, "darkness", darkness);
        commandBuffer.SetComputeFloatParam(shader, "blackAndWhite", blackAndWhite);
        commandBuffer.SetComputeVectorParam(shader, "colourAMix", new Vector3(colorA.r, colorA.b, colorA.g));
        commandBuffer.SetComputeVectorParam(shader, "colourBMix", new Vector3(colorB.r, colorB.b, colorB.g));

        commandBuffer.SetComputeMatrixParam(shader, "_CameraToWorld", camera.cameraToWorldMatrix);
        commandBuffer.SetComputeMatrixParam(shader, "_CameraInverseProjection", camera.projectionMatrix.inverse);
        commandBuffer.SetComputeVectorParam(shader, "_LightDirection", Quaternion.Euler(lightRotation) * Vector3.forward);
    }
}