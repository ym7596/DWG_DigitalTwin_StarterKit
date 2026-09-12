using UnityEngine;

public class UIManagerComplete2 : MonoBehaviour
{
    [SerializeField] private UIPopupCCTV _cctvPopup;
    [SerializeField] private UIPopupTempComplete _tempPopup;

    private void Start()
    {
        CloseAllPopups();
    }

    public void ShowPopup(Facility data)
    {
        if (data == null) return;

        CloseAllPopups();

        switch (data.facilityType)
        {
            case FacilityType.TempSensor:
                _tempPopup.gameObject.SetActive(true);
                // 1초마다 난수 수신 -> UI 갱신
                MockTelemetryService.Instance.StartStreaming(data, (streamData) =>
                {
                    var data = new TempData()
                    {
                        DeviceID = streamData.deviceId,
                        DeviceName = streamData.deviceName,
                        IsOnline = streamData.isOnline,
                        Temperature = streamData.Temperature,
                        Humidity = streamData.Humidity
                    };
                    _tempPopup.RefreshData(data);
                });
                break;

            case FacilityType.CCTV:
                _cctvPopup.gameObject.SetActive(true);
                MockTelemetryService.Instance.StartStreaming(data, (streamData) =>
                {
                    
                    var data = new CctvData()
                    {
                        DeviceID = streamData.deviceId,
                        DeviceName = streamData.deviceName,
                        IsOnline = streamData.isOnline,
                        ResolutionH = streamData.ResolutionH,
                        ResolutionW = streamData.ResolutionW
                    };
                    _cctvPopup.RefreshData(data);
                });
                break;
        }
    }
    
    
    public void CloseAllPopups()
    {
        // 팝업이 닫히면 코루틴도 정지
        if (MockTelemetryService.Instance != null)
        {
            MockTelemetryService.Instance.StopStreaming();
        }

        _cctvPopup.gameObject.SetActive(false);
        _tempPopup.gameObject.SetActive(false);
    }
}
