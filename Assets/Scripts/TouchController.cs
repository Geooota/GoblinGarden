using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using static PlotInfo;

public enum GameMode
{
    Normal,
    Building,
    Deconstructing
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
    public PlotInfo plot;

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
        public PlotInfo plotInfo;
        public PlantInfo plantInfo;
    }

    public Camera cam;
    public Tilemap tilemap;
    public GameObject plantPrefab;
    public GameObject placementUI;
    public GameObject deconstructUI;
    public AudioClip plantSound;
    public UIIconFollower iconFollower;
    public TrashCompactor trashCompactor;
    public float compactTime = 0.3f;
    public AudioClip[] clickSound;
    public GameObject trashTarget;
    public List<Sprite> harvestingSprites;
    public List<Sprite> wateringSprites;
    public bool isGoldCost;
    public bool isPlanting;

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
    private float panThreshold = 10f;
    private float panSpeed = 1f;


    private void Start()
    {
        trashText.text = trashAmount.ToString();
        goldText.text = goldAmount.ToString();
    }

    void Update()
    {
        var pointer = UnityEngine.InputSystem.Pointer.current;
        if (pointer == null) return;


        switch (currentMode)
        {
            case GameMode.Normal:
                HandleNormalInput(pointer);
                break;
            case GameMode.Building:
                HandleBuildingInput(pointer);
                break;
            case GameMode.Deconstructing:
                HandleDeconstructInput(pointer);
                break;
        }
    }

    // -------------------------
    // NORMAL MODE (panning only)
    // -------------------------
    private void HandleNormalInput(UnityEngine.InputSystem.Pointer pointer)
    {
        if (pointer.press.wasPressedThisFrame)
        {
            isPressing = true;
            isPanning = false;
            pressScreenPos = pointer.position.ReadValue();
            pressWorldPos = ScreenToWorldOnGround(pressScreenPos);

            if (EventSystem.current.IsPointerOverGameObject())
            {
                isPressing = false;
                return;
            }

            Ray ray = cam.ScreenPointToRay(pressScreenPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {

                // when you detect the click on a plant
                PlantInfo plant = hit.collider.GetComponent<PlantInfo>();

                if (plant != null)
                {
                    // this is how we queue work for John now
                    var job = new Job(
                    plant.gameObject,
                    onComplete: () => CollectCrop(plant),
                    harvestingSprites
                    );

                    JohnController.Instance.EnqueueJob(job);
                    // End how to enqueue jobs
                    isPressing = false; // prevent panning etc.
                    return;
                }

                if (hit.collider.GetComponent<TrashCompactor>() != null)
                {
                    isPressing = false; // prevent panning etc.
                    isHolding = true;
                    if (Vector3.Distance(JohnController.Instance.gameObject.transform.position, trashTarget.transform.position) > 1)
                    {
                        Debug.Log(Vector3.Distance(JohnController.Instance.gameObject.transform.position, trashTarget.transform.position));
                        var takeOutTrash = new Job(trashTarget);
                        JohnController.Instance.EnqueueJob(takeOutTrash);
                    }
                    else
                    {
                        Debug.Log(Vector3.Distance(JohnController.Instance.gameObject.transform.position, trashTarget.transform.position));
                        StartCoroutine(compactTrash());
                    }
                    return;
                }

                else
                {
                    if (plot != null)
                    {
                        if (!EventSystem.current.IsPointerOverGameObject())
                        {
                            plot.MakeOptionsDisappear();
                        }
                    }

                    plot = hit.collider.GetComponent<PlotInfo>();
                    if (plot != null)
                    {
                        if (plot.dry)
                        {
                            var watering = new Job(
                            plot.gameObject,
                            onComplete: () => plot.Wet(),
                            wateringSprites
                            );
                            JohnController.Instance.EnqueueJob(watering);

                        }
                        else
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
    private void HandleBuildingInput(UnityEngine.InputSystem.Pointer pointer)
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
            // Check if the press is over the heldPlant
            if (isDraggingPlant)
            {
                if(isPlanting)
                {
                    if (heldPlant.transform.position != spawnPos && tileInfos.ContainsKey(cellPos))
                    {
                        heldPlant.transform.position = spawnPos;                     // Update heldPlant position
                        int randomIndex = Random.Range(0, clickSound.Length);
                        AudioSource.PlayClipAtPoint(clickSound[randomIndex], Camera.main.transform.position);
                    }
                }
                else if (heldPlant.transform.position != spawnPos && !tileInfos.ContainsKey(cellPos))
                {
                    heldPlant.transform.position = spawnPos;
                }
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
    // DECONSTRUCT MODE
    // -------------------------

    private void HandleDeconstructInput(UnityEngine.InputSystem.Pointer pointer)
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

            if (EventSystem.current.IsPointerOverGameObject())
            {
                isPressing = false;
                return;
            }

            Ray ray = cam.ScreenPointToRay(pressScreenPos);

            // when you detect the click on a plant
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                PlantInfo plant = hit.collider.GetComponent<PlantInfo>();

                if (plant != null)
                {
                    placementUI.SetActive(true);
                    iconFollower.target = plant.transform;
                }
                else
                {
                    plot = hit.collider.GetComponent<PlotInfo>();
                    if (plot != null)
                    {
                        placementUI.SetActive(true);
                        iconFollower.target = plant.transform;
                    }
                }
            }
        }

        // -------------------------
        // While the press/finger is held
        // -------------------------
        if (isPressing && pointer.press.isPressed)
        {
            CameraPanning(currentScreenPos);
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
        if (prefab.GetComponent<PlantInfo>() != null)
        {
            heldCost = prefab.GetComponent<PlantInfo>().cost;
            isGoldCost = false;
            isPlanting = true;
        }

        else if (prefab.GetComponent<PlotInfo>() != null)
        {
            heldCost = prefab.GetComponent<PlotInfo>().cost;
            isGoldCost = true;
            isPlanting = false;
        }
        else 
        {
            isPlanting = false;
            Debug.LogWarning("no Cost on item found");
        }
            currentMode = GameMode.Building;
    }

    public void EnterDeconstructMode()
    {
        ExitBuildMode();
        currentMode = GameMode.Deconstructing;
    }

    public void ConfirmPlacement()
    {
        if (heldPlant == null) return;

        // Snap to grid
        Vector3Int cellPos = tilemap.WorldToCell(heldPlant.transform.position);

        if (isPlanting)
        {
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
                    if (!tileInfos[cellPos].plotInfo.dry)
                        tileInfos[cellPos].plantInfo.StartGrowthCycle();
                    tileInfos[cellPos].plantInfo.myPlot = tileInfos[cellPos].plotInfo;
                    tileInfos[cellPos].plantInfo.myPlotType = tileInfos[cellPos].plotInfo.thisPlotType;
                    tileInfos[cellPos].plantInfo.myCellPos = cellPos;
                    heldPlant = null;

                    AudioSource.PlayClipAtPoint(plantSound, Camera.main.transform.position);

                    trashAmount -= heldCost;
                    trashText.text = trashAmount.ToString();

                    ExitBuildMode();
                }
            }
        }
        else if (heldPlant.GetComponent<PlotInfo>() != null)
        {
            if (goldAmount >= heldCost)
            {
                Vector3Int min;
                Vector3Int size;
                BoundsInt region;
                switch (heldPlant.GetComponent<PlotInfo>().thisPlotSize)
                {
                    case PlotSize.Medium:
                        min = cellPos + new Vector3Int(-2, -2, 0);
                        size = new Vector3Int(5, 5, 1);
                        region = new BoundsInt(min, size);
                        break;

                    case PlotSize.Large:
                        min = cellPos + new Vector3Int(-3, -3, 0);
                        size = new Vector3Int(7, 7, 1);
                        region = new BoundsInt(min, size);
                        break;

                    default:
                        min = cellPos + new Vector3Int(-1, -1, 0);
                        size = new Vector3Int(3, 3, 1);
                        region = new BoundsInt(min, size);
                        break;
                }

                foreach (var pos in region.allPositionsWithin)
                {
                    if(tileInfos.ContainsKey(cellPos))
                    {
                        Debug.Log("A tile is already occupied!");
                        return;
                    }
                }
                heldPlant.GetComponent<PlotInfo>().BuildThisPlot(1);
                heldPlant = null;
                goldAmount -= heldCost;
                goldText.text = goldAmount.ToString();
                ExitBuildMode();






            }

        }
    }

    public void BuildPlot(PlotInfo plotinfo, BoundsInt plotArea)
    {
        foreach (var pos in plotArea.allPositionsWithin)
        {
            Debug.Log($"Cell at {pos}");
            if (!tileInfos.ContainsKey(pos))
            {
                tileInfos[pos] = new TileInfo { isOccupied = false, plotInfo = plotinfo };
                Debug.Log($"Built a {plotinfo.thisPlotType} plot at {pos}");
            }
            else
            {
                tileInfos[pos].plotInfo = plotinfo;
                foreach (var loc in plotinfo.region.allPositionsWithin)
                {
                    if (tileInfos[loc].plantInfo != null)
                        tileInfos[loc].plantInfo.myPlotType = plotinfo.thisPlotType;
                }
                Debug.Log($"Replaced a plot with a {plotinfo.thisPlotType} plot at {pos}");
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
        Debug.Log("Compacting Trash");
        trashCompactor.CompactTrash();
        yield return new WaitForSeconds(compactTime); // Simulate delay for compacting
        compactTime = Mathf.Max(0.8f * compactTime, 0.09f);
        if (isHolding)
            StartCoroutine(compactTrash());
        else
            compactTime = 0.2f;
    }

}
