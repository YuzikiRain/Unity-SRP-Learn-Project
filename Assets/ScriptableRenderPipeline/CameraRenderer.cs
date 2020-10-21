using UnityEngine.Rendering;
using UnityEngine;
using UnityEngine.Profiling;

public class CameraRenderer
{
    ScriptableRenderContext context;
    Camera camera;
#if UNITY_EDITOR
    private string SampleName { get; set; }
#else
    private const SampleName = BufferName;
#endif
    private const string BufferName = "Render Camera";
    CommandBuffer cameraBuffer = new CommandBuffer() { name = BufferName };
    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };
    ScriptableCullingParameters cullingParameters;
    CullingResults cullingResults;

    public void Render(ScriptableRenderContext context, Camera camera, bool enableDynamicBatching, bool enableGPUInstancing)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull()) { return; }
        Setup();

        // 绘制不受支持的Shader
        DrawUnsupportedShaders();
        // 绘制几何
        DrawVisibleGeometry(enableDynamicBatching, enableGPUInstancing);
        // 绘制Gizmos
        DrawGizmos();

        Submit();
    }



    private void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        cameraBuffer.name = SampleName = camera.name;
        Profiler.EndSample();
    }

    private void PrepareForSceneWindow()
    {
#if UNITY_EDITOR

        // 为Scene窗口渲染时，必须通过EmitWorldGeometryForSceneView并使用Camera作为参数来讲UI显式地添加到世界几何中
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }

#endif
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
        CameraClearFlags flags = camera.clearFlags;
        cameraBuffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, Color.clear);
        cameraBuffer.BeginSample(SampleName);
        ExcuteBuffer();
    }

    private void ExcuteBuffer()
    {
        context.ExecuteCommandBuffer(cameraBuffer);
        cameraBuffer.Clear();
    }

    private static Material _errorMaterial = null;

    private void DrawUnsupportedShaders()
    {
#if UNITY_EDITOR
        if (_errorMaterial == null) { _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader")); }

        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera)) { overrideMaterial = _errorMaterial };
        for (int i = 1, length = legacyShaderTagIds.Length; i < length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
#endif
    }

    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    private void DrawVisibleGeometry(bool enableDynamicBatching, bool enableGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { enableDynamicBatching = enableDynamicBatching, enableInstancing = enableGPUInstancing };
        // 这种写法不行
        //FilteringSettings filteringSettings = new FilteringSettings() { renderQueueRange = RenderQueueRange.all, layerMask = -1 };
        // 必须要这么写
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);

        context.DrawSkybox(camera);

        sortingSettings.criteria = SortingCriteria.CommonTransparent;
        drawingSettings.sortingSettings = sortingSettings;
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
        cameraBuffer.EndSample(SampleName);
        ExcuteBuffer();
        context.Submit();
    }
}
