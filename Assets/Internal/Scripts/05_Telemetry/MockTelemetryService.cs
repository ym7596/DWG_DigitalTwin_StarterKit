using System;
using System.Collections;
using UnityEngine;

public class MockTelemetryService : MonoBehaviour
{
    public static MockTelemetryService Instance { get; private set; }

    private Coroutine _telemetryCoroutine;

    private void Awake()
    {
        Instance = this;
    }

    // 특정 설비에 대한 실시간 수신 시작
    public void StartStreaming(Facility facility, Action<object> onDataReceived)
    {
        StopStreaming();
        _telemetryCoroutine = StartCoroutine(CoStreamData(facility, onDataReceived));
    }

    public void StopStreaming()
    {
        if (_telemetryCoroutine != null)
        {
            StopCoroutine(_telemetryCoroutine);
            _telemetryCoroutine = null;
        }
    }

    private IEnumerator CoStreamData(Facility facility, Action<object> onDataReceived)
    {
        var wait = new WaitForSeconds(1.0f);

        while (true)
        {
            if (facility.facilityType == FacilityType.TempSensor)
            {
                // 18℃ ~ 35℃ 사이 난수 생성 (30도 이상이면 경고용)
                var tempData = new TempData
                {
                    DeviceID = facility.facilityId,
                    DeviceName = facility.facilityName,
                    Temperature = UnityEngine.Random.Range(20.0f, 33.0f),
                    Humidity = UnityEngine.Random.Range(40.0f, 65.0f)
                };
                onDataReceived?.Invoke(tempData);
            }
            else if (facility.facilityType == FacilityType.CCTV)
            {
                var cctvData = new CctvData
                {
                    DeviceID = facility.facilityId,
                    DeviceName = facility.facilityName,
                    StreamUrl = "localhost:8080/test",
                    ResolutionH = 1080,
                    ResolutionW = 1920
                };
                onDataReceived?.Invoke(cctvData);
            }

            yield return wait;
        }
    }
}
