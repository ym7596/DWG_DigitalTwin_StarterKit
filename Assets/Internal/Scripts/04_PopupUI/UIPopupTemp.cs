using System;
using TMPro;
using UnityEngine;

[Serializable]
public class TempData : DeviceDataBase
{
    public float Temperature;
    public float Humidity;
}

public class UIPopupTemp : PopupBase<TempData>
{
    [SerializeField] private TextMeshProUGUI _txtTemp;
    [SerializeField] private TextMeshProUGUI _txtHumidity;
    protected override void OnSetupSpecificUI(TempData data)
    {
        Debug.Log($"Temperture : {data.Temperature} C, Humidity : {data.Humidity}");
    }
}
