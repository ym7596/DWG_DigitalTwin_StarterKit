using UnityEngine;

public class MainManagerComplete : MonoBehaviour
{
    [SerializeField] private UIManagerComplete _uiManager;
    [SerializeField] private InputManagerComplete _inputManager;

    private void Start()
    {
        
    }

    private void OnEnable()
    {
        _inputManager.OnAction_RaycastHit -= LeftClickAction;
        _inputManager.OnAction_RaycastHit += LeftClickAction;
    }

    private void OnDisable()
    {
        _inputManager.OnAction_RaycastHit -= LeftClickAction;
    }

    private void LeftClickAction(RaycastHit hit)
    {
        var facility = hit.collider.gameObject.GetComponent<Facility>();
        if(facility != null)
            _uiManager.ShowPopup(facility);
    }
}
