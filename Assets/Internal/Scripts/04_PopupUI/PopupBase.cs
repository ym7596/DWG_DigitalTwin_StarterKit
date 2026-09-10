using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public abstract class DeviceDataBase
{
    public string DeviceID;
    public string DeviceName;
    public bool IsOnline;
}

public abstract class PopupBase<T> : MonoBehaviour where T : DeviceDataBase
{
    [Header("Common Elements")] 
    [SerializeField] protected TextMeshProUGUI _txtID;
    [SerializeField] protected TextMeshProUGUI _txtName;
    [SerializeField] protected Image _imgOnline;
    [SerializeField] protected Button _btnClose;
    [Header("Status Colors")]
    [SerializeField] private Color onlineColor = Color.green;
    [SerializeField] private Color offlineColor = Color.red;
    
    public T DeviceData { get; private set; }
    
    public FacilityType FacilityType { get; private set; }


    protected virtual void Awake()
    {
        if (_btnClose != null)
        {
            _btnClose.onClick.AddListener(ClosePopup);
        }
    }

    public virtual void OpenPopup(T data)
    {
        DeviceData = data;
        gameObject.SetActive(true);

        UpdateCommonUI();
        OnSetupSpecificUI(data);
    }

    protected virtual void UpdateCommonUI()
    {
        if (DeviceData == null)
            return;
        if(_txtID != null) _txtID.text = DeviceData.DeviceID;
        if(_txtName != null) _txtName.text = DeviceData.DeviceName;
        if(_imgOnline != null) _imgOnline.color = DeviceData.IsOnline ? onlineColor : offlineColor;
    }
    
    protected abstract void OnSetupSpecificUI(T data);

    public virtual void RefreshData(T updatedData)
    {
        DeviceData = updatedData;
        UpdateCommonUI();
    }

    protected virtual void ClosePopup()
    {
        gameObject.SetActive(false);
    }
    
    protected virtual void OnDestroy()
    {
        if (_btnClose != null)
        {
            _btnClose.onClick.RemoveListener(ClosePopup);
        }
    }
}
