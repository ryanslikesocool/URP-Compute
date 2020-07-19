using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public abstract class ComputeAsset : ScriptableObject
{
    public ComputeShader shader;

    public virtual void Setup() { }
    public abstract void Render(CommandBuffer commandBuffer);
}