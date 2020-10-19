using UnityEngine.Rendering;
using UnityEngine;

public class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;
    private const string BufferName = "Render Camera";
    CommandBuffer cameraBuffer = new CommandBuffer() { name = BufferName };
    ScriptableCullingParameters cullingParameters;
    CullingResults cullingResults;

    public void Render(ScriptableRenderContext context, Camera camera)
    {
        this.context = context;
        this.camera = camera;

        if (!Cull()) { return; }
        Setup();

        // 绘制几何
        DrawVisibleGeometry();
        DrawGizmos();

        Submit();
    }

    private bool Cull()
    {
        if (camera.TryGetCullingParameters(out cullingParameters))
        {
            cullingParameters.isOrthographic = false;
            cullingResults = context.Cull(ref cullingParameters);
            return true;
        }
        return false;
    }

    private void Setup()
    {
        // 将摄像机属性应用于上下文。设置矩阵以及其他一些属性
        context.SetupCameraProperties(camera);
        // 清除用的Buffer
        cameraBuffer.ClearRenderTarget(true, true, Color.clear);
        cameraBuffer.BeginSample(BufferName);
        ExcuteBuffer();
    }

    private void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();
    }

    //static ShaderTagId unlitShaderTagId = new ShaderTagId("UniversalForward");
    //static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId unlitShaderTagId = new ShaderTagId("Always");

    private void DrawVisibleGeometry()
    {
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings);
        // 这种写法不行
        //FilteringSettings filteringSettings = new FilteringSettings() { renderQueueRange = RenderQueueRange.all, layerMask = -1 };
        // 必须要这么写
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
        //filteringSettings = new FilteringSettings(RenderQueueRange.transparent, -1);
        filteringSettings.renderQueueRange = RenderQueueRange.transparent;
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
    }

    private void DrawGizmos()
    {
#if UNITY_EDITOR
        if (UnityEditor.Handles.ShouldRenderGizmos())
        {
            context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
            context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
        }
#endif
    }

    private void Submit()
    {
        cameraBuffer.EndSample(BufferName);
        ExcuteBuffer();
        context.Submit();
    }
}
