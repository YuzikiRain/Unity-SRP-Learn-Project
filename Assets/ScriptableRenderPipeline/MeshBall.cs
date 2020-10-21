using UnityEngine;

public class MeshBall : MonoBehaviour
{
    static int baseColorId = Shader.PropertyToID("_BaseColor");
    [SerializeField] Mesh mesh;
    [SerializeField] Material material;

    private const int Count = 1022;
    Matrix4x4[] matrices = new Matrix4x4[Count];
    Vector4[] baseColors = new Vector4[Count];
    float[] cutoff = new float[Count];

    MaterialPropertyBlock block;

    private void Awake()
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            matrices[i] = Matrix4x4.TRS(
                Random.insideUnitSphere * 10f,
                Quaternion.Euler(Random.value * 360f, Random.value * 360f, Random.value * 360f),
                Vector3.one * Random.Range(0.5f, 1.5f));
            baseColors[i] = new Vector4(Random.value, Random.value, Random.value, Random.Range(0.5f, 1f));
            cutoff[i] = Random.value;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (block == null)
        {
            block = new MaterialPropertyBlock();
            block.SetVectorArray(baseColorId, baseColors);
            block.SetFloatArray(baseColorId, cutoff);
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, Count, block);
    }
}
