using UnityEngine;

public class FaceCamera : MonoBehaviour
{ 
    private Transform mainCameraTransform;

    void Start()
    {
        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            // 카메라와 동일한 회전값을 적용해 텍스트/UI 좌우 반전을 방지
            transform.LookAt(transform.position + (transform.position - mainCameraTransform.position));
        }
    }
}
