using UnityEngine;

public class UIIconFollower : MonoBehaviour
{
    public Transform target;   // The world object to follow
    public Vector3 offset;     // Extra offset if needed
    private Camera mainCam;
    private RectTransform rectTransform;
    public bool isPlot;

    void Awake()
    {
        mainCam = Camera.main;
        rectTransform = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (target == null) return;



        // Convert world position to screen position
        Vector3 screenPos = mainCam.WorldToScreenPoint(target.position + offset);
        if (isPlot)
        {
            screenPos.y -= 100;
        }

        // Hide if behind camera
        if (screenPos.z < 0)
        {
            rectTransform.gameObject.SetActive(false);
        }
        else
        {
            rectTransform.gameObject.SetActive(true);
            rectTransform.position = screenPos;
        }
    }
}
