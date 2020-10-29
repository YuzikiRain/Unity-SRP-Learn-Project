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
    CommandBuffer buffer = new CommandBuffer() { name = BufferName };
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

    Lighting lighting = new Lighting();

    public void Render(ScriptableRenderContext context, Camera camera, bool enableDynamicBatching, bool enableGPUInstancing, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        PrepareForSceneWindow();
        if (!Cull(shadowSettings.maxDistance)) { return; }
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
        lighting.Setup(context, cullingResults, shadowSettings);
        buffer.EndSample(SampleName);
        Setup();

        // 绘制几何
        DrawVisibleGeometry(enableDynamicBatching, enableGPUInstancing);
        // 绘制不受支持的Shader
        DrawUnsupportedShaders();
        // 绘制Gizmos
        DrawGizmos();

        lighting.Cleanup();
        Submit();
    }

    private void PrepareBuffer()
    {
        Profiler.BeginSample("Editor Only");
        buffer.name = SampleName = camera.name;
        Profiler.EndSample();
    }

    private void PrepareForSceneWindow()
    {
#if UNITY_EDITOR

        // 为Scene窗口渲染时，必须通过EmitWorldGeometryForSceneView并使用Camera作为参数来将UI显式地添加到世界几何中
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }

#endif
    }

    private bool Cull(float maxShadowDistance)
    {
        if (camera.TryGetCullingParameters(out cullingParameters))
        {
            //cullingParameters.isOrthographic = false;
            cullingParameters.shadowDistance = Mathf.Min(maxShadowDistance, camera.farClipPlane);
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
        buffer.ClearRenderTarget(flags <= CameraClearFlags.Depth, flags == CameraClearFlags.Color, Color.clear);
        buffer.BeginSample(SampleName);
        ExecuteBuffer();
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
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
    static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");
    private void DrawVisibleGeometry(bool enableDynamicBatching, bool enableGPUInstancing)
    {
        var sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { enableDynamicBatching = enableDynamicBatching, enableInstancing = enableGPUInstancing };
        drawingSettings.SetShaderPassName(1, litShaderTagId);
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
        buffer.EndSample(SampleName);
        ExecuteBuffer();
        context.Submit();
    }
}
