using UnityEngine;

public class DestroyGoop : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Color c = spriteRenderer.color;
    }

    private void Update()
    {
        Color c = spriteRenderer.color;
        c.a -= Time.deltaTime * 0.2f;
        spriteRenderer.color = c;
        if (c.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
