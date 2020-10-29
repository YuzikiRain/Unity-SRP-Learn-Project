using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class CustomShaderGUI : ShaderGUI
{
    private MaterialEditor editor;
    private Object[] materials;
    MaterialProperty[] properties;

    private bool Clipping { set => SetProperty("_Clipping", "_CLIPPING", value); }
    private bool PremultiplyAlpha { set => SetProperty("_PremultiplyAlpha", "_PREMULTIPLY_ALPHA", value); }
    private BlendMode SrcBlend { set => SetProperty("_SrcBlend", (float)value); }
    private BlendMode DstBlend { set => SetProperty("_DstBlend", (float)value); }
    private bool ZWrite { set => SetProperty("_ZWrite", value ? 1f : 0f); }

    RenderQueue RenderQueue
    {
        set
        {
            foreach (Material material in materials)
            {
                material.renderQueue = (int)value;
            }
        }
    }

    enum RenderingMode { Opaque, Cutoff, Fade, Transparent, }
    string[] UnlitRenderingMode = new string[] { "Opaque", "Cutoff", "Fade", };
    RenderingMode renderingMode;
    private bool HasProperty(string name) => FindProperty(name, properties, false) != null;
    private bool HasPremultiplyAlpha => HasProperty("_PremultiplyAlpha");

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        editor = materialEditor;
        materials = materialEditor.targets;
        this.properties = properties;

        var newRenderingMode = (RenderingMode)EditorGUILayout.Popup(nameof(RenderingMode), (int)renderingMode,
            HasPremultiplyAlpha ? System.Enum.GetNames(typeof(RenderingMode)) : UnlitRenderingMode);
        if (newRenderingMode != renderingMode)
        {
            renderingMode = newRenderingMode;
            switch (renderingMode)
            {
                case RenderingMode.Opaque:
                    Clipping = false;
                    PremultiplyAlpha = false;
                    SrcBlend = BlendMode.One;
                    DstBlend = BlendMode.Zero;
                    ZWrite = true;
                    RenderQueue = RenderQueue.Geometry;
                    break;
                case RenderingMode.Cutoff:
                    Clipping = true;
                    PremultiplyAlpha = false;
                    SrcBlend = BlendMode.One;
                    DstBlend = BlendMode.Zero;
                    ZWrite = true;
                    RenderQueue = RenderQueue.AlphaTest;
                    break;
                case RenderingMode.Fade:
                    Clipping = false;
                    PremultiplyAlpha = false;
                    SrcBlend = BlendMode.SrcAlpha;
                    DstBlend = BlendMode.OneMinusSrcAlpha;
                    ZWrite = false;
                    RenderQueue = RenderQueue.Transparent;
                    break;
                case RenderingMode.Transparent:
                    Clipping = false;
                    PremultiplyAlpha = true;
                    SrcBlend = BlendMode.One;
                    DstBlend = BlendMode.OneMinusSrcAlpha;
                    ZWrite = false;
                    RenderQueue = RenderQueue.Transparent;
                    break;
                default:
                    break;
            }
        }

        base.OnGUI(materialEditor, properties);
    }

    private bool SetProperty(string name, float value)
    {
        var property = FindProperty(name, properties);
        if (property != null)
        {
            property.floatValue = value;
            return true;
        }
        else { return false; }
    }

    private void SetKeyword(string keyword, bool enabled)
    {
        if (enabled)
        {
            foreach (Material material in materials)
            {
                material.EnableKeyword(keyword);
            }
        }
        else
        {
            foreach (Material material in materials)
            {
                material.DisableKeyword(keyword);
            }
        }
    }

    private void SetProperty(string name, string keyword, bool value)
    {
        if (SetProperty(name, value ? 1f : 0f))
        {
            SetKeyword(keyword, value);
        }
    }
}
