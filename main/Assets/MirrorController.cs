using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class MirrorInverterURP : MonoBehaviour
{
    private Camera cam;

    void OnEnable()
    {
        cam = GetComponent<Camera>();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == cam)
        {
            camera.ResetProjectionMatrix();
            camera.projectionMatrix *= Matrix4x4.Scale(new Vector3(-1, 1, 1));
        }
    }
}