﻿using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ComputePass : ScriptableRenderPass
{
    private string profilerTag;

    private RenderTargetIdentifier source { get; set; }
    private RenderTargetHandle destination { get; set; }

    private ComputeAsset computeAsset;
    private RenderTexture target;
    private RenderTargetIdentifier targetIdentifier;

    public ComputePass(string profilerTag, ComputeFeature.ComputeSettings settings)
    {
        this.profilerTag = profilerTag;
        computeAsset = settings.computeAsset;
        renderPassEvent = settings.passEvent;
    }

    public void Setup(RenderTargetIdentifier source, RenderTargetHandle destination)
    {
        this.source = source;
        this.destination = destination;
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        if (target != null)
        {
            target.Release();
        }

        target = new RenderTexture(cameraTextureDescriptor)
        {
            enableRandomWrite = true
        };
        target.Create();

        targetIdentifier = new RenderTargetIdentifier(target);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (computeAsset == null || computeAsset.shader == null) { return; }

        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);

        cmd.SetComputeTextureParam(computeAsset.shader, 0, "Result", targetIdentifier);
        computeAsset.Render(cmd);

        Blit(cmd, targetIdentifier, source);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
}