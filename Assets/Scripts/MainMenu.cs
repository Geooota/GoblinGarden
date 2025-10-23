using UnityEngine;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour
{
    public GameObject shopMenu;
    private bool isShopOpen = false;

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
        }
        else
        {
            shopMenu.SetActive(true);
            isShopOpen = true;
        }
    }
}

