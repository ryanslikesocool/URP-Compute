using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class ComputeFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class ComputeSettings
    {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingOpaques;
        public ComputeAsset computeAsset;
    }

    public ComputeSettings settings = new ComputeSettings();

    private ComputePass computePass;

    public override void Create()
    {
        if (settings.computeAsset == null) { return; }

        settings.computeAsset.Setup();
        computePass = new ComputePass(name, settings, new Material(Shader.Find("Hidden/AddShader")));
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.computeAsset == null) { return; }

        RenderTargetIdentifier src = renderer.cameraColorTarget;
        computePass.Setup(src);
        renderer.EnqueuePass(computePass);
    }

    private void OnDisable()
    {
        settings.computeAsset?.Cleanup();
    }
}
