using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 该资产为SRP模板，提供了CreatePipeline方法来创建CustomRenderPipeline实例
/// <para>还序列化了创建CustomRenderPipeline时的默认设置</para>
/// </summary>
[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline Asset")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField] bool enableDynamicBatching = true, enableGPUInstancing = true, enableSRPBatcher = true;
    [SerializeField] private ShadowSettings shadowSettings = default;

    /// <summary>
    /// 重写该方法，返回CustomRenderPipeline实例
    /// </summary>
    /// <returns></returns>
    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(enableDynamicBatching, enableGPUInstancing, enableSRPBatcher, shadowSettings);
    }


}
