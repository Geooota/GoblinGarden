using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    public GameObject shopMenu;
    public GameObject trashMenu;
    public GameObject goldMenu;
    private bool isShopOpen = false;
    private bool isTrashOpen = false;
    private bool isGoldOpen = false;


    public void StartGame()
    {
        SceneManager.LoadScene("CaveWhitebox");
        Debug.Log("Start game.");
    }

    public void ToggleShopMenu()
    {
        if (isShopOpen)
        {
            shopMenu.SetActive(false);
            isShopOpen = false;

            Time.timeScale = 1f;
        }
        else
        {
            shopMenu.SetActive(true);
            isShopOpen = true;

            Time.timeScale = 0f;
        }
    }

    public void ToggleTrashMenu()
    {
        if (isTrashOpen)
        {
            trashMenu.SetActive(false);
            isTrashOpen = false;
        }
        else
        {
            trashMenu.SetActive(true);
            isTrashOpen = true;
        }
    }

    public void ToggleGoldMenu()
    {
        if (isGoldOpen)
        {
            goldMenu.SetActive(false);
            isGoldOpen = false;
        }
        else
        {
            goldMenu.SetActive(true);
            isGoldOpen = true;
        }
    }
}