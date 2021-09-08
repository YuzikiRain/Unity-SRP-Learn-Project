using UnityEngine;
using UnityEngine.Rendering;

public class CustomRenderPipeline : RenderPipeline
{
    private bool enableDynamicBatching, enableGPUInstancing;
    [SerializeField] private ShadowSettings shadowSettings = default;

    public CustomRenderPipeline(bool enableDynamicBatching, bool enableGPUInstancing, bool enableSRPBatcher, ShadowSettings shadowSettings)
    {
        this.enableDynamicBatching = enableDynamicBatching;
        this.enableGPUInstancing = enableGPUInstancing;
        this.shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = enableSRPBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }

    /// <summary>
    /// 每一渲染帧（不是MonoBehavior的逻辑帧），渲染管线会使用所有相机进行渲染
    /// </summary>
    /// <param name="context"></param>
    /// <param name="cameras"></param>
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // 这里的相机数组是通过camera.depth从小到大排序的
        foreach (var camera in cameras)
        {
            cameraRenderer.Render(context, camera, enableDynamicBatching, enableGPUInstancing, shadowSettings);
        }
    }

    CameraRenderer cameraRenderer = new CameraRenderer();

}
