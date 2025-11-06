using UnityEngine;

public class UISound : MonoBehaviour
{
    public static UISound Instance;

    public AudioClip[] buttonClickSound;
    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject);
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayButtonClick()
    {
        if (audioSource != null && buttonClickSound != null && buttonClickSound.Length > 0)
        {
            int index = Random.Range(0, buttonClickSound.Length);
            audioSource.PlayOneShot(buttonClickSound[index]);
        }
    }
}