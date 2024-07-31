using UnityEngine;

public class RotateCanvasToCamera : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        // 获取主相机
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        // 将Canvas的正面朝向相机
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward, mainCamera.transform.rotation * Vector3.up);
    }
}
