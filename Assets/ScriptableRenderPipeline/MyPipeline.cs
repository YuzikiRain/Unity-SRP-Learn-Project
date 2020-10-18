using UnityEngine;
using UnityEngine.Rendering;

public class MyPipeline : RenderPipeline
{
    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        //base.Render(context, cameras);

        foreach (var camera in cameras)
        {
            renderer.Render(context, camera);
        }
    }

    CameraRenderer renderer = new CameraRenderer();

}
