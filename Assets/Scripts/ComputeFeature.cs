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
        settings.computeAsset.Setup();
        computePass = new ComputePass(name, settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        RenderTargetIdentifier src = renderer.cameraColorTarget;
        RenderTargetHandle dst = RenderTargetHandle.CameraTarget;
        computePass.Setup(src, dst);
        renderer.EnqueuePass(computePass);
    }

    private void OnDisable()
    {
        settings.computeAsset.Cleanup();
    }
}
