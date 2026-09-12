using Unity.Cinemachine;
using UnityEngine;

public enum FacilityType { CCTV, TempSensor }
public class Facility : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    
    public string facilityId = "FAC-001";
    public string facilityName = "공정 1구역 CCTV";
    public FacilityType facilityType = FacilityType.CCTV;
    
    public CinemachineCamera Camera => _camera;

    // 클릭 시 시각적 피드백 (선택 강조용 간단한 색상 변경 등)
    public void OnSelected()
    {
        Debug.Log($"[선택됨] {facilityName} ({facilityId})");
    }
}
