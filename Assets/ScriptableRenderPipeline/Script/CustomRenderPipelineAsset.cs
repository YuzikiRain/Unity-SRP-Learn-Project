using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline Asset")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField] bool enableDynamicBatching = true, enableGPUInstancing = true, enableSRPBatcher = true;
    [SerializeField] private ShadowSettings shadowSettings = default;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomPipeline(enableDynamicBatching, enableGPUInstancing, enableSRPBatcher, shadowSettings);
    }


}
