using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CctvData : DeviceDataBase
{
    public string StreamUrl;
    public int ResolutionW;
    public int ResolutionH;
}

public class UIPopupCCTV : PopupBase<CctvData>
{
    [SerializeField] private Image _imgCctv;
    protected override void OnSetupSpecificUI(CctvData data)
    {
        Debug.Log($"Connecting to CCTV Stream: {data.StreamUrl}");
    }
}
