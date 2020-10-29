using UnityEngine;
using UnityEngine.Rendering;

public class CustomPipeline : RenderPipeline
{
    private bool enableDynamicBatching, enableGPUInstancing;
    [SerializeField] private ShadowSettings shadowSettings = default;

    public CustomPipeline(bool enableDynamicBatching, bool enableGPUInstancing, bool enableSRPBatcher, ShadowSettings shadowSettings)
    {
        this.enableDynamicBatching = enableDynamicBatching;
        this.enableGPUInstancing = enableGPUInstancing;
        this.shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = enableSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (var camera in cameras)
        {
            renderer.Render(context, camera, enableDynamicBatching, enableGPUInstancing, shadowSettings);
        }
    }

    CameraRenderer renderer = new CameraRenderer();

}
