using Unity.XR.Oculus.Input;
using UnityEngine;

public class TrashCompactor : MonoBehaviour
{
    private int trashAmount = 0;
    private int goldAmount = 0;
    public TilemapClicker touchController;

    private void Start()
    {
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
        }
    }
}
