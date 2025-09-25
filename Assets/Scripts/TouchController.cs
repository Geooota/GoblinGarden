using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public enum GameMode
{
    Normal,
    Building
}

public class TilemapClicker : MonoBehaviour
{
    public Camera cam;
    public Tilemap tilemap;
    public GameObject plantPrefab;
    public GameObject placementUI;
    public UIIconFollower iconFollower;

    private GameMode currentMode = GameMode.Normal;
    private GameObject heldPlant;
    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private bool isDraggingPlant = false;

    // Panning state
    private bool isPressing = false;
    private bool isPanning = false;
    private Vector2 pressScreenPos;
    private Vector3 pressWorldPos;
    public float panThreshold = 10f;
    public float panSpeed = 1f;

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null) return;

        
        switch (currentMode)
        {
            case GameMode.Normal:
                HandleNormalInput(pointer);
                break;
            case GameMode.Building:
                HandleBuildingInput(pointer);
                break;
        }
    }

    // -------------------------
    // NORMAL MODE (panning only)
    // -------------------------
    private void HandleNormalInput(Pointer pointer)
    {
        if (pointer.press.wasPressedThisFrame)
        {
            isPressing = true;
            isPanning = false;
            pressScreenPos = pointer.position.ReadValue();
            pressWorldPos = ScreenToWorldOnGround(pressScreenPos);
        }

        if (isPressing && pointer.press.isPressed)
        {
            Vector2 currentScreenPos = pointer.position.ReadValue();
            float distance = Vector2.Distance(currentScreenPos, pressScreenPos);

            if (!isPanning && distance > panThreshold)
                isPanning = true;

            if (isPanning)
            {
                Vector3 currentWorldPos = ScreenToWorldOnGround(currentScreenPos);
                Vector3 delta = pressWorldPos - currentWorldPos;
                cam.transform.position += delta * panSpeed;
                pressWorldPos = ScreenToWorldOnGround(currentScreenPos);
            }
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            // No placement in normal mode
            isPressing = false;
            isPanning = false;
        }
    }

    // -------------------------
    // BUILDING MODE
    // -------------------------
    private void HandleBuildingInput(Pointer pointer)
    {
        // Get the current pointer/finger position on the screen
        Vector2 currentScreenPos = pointer.position.ReadValue();

        Vector3 worldPos = ScreenToWorldOnGround(currentScreenPos); // Convert pointer to world space
        Vector3Int cellPos = tilemap.WorldToCell(worldPos);          // Convert world position to tilemap cell
        Vector3 spawnPos = tilemap.GetCellCenterWorld(cellPos);      // Get center of that cell

        // -------------------------
        // Detect initial press
        // -------------------------
        if (pointer.press.wasPressedThisFrame)
        {
            if (IsPointerOverUI())
                return;
            // Start tracking a press
            isPressing = true;

            // Store the initial screen position of the press
            pressScreenPos = currentScreenPos;

            // Convert the screen position to a world position on the ground plane (y=0)
            pressWorldPos = ScreenToWorldOnGround(pressScreenPos);

            // instead of raycasting, find if the screen pos is the same as the heldPlant transform
            if (spawnPos == heldPlant.transform.position)
            {
                Debug.Log("Found plant under thumb");
                isDraggingPlant = true;
            }
            else 
                isDraggingPlant = false;
        }

        // -------------------------
        // While the press/finger is held
        // -------------------------
        if (isPressing && pointer.press.isPressed)
        {
            // Check if the press is over the heldPlant (or a button, if included in IsPointerOverObject)
            if (isDraggingPlant)
            {
                // Move the heldPlant to follow the pointer, snapping to the tilemap grid

                heldPlant.transform.position = spawnPos;                     // Update heldPlant position
            }
            else
            {
                // If not over the heldPlant, assume the player wants to pan the camera
                Vector3 currentWorldPos = ScreenToWorldOnGround(currentScreenPos); // Convert current pointer to world
                Vector3 delta = pressWorldPos - currentWorldPos;                   // Calculate movement delta
                cam.transform.position += delta * panSpeed;                        // Move camera by delta
                pressWorldPos = ScreenToWorldOnGround(currentScreenPos);                                   // Reset reference for continuous panning
            }
        }

        // -------------------------
        // Release press/finger
        // -------------------------
        if (pointer.press.wasReleasedThisFrame)
        {
            // Stop tracking the press
            isPressing = false;
        }
    }


    // -------------------------
    // HELPERS
    // -------------------------
    private Vector3 ScreenToWorldOnGround(Vector2 screenPos)
    {
        Ray ray = cam.ScreenPointToRay(screenPos);
        Plane ground = new Plane(Vector3.up, Vector3.zero);
        if (ground.Raycast(ray, out float enter))
            return ray.GetPoint(enter);
        return Vector3.zero;
    }

    // Switch back to normal gameplay
    public void ExitBuildMode()
    {
        if (heldPlant != null)
        {
            Destroy(heldPlant);
            heldPlant = null;
        }

        placementUI.SetActive(false);
        currentMode = GameMode.Normal;
        plantPrefab = null;
    }

    // Call this from UI when player selects a plant to build
    public void EnterBuildMode(GameObject prefab)
    {
        ExitBuildMode(); // cleanup first
        plantPrefab = prefab;

        // If no ghost yet, spawn one at screen center
        if (heldPlant == null)
        {
            Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
            Vector3 worldPos = ScreenToWorldOnGround(screenCenter);
            Vector3Int cellPos = tilemap.WorldToCell(worldPos);
            Vector3 spawnPos = tilemap.GetCellCenterWorld(cellPos);

            heldPlant = Instantiate(plantPrefab, spawnPos, Quaternion.identity);

            // Show placement UI & attach to ghost
            placementUI.SetActive(true);
            iconFollower.target = heldPlant.transform;
        }

        currentMode = GameMode.Building;
    }
    public void ConfirmPlacement()
    {
        if (heldPlant == null) return;

        // Snap to grid
        Vector3Int cellPos = tilemap.WorldToCell(heldPlant.transform.position);

        if (!occupiedCells.Contains(cellPos))
        {
            occupiedCells.Add(cellPos);
            heldPlant = null;
            ExitBuildMode();
        }
        else
        {
            Debug.Log("Cell occupied!");
        }
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

}
