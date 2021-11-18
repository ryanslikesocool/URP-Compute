// Developed with love by Ryan Boyer http://ryanjboyer.com <3

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class ComputeFeature : ScriptableRendererFeature {
    [Serializable]
    public class ComputeSettings {
        public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingOpaques;
        public ComputeAsset computeAsset;
    }

    public ComputeSettings settings = new ComputeSettings();

    private ComputePass computePass;

    public override void Create() {
        if (settings.computeAsset == null) { return; }

        settings.computeAsset.Setup();
        computePass = new ComputePass(name, settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData) {
        if (settings.computeAsset == null) { return; }
        renderer.EnqueuePass(computePass);
    }

    private void OnDisable() {
        settings.computeAsset?.Cleanup();
    }
}
