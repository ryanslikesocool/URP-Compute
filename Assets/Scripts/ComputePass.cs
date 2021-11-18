// Developed with love by Ryan Boyer http://ryanjboyer.com <3

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;

public class ComputePass : ScriptableRenderPass {
    private string profilerTag;

    private ComputeAsset computeAsset;

    private readonly int TargetBufferID = Shader.PropertyToID("targetBuffer");
    private readonly int ConvergedBufferID = Shader.PropertyToID("convergedBuffer");

    private Material addMaterial;
    private int currentSample;

    public ComputePass(string profilerTag, ComputeFeature.ComputeSettings settings) {
        this.profilerTag = profilerTag;
        computeAsset = settings.computeAsset;
        renderPassEvent = settings.passEvent;

        currentSample = 0;

        computeAsset.Setup();
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData) {
        base.OnCameraSetup(cmd, ref renderingData);
        RenderTextureDescriptor textureDescriptor = renderingData.cameraData.cameraTargetDescriptor;
        textureDescriptor.enableRandomWrite = true;

        cmd.GetTemporaryRT(TargetBufferID, textureDescriptor);
        ConfigureTarget(TargetBufferID);

        cmd.GetTemporaryRT(ConvergedBufferID, textureDescriptor);
        ConfigureTarget(ConvergedBufferID);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData) {
        if (computeAsset == null || computeAsset.shader == null) { return; }
        if (addMaterial == null) {
            addMaterial = new Material(Shader.Find("Hidden/AddShader"));
        }

        CommandBuffer cmd = CommandBufferPool.Get();
        ScriptableRenderer renderer = renderingData.cameraData.renderer;

        int kernelHandle = computeAsset.shader.FindKernel("CSMain");

        using (new ProfilingScope(cmd, new ProfilingSampler("Compute Pass"))) {
            computeAsset.Render(cmd, kernelHandle);

            cmd.SetComputeTextureParam(computeAsset.shader, kernelHandle, "Result", TargetBufferID);
            cmd.DispatchCompute(computeAsset.shader, kernelHandle, Mathf.CeilToInt(Screen.width / 8), Mathf.CeilToInt(Screen.height / 8), 1);

            addMaterial.SetFloat("_Sample", currentSample);
            Blit(cmd, TargetBufferID, ConvergedBufferID, addMaterial);
            Blit(cmd, ConvergedBufferID, renderer.cameraColorTarget);
        }

        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        CommandBufferPool.Release(cmd);
        //currentSample++;
    }

    public override void OnCameraCleanup(CommandBuffer cmd) {
        cmd.ReleaseTemporaryRT(TargetBufferID);
        cmd.ReleaseTemporaryRT(ConvergedBufferID);
    }

    public void Dispose() => Material.Destroy(addMaterial);
}