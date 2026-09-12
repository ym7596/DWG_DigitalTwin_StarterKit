using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    [Header("Main Camera")]
    [SerializeField] private CinemachineCamera mainVCam;
    [Header("Priority Settings")]
    [SerializeField] private int mainPriority = 10;
    [SerializeField] private int activePriority = 20;
    private CinemachineCamera currentActiveVCam;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    private void Start()
    {
        ReturnToMainCamera();
    }

    public void SwitchToFacilityCamera(CinemachineCamera targetVCam)
    {
        if (targetVCam == null) return;
        // 기존 선택된 Facility VCam이 있다면 Priority 원복
        if (currentActiveVCam != null && currentActiveVCam != targetVCam)
        {
            currentActiveVCam.Priority = 0;
        }
        
        currentActiveVCam = targetVCam;
        currentActiveVCam.Priority = activePriority;
        mainVCam.Priority = mainPriority;
    }

    public void ReturnToMainCamera()
    {
        if (currentActiveVCam != null)
        {
            currentActiveVCam.Priority = 0;
            currentActiveVCam = null;
        }
        // 메인 카메라 Priority를 높게 유지
        if (mainVCam != null)
        {
            mainVCam.Priority = mainPriority;
        }
    }
}
