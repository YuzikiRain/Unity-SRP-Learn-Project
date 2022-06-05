using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPB : MonoBehaviour
{
    MaterialPropertyBlock MaterialPropertyBlock;

    private void OnValidate()
    {
        MaterialPropertyBlock = new MaterialPropertyBlock();
        //MaterialPropertyBlock.SetColor("_BaseColor", Color.green);
        GetComponent<Renderer>().SetPropertyBlock(MaterialPropertyBlock);
    }
}
