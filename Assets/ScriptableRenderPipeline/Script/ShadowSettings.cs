using UnityEngine;

[System.Serializable]
public class ShadowSettings
{
    [Min(0f)] public float maxDistance = 100f;
    [System.Serializable]
    public struct Directional
    {
        [Range(1, 4)] public int cascadeCount;
        [Range(0f, 1f)] public float cascadeRatio1, cascadeRatio2, cascadeRatio3;
        public Vector3 CascadeRatios => new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
        public TextureSize atlasSize;
    }
    public Directional directional = new Directional
    {
        atlasSize = TextureSize._1024,
        cascadeCount = 4,
        cascadeRatio1 = 0.1f,
        cascadeRatio2 = 0.25f,
        cascadeRatio3 = 0.5f,
    };
    public enum TextureSize { _256 = 256, _512 = 512, _1024 = 1024, _2024 = 2048, _4096 = 4096, _8192 = 8192, }

}
