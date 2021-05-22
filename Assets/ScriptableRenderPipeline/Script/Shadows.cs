using UnityEngine;
using UnityEngine.Rendering;

public class Shadows
{
    private const string bufferName = "Shadows";
    private const int maxShadowedDirectionalLightCount = 4;
    private const int maxCascades = 4;

    private static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");
    private static int dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");
    private static int cascadeCountId = Shader.PropertyToID("_CascadeCount");
    private static int cascadeCulingSpheresId = Shader.PropertyToID("_CascadeCullingSpheres");
    private static int shadowDistanceId = Shader.PropertyToID("_ShadowDistance");

    private static Vector4[] cascadeCullingSpheres = new Vector4[maxCascades];

    private static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount * maxCascades];

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    private int shadowedDirectionalLightCount;

    CommandBuffer buffer = new CommandBuffer { name = bufferName };

    ScriptableRenderContext context;
    CullingResults cullingResults;
    ShadowSettings settings;


    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        shadowedDirectionalLightCount = 0;
    }

    public void Cleanup()
    {
        if (shadowedDirectionalLightCount > 0)
        {
            buffer.ReleaseTemporaryRT(dirShadowAtlasId);
            ExecuteBuffer();
        }
    }

    /// <summary>
    /// 取得会产生阴影的方向光信息
    /// </summary>
    /// <param name="light"></param>
    /// <param name="visibleLightIndex"></param>
    public Vector2 ReserveDirectionalShadows(Light light, int visibleLightIndex)
    {
        if (shadowedDirectionalLightCount < maxShadowedDirectionalLightCount
            && light.shadows != LightShadows.None
            && light.shadowStrength > 0f
            // 在光照Bounds内
            && cullingResults.GetShadowCasterBounds(visibleLightIndex, out var _))
        {
            shadowedDirectionalLights[shadowedDirectionalLightCount] = new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
            return new Vector2(light.shadowStrength, settings.directional.cascadeCount * shadowedDirectionalLightCount++);
        }
        return Vector2.zero;
    }

    public void Render()
    {
        if (shadowedDirectionalLightCount > 0)
        { RenderDirectionalShadows(); }
    }

    private void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        buffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        buffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.ClearRenderTarget(true, false, Color.clear);

        buffer.BeginSample(bufferName);
        ExecuteBuffer();

        // 多个灯光，要将ShadowMap拆分
        int tiles = shadowedDirectionalLightCount * settings.directional.cascadeCount;
        int split = tiles <= 1 ? 1 : tiles <= 4 ? 2 : 4;
        int tileSize = atlasSize / split;

        for (int i = 0; i < shadowedDirectionalLightCount; i++)
        {
            // 渲染每个ShadowMap切片
            RenderDirectionalShadows(i, split, tileSize);
        }

        // 将级联计数和球体发送到GPU
        buffer.SetGlobalInt(cascadeCountId, settings.directional.cascadeCount);
        buffer.SetGlobalVectorArray(cascadeCulingSpheresId, cascadeCullingSpheres);

        buffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);

        buffer.SetGlobalFloat(shadowDistanceId, settings.maxDistance);
        buffer.EndSample(bufferName);
        ExecuteBuffer();
    }

    private void RenderDirectionalShadows(int index, int split, int tileSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[index];
        ShadowDrawingSettings shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);

        int cascadeCount = settings.directional.cascadeCount;
        int tileOffset = index * cascadeCount;
        Vector3 ratios = settings.directional.CascadeRatios;

        for (int i = 0; i < cascadeCount; i++)
        {
            cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, i, cascadeCount, ratios, tileSize, 0f,
                out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

            shadowSettings.splitData = splitData;

            // 只需要对第一个光源执行此操作，因为所有光源的级联都是等效的。
            if (index == 0)
            {
                Vector4 cullingSphere = splitData.cullingSphere;
                // w分量表示球的半径，存储半径平方来比较，速度更快
                cullingSphere.w *= cullingSphere.w;
                cascadeCullingSpheres[i] = cullingSphere;
            }

            // 从世界空间到光空间的转换矩阵
            // 由于可能一个ShadowMap中使用了Tile来存储多个阴影，
            int tileIndex = tileOffset + i;
            dirShadowMatrices[tileIndex] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(tileIndex, split, tileSize), split);
            buffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
            ExecuteBuffer();
            context.DrawShadows(ref shadowSettings);
        }
    }

    private Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;
        }
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);
        return m;
    }

    /// <summary>
    /// 根据分割数split以及当前index确定 Tile 的offset
    /// </summary>
    /// <param name="index"></param>
    /// <param name="split"></param>
    /// <param name="tileSize"></param>
    /// <returns></returns>
    private Vector2 SetTileViewport(int index, int split, float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        buffer.SetViewport(new Rect(offset.x * tileSize, offset.y * tileSize, tileSize, tileSize));
        return offset;
    }

    private void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(buffer);
        buffer.Clear();
    }
}
