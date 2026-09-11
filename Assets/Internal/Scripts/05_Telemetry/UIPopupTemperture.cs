using TMPro;
using UnityEngine;

public class UIPopupTemperture : PopupBase<TempData>
{
    [SerializeField] private TextMeshProUGUI _txtTemp;
    [SerializeField] private TextMeshProUGUI _txtHumidity;
    
    protected override void OnSetupSpecificUI(TempData data)
    {
        Debug.Log($"Temperture : {data.Temperature} C, Humidity : {data.Humidity}");
        _txtTemp.text = $"{data.Temperature} C";
        _txtHumidity.text = $"{data.Humidity} %";
    }
}
