using UnityEngine;
[CreateAssetMenu(fileName = "PlantInfoContainer", menuName = "Scriptable Objects/PlantInfoContainer")]
public class PlantInfoContainer : ScriptableObject
{
    public int plantCost;
    public string plantName;
    public GameObject plantMesh;
}
