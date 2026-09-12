using System;
using System.Collections;
using UnityEngine;

public class MockTelemetryService : MonoBehaviour
{
    public static MockTelemetryService Instance { get; private set; }

    private Coroutine _telemetryCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartStreaming(Facility facility, Action<DataCommon> onDataReceived)
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

    private IEnumerator CoStreamData(Facility facility, Action<DataCommon> onDataReceived)
    {
        var wait = new WaitForSeconds(1.0f); // 1초마다 갱신

        while (true)
        {
            var data = new DataCommon
            {
                deviceId = facility.facilityId,
                deviceName = facility.facilityName,
                deviceType = (int)facility.facilityType,
                isOnline = true
            };

            switch (facility.facilityType)
            {
                case FacilityType.TempSensor:
                    // 18.0℃ ~ 35.0℃ 범위 난수 (소수점 1자리)
                    data.Temperature = Mathf.Round(UnityEngine.Random.Range(18.0f, 35.0f) * 10f) / 10f;
                    data.Humidity = Mathf.Round(UnityEngine.Random.Range(40.0f, 75.0f) * 10f) / 10f;
                    break;

                case FacilityType.CCTV:
                    data.StreamUrl = "rtsp://192.168.1.100:554/live";
                    data.ResolutionW = 1920;
                    data.ResolutionH = 1080;
                    break;
            }

            onDataReceived?.Invoke(data);
            yield return wait;
        }
    }
}