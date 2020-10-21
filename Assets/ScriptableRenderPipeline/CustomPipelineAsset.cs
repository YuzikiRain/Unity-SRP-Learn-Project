using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Pipeline Asset")]
public class CustomPipelineAsset : RenderPipelineAsset
{
    [SerializeField] bool enableDynamicBatching = true, enableGPUInstancing = true, enableSRPBatcher = true;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomPipeline(enableDynamicBatching, enableGPUInstancing, enableSRPBatcher);
    }


}
