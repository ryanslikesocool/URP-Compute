using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class ComputePass : ScriptableRenderPass
{
    private string profilerTag;

    private RenderTargetIdentifier destination;

    private ComputeAsset computeAsset;

    private RenderTexture target;
    private RenderTexture converged;

    private Material addMaterial;
    private int currentSample;

    public ComputePass(string profilerTag, ComputeFeature.ComputeSettings settings, Material addMaterial)
    {
        this.profilerTag = profilerTag;
        computeAsset = settings.computeAsset;
        renderPassEvent = settings.passEvent;
        this.addMaterial = addMaterial;

        target?.Release();
        target = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        target.enableRandomWrite = true;
        target.Create();

        converged?.Release();
        converged = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
        converged.enableRandomWrite = true;
        converged.Create();

        currentSample = 0;

        computeAsset.Setup();
    }

    public void Setup(RenderTargetIdentifier destination)
    {
        this.destination = destination;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (computeAsset == null || computeAsset.shader == null) { return; }

        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        int kernelHandle = computeAsset.shader.FindKernel("CSMain");

        computeAsset.Render(cmd, kernelHandle);

        cmd.SetComputeTextureParam(computeAsset.shader, 0, "Result", target);
        cmd.DispatchCompute(computeAsset.shader, kernelHandle, Mathf.CeilToInt(Screen.width / 8), Mathf.CeilToInt(Screen.height / 8), 1);

        addMaterial.SetFloat("_Sample", currentSample);
        Blit(cmd, target, converged, addMaterial);
        Blit(cmd, converged, destination);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);

        //currentSample++;
    }

    public void Dispose()
    {
        target?.Release();
        converged?.Release();
    }
}