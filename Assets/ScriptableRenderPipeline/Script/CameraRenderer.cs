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
    // 一个通用的绘制命令缓冲区，
    CommandBuffer buffer = new CommandBuffer() { name = BufferName };
    // 在当前SRP中不支持的所有Unity默认的shader，需要绘制成洋红色
    static ShaderTagId[] legacyShaderTagIds =
    {
        new ShaderTagId("Always"),
        new ShaderTagId("ForwardBase"),
        new ShaderTagId("PrepassBase"),
        new ShaderTagId("Vertex"),
        new ShaderTagId("VertexLMRGBM"),
        new ShaderTagId("VertexLM"),
    };
    CullingResults cullingResults;

    Lighting lighting = new Lighting();

    public void Render(ScriptableRenderContext context, Camera camera, bool enableDynamicBatching, bool enableGPUInstancing, ShadowSettings shadowSettings)
    {
        this.context = context;
        this.camera = camera;

        PrepareBuffer();
        // 当使用相机渲染Scene视图时，将UI添加到世界几何物体中
        PrepareForSceneWindow();
        // 如果获得剔除内容失败，则终止渲染
        if (!Cull(shadowSettings.maxDistance)) { return; }
        //buffer.BeginSample(SampleName);
        //ExecuteBuffer();
        //lighting.Setup(context, cullingResults, shadowSettings);
        //buffer.EndSample(SampleName);

        // 设置矩阵等属性
        Setup();
        // 设置光照（主要是设置光照属性到以便shader
        lighting.Setup(context, cullingResults, shadowSettings);

        // 绘制可见的几何图形
        DrawVisibleGeometry(enableDynamicBatching, enableGPUInstancing);
        // 绘制不受支持的Shader
        DrawUnsupportedShaders();
        // 绘制Gizmos，应当最后绘制
        DrawGizmos();

        lighting.Cleanup();

        // 向context发出的绘制命令实际上都只是被添加到缓冲中，直到主动提交它。因此必须调用context.Submit()来提交缓冲的（队列形式的）工作
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

        // 为Scene窗口渲染时，必须通过EmitWorldGeometryForSceneView并使用Camera作为参数来将UI显式地添加到世界几何物体中
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
        }

#endif
    }

    private bool Cull(float maxShadowDistance)
    {
        // 如果要手动计算剔除的内容，需要跟踪多个相机设置和矩阵
        // 也可以用camera.TryGetCullingParameters来使用Unity默认的相机剔除设置
        if (camera.TryGetCullingParameters(out ScriptableCullingParameters cullingParameters))
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
        // 将摄像机属性（主要是位置、旋转、FOV等）应用于上下文。设置矩阵以及其他一些属性
        context.SetupCameraProperties(camera);
        // 清除渲染目标
        // Skybox = 0，Color = 1，SolidColor = 2，Depth = 3，都会清除深度缓冲，不同在于如何清除颜色缓冲
        CameraClearFlags flags = camera.clearFlags;
        buffer.ClearRenderTarget(
            flags <= CameraClearFlags.Depth,
            flags == CameraClearFlags.Color,
            flags == CameraClearFlags.Color ?
            camera.backgroundColor.linear : Color.clear);
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
        //#if UNITY_EDITOR
        if (_errorMaterial == null) { _errorMaterial = new Material(Shader.Find("Hidden/InternalErrorShader")); }
        // overrideMaterial 表示用一个material来作为这次绘制设置的通用材质，而不是每个pass使用单独的material
        var drawingSettings = new DrawingSettings(legacyShaderTagIds[0], new SortingSettings(camera)) { overrideMaterial = _errorMaterial };
        for (int i = 1, length = legacyShaderTagIds.Length; i < length; i++)
        {
            drawingSettings.SetShaderPassName(i, legacyShaderTagIds[i]);
        }
        var filteringSettings = FilteringSettings.defaultValue;

        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        //#endif
    }

    /// <summary>
    /// 支持的Tag
    /// </summary>
    static ShaderTagId unlitShaderTagId = new ShaderTagId("SRPDefaultUnlit");
    static ShaderTagId litShaderTagId = new ShaderTagId("CustomLit");

    /// <summary>
    /// 绘制可见几何物体
    /// </summary>
    /// <param name="enableDynamicBatching"></param>
    /// <param name="enableGPUInstancing"></param>
    private void DrawVisibleGeometry(bool enableDynamicBatching, bool enableGPUInstancing)
    {
        // 用于决定应用正交还是基于距离的排序
        SortingSettings sortingSettings = new SortingSettings(camera) { criteria = SortingCriteria.CommonOpaque };
        // DrawingSettings中的ShaderTagId表示允许tag为ShaderTagId的pass
       DrawingSettings drawingSettings = new DrawingSettings(unlitShaderTagId, sortingSettings) { enableDynamicBatching = enableDynamicBatching, enableInstancing = enableGPUInstancing };
       //  设置第0个pass的tag为 unlitShaderTagId
        drawingSettings.SetShaderPassName(0, unlitShaderTagId);
        // 设置第1个pass的tag为 litShaderTagId
        drawingSettings.SetShaderPassName(1, litShaderTagId);
        // 这种写法不行
        //FilteringSettings filteringSettings = new FilteringSettings() { renderQueueRange = RenderQueueRange.all, layerMask = -1 };
        // FilteringSettings设置了允许使用哪些渲染队列
        // 先绘制不透明物体
        FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange.opaque, -1);
        context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
        // 再绘制天空盒
        // DrawSkybox是绘制天空盒专用的命令
        context.DrawSkybox(camera);
        // 最后再绘制透明物体
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
