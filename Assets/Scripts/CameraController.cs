using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Camera cam;
    private float zoomSpeed = 5f;
    private float minZoom = 2f;
    private float maxZoom = 10f;

    public float maxXCam = 22.5f;
    public float minXCam = -36.5f;
    public float maxZCam = 23f;
    public float minZCam = -36.5f;

    private float targetZoom;

    void Start()
    {
        targetZoom = cam.orthographicSize;
    }

    void Update()
    {
        HandleMouseZoom();
        HandleTouchZoom();
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * zoomSpeed);

        ResetCameraSmoothly();

    }
    private void HandleMouseZoom()
    {
        if (Mouse.current == null)
            return;

        float scroll = Mouse.current.scroll.ReadValue().y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            targetZoom -= scroll * 2f;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }
    }


    private void HandleTouchZoom()
    {
        if (Touchscreen.current == null) return;

        if (Touchscreen.current.touches.Count >= 2)
        {
            var touch0 = Touchscreen.current.touches[0];
            var touch1 = Touchscreen.current.touches[1];

            if (touch0.isInProgress && touch1.isInProgress)
            {
                Vector2 pos0 = touch0.position.ReadValue();
                Vector2 pos1 = touch1.position.ReadValue();

                // Previous frame positions
                Vector2 pos0Prev = pos0 - touch0.delta.ReadValue();
                Vector2 pos1Prev = pos1 - touch1.delta.ReadValue();

                float prevDist = Vector2.Distance(pos0Prev, pos1Prev);
                float currDist = Vector2.Distance(pos0, pos1);

                float diff = currDist - prevDist;
                targetZoom -= diff * 0.01f; // adjust sensitivity
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
    }
    private void ResetCameraSmoothly()
    {
        Vector3 camPos = cam.transform.position;

        float targetX = Mathf.Clamp(camPos.x, minXCam, maxXCam);
        float targetZ = Mathf.Clamp(camPos.z, minZCam, maxZCam);

        Vector3 targetPos = new Vector3(targetX, camPos.y, targetZ);

        cam.transform.position = Vector3.Lerp(camPos, targetPos, Time.deltaTime * 5f);
    }


}
