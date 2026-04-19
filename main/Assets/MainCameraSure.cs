using UnityEngine;

public class CameraPriorityFix : MonoBehaviour
{
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();

        cam.tag = "MainCamera";
        cam.enabled = true;

        cam.depth = 99;
    }

    void Update()
    {
        if (!cam.enabled) cam.enabled = true;
    }
}