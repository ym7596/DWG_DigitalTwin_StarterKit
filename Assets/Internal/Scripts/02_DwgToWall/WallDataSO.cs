using UnityEngine;

[CreateAssetMenu(fileName = "WallData", menuName = "DigitalTwin/WallData")]
public class WallDataSO : ScriptableObject
{
    public float wallHeight;
    public float wallThickness;
    public Material wallMaterial;
    [Header("실제 dwg 화면 대비 축소/확대 비율")]
    public float magnificationRate;
}