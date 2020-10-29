using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class PerObjectMaterialProperties : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    static int cutoffId = Shader.PropertyToID("_Cutoff");
    static int metallicId = Shader.PropertyToID("_Metallic");
    static int smoothnessId = Shader.PropertyToID("_Smoothness");

    [SerializeField] private Color baseColor = Color.white;
    [SerializeField] [Range(0f, 1f)] private float cutoff = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float metallic = 0f;
    [SerializeField] [Range(0f, 1f)] private float smoothness = 0.5f;

    static MaterialPropertyBlock block;
    private Renderer _renderer;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (block == null) { block = new MaterialPropertyBlock(); }
        _renderer = GetComponent<Renderer>();

        block.SetColor(baseColorId, baseColor);
        block.SetFloat(cutoffId, cutoff);
        block.SetFloat(metallicId, metallic);
        block.SetFloat(smoothnessId, smoothness);

        _renderer.SetPropertyBlock(block);
    }
}
