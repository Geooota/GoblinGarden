using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    void LateUpdate() // Use LateUpdate to ensure camera movement is processed first
    {
        if (mainCameraTransform != null)
        {
            transform.LookAt(mainCameraTransform);

            // Optional: Lock rotation to a specific axis (e.g., only rotate around Y-axis)
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        }
    }
}
