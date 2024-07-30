using UnityEngine;

public class RotateCanvasToCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // ��ȡ�����
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // ��Canvas�����泯�����
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
