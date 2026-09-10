using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIPopupCCTV _cctvPopup;
    [SerializeField] private UIPopupTemp _tempPopup;

    private void Awake()
    {
        _cctvPopup.gameObject.SetActive(false);
        _tempPopup.gameObject.SetActive(false);
    }

    public void ShowPopup(Facility data)
    {
        var type = data.facilityType;
        switch (type)
        {
            case FacilityType.CCTV:
                
                _cctvPopup.OpenPopup(new CctvData()
                    {
                        DeviceID = data.facilityId,
                        DeviceName = data.facilityName,
                        StreamUrl = "localhost:8080/test",
                        ResolutionH = 1080,
                        ResolutionW = 1920
                    }
                   );
                
                break;
            case FacilityType.TempSensor:
                break;
            default:
                break;
        }
    }
}
