using UnityEngine;
using UnityEngine.Rendering;

public class CustomPipeline : RenderPipeline
{
    private bool enableDynamicBatching, enableGPUInstancing;

    public CustomPipeline(bool enableDynamicBatching, bool enableGPUInstancing, bool enableSRPBatcher)
    {
        this.enableDynamicBatching = enableDynamicBatching;
        this.enableGPUInstancing = enableGPUInstancing;
        GraphicsSettings.useScriptableRenderPipelineBatching = enableSRPBatcher;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            renderer.Render(context, camera, this.enableDynamicBatching, this.enableGPUInstancing);
        }
    }

    CameraRenderer renderer = new CameraRenderer();

}
