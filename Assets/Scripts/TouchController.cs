using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public enum GameMode
{
    Normal,
    Building
}

public enum PlotType
{
    Dirty,
    Speedy,
    Watery,
    Golden
}



public class TilemapClicker : MonoBehaviour
{
    public static TilemapClicker Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // ensure only one instance exists
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // optional: persists across scenes
    }
    public class TileInfo
    {
        public bool isOccupied;
        public PlotType plotType;
        public PlantInfo plantInfo;
    }

    public Camera cam;
    public Tilemap tilemap;
    public GameObject plantPrefab;
    public GameObject placementUI;
    public UIIconFollower iconFollower;

    private GameMode currentMode = GameMode.Normal;
    private GameObject heldPlant;
    // Change hashset to track the type of plant and plot on each cell
    public Dictionary<Vector3Int, TileInfo> tileInfos = new Dictionary<Vector3Int, TileInfo>();
    private bool isDraggingPlant = false;
    public TMPro.TextMeshProUGUI trashText;
    public TMPro.TextMeshProUGUI goldText;

    // Panning state
    private bool isPressing = false;
    private bool isPanning = false;
    private bool isHolding = false;
    public int trashAmount = 10;
    public int goldAmount = 100;
    private int heldCost = 0;
    private Vector2 pressScreenPos;
    private Vector3 pressWorldPos;
    public float panThreshold = 10f;
    public float panSpeed = 1f;


    private void Start()
    {
        trashText.text = trashAmount.ToString();
        goldText.text = goldAmount.ToString();
    }

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

            Ray ray = cam.ScreenPointToRay(pressScreenPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {

                // when you detect the click on a plant
                PlantInfo plant = hit.collider.GetComponent<PlantInfo>();
                TrashCompactor trashCompactor = hit.collider.GetComponent<TrashCompactor>();

                if (plant != null)
                {
                    // this is how we queue work for John now
                    var job = new Job(
                    plant.gameObject,
                    onComplete: () => CollectCrop(plant)
                    );

                    JohnController.Instance.EnqueueJob(job);
                    // End how to enqueue jobs
                    isPressing = false; // prevent panning etc.
                    return;
                }

                if (trashCompactor != null)
                {
                    Debug.Log("Found Trash Compactor");
                    StartCoroutine(compactTrash());
                    isPressing = false; // prevent panning etc.
                    return;
                }

                else
                {
                    PlotInfo plot = hit.collider.GetComponent<PlotInfo>();
                    if (plot != null)
                    {
                        plot.MakeOptionsAppear();
                    }
                }
            }
        }

        if (isPressing && pointer.press.isPressed)
        {
            Vector2 currentScreenPos = pointer.position.ReadValue();
            float distance = Vector2.Distance(currentScreenPos, pressScreenPos);

            if (!isPanning && distance > panThreshold)
                isPanning = true;

            if (isPanning)
            {
                CameraPanning(currentScreenPos);
            }
        }

        if (pointer.press.wasReleasedThisFrame)
        {
            // No placement in normal mode
            isPressing = false;
            isPanning = false;
            isHolding = false;
        }
    }

    public void CollectCrop(PlantInfo plant)
    {
        if (plant.collectable == true)
        {
            trashAmount += plant.CollectCrop();
            trashText.text = trashAmount.ToString();
            isPressing = false;
            return;
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
                CameraPanning(currentScreenPos);
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
        heldCost = prefab.GetComponent<PlantInfo>().cost;
        currentMode = GameMode.Building;
    }

    public void ConfirmPlacement()
    {
        if (heldPlant == null) return;

        // Snap to grid
        Vector3Int cellPos = tilemap.WorldToCell(heldPlant.transform.position);

        if (tileInfos.ContainsKey(cellPos) && trashAmount >= heldCost)
        {
            if (tileInfos[cellPos].isOccupied)
            {
                Debug.Log("Tile is already occupied!");
                return;
            }
            else
            {
                tileInfos[cellPos].isOccupied = true;
                tileInfos[cellPos].plantInfo = heldPlant.GetComponent<PlantInfo>();
                tileInfos[cellPos].plantInfo.StartGrowthCycle();
                tileInfos[cellPos].plantInfo.myCellPos = cellPos;
                heldPlant = null;

                trashAmount -= heldCost;
                trashText.text = trashAmount.ToString();

                ExitBuildMode();
            }
        }
    }

    public void BuildPlot(PlotType plotType, BoundsInt plotArea)
    {
        foreach (var pos in plotArea.allPositionsWithin)
        {
            Debug.Log($"Cell at {pos}");
            if (!tileInfos.ContainsKey(pos))
            {
                tileInfos[pos] = new TileInfo { isOccupied = false, plotType = plotType };
                Debug.Log($"Built a {plotType} plot at {pos}");
            }
            else
            {
                tileInfos.Remove(pos);
                tileInfos[pos] = new TileInfo { isOccupied = false, plotType = plotType };
                Debug.Log($"Replaced a plot with a {plotType} plot at {pos}");
            }
        }
    }

    public void CameraPanning(Vector2 scPos)
    {
        Vector3 currentWorldPos = ScreenToWorldOnGround(scPos);             // Convert current pointer to world
        Vector3 delta = pressWorldPos - currentWorldPos;                    // Calculate movement delta
        cam.transform.position += delta * panSpeed;                         // Move camera by delta
        pressWorldPos = ScreenToWorldOnGround(scPos);                       // Reset reference for continuous panning
    }

    private System.Collections.IEnumerator compactTrash()
    {
        Debug.Log("Starting compact trash coroutine");
        yield return new WaitForSeconds(0.2f); // Simulate delay for compacting
        TrashCompactor trashCompactor = GetComponent<TrashCompactor>();
        if (trashCompactor != null)
        {
            Debug.Log("Compacting Trash");
            trashCompactor.CompactTrash();
        }
    }

}
