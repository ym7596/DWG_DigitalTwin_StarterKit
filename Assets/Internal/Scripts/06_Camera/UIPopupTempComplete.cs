using TMPro;
using UnityEngine;

public class UIPopupTempComplete : PopupBase<TempData>
{
    [SerializeField] private TextMeshProUGUI _txtTemp;
    [SerializeField] private TextMeshProUGUI _txtHumidity;

    protected override void UpdateCommonUI()
    {
        base.UpdateCommonUI();
        _txtTemp.text = DeviceData.Temperature.ToString("F2") + " C";
        _txtHumidity.text = DeviceData.Humidity.ToString("F2") + " %";
    }

    protected override void OnSetupSpecificUI(TempData data)
    {
        Debug.Log($"Temperture : {data.Temperature} C, Humidity : {data.Humidity}");
        _txtTemp.text = $"{data.Temperature} C";
        _txtHumidity.text = $"{data.Humidity} %";
    }
}
