using Unity.XR.Oculus.Input;
using UnityEngine;

public class TrashCompactor : MonoBehaviour
{
    private int trashAmount = 0;
    private int goldAmount = 0;
    private Vector3 trashCompactorTransform;
    public TilemapClicker touchController;
    public AudioClip compactSound;

    private CameraShake cameraShake;

    private void Start()
    {
        cameraShake = Camera.main.GetComponent<CameraShake>();

        trashCompactorTransform = new Vector3(transform.position.x, transform.position.y + 10, transform.position.z);

        trashAmount = touchController.trashAmount;
        goldAmount = touchController.goldAmount;
    }

    public void CompactTrash()
    {
        Debug.Log("Compacting Trash");
        trashAmount = touchController.trashAmount;
        goldAmount = touchController.goldAmount;

        if (trashAmount >= 10)
        {
            Debug.Log("Trash Compacted");
            trashAmount -= 5;
            goldAmount += 1;
            touchController.trashAmount = trashAmount;
            touchController.goldAmount = goldAmount;
            touchController.trashText.text = trashAmount.ToString();
            touchController.goldText.text = goldAmount.ToString();
            AudioSource.PlayClipAtPoint(compactSound, trashCompactorTransform);

            if (cameraShake != null)
            {
                cameraShake.ShakeCamera();
            }
        }
    }
}
