using UnityEngine;

public class SpawnPrefabOnButtonPress : MonoBehaviour
{
    public GameObject prefabToSpawn; // 要生成的 prefab
    public float spawnDistance = 300f; // 生成位置的距离
    private Transform cameraTransform; // 相机的 Transform

    void Start()
    {
        // 获取 OVRPlayerController 中的相机 Transform
        cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // 检查是否按下了 B 键
        if (OVRInput.GetDown(OVRInput.Button.Two, OVRInput.Controller.RTouch))
        {
            SpawnPrefab();
        }
    }

    void SpawnPrefab()
    {
        if (cameraTransform == null)
        {
            Debug.LogWarning("Camera transform is not assigned.");
            return;
        }

        // 获取相机的位置和朝向
        Vector3 cameraPosition = cameraTransform.position;
        Vector3 cameraForward = cameraTransform.forward;

        // 计算生成位置
        Vector3 spawnPosition = cameraPosition + (cameraForward * spawnDistance);

        // 在计算的生成位置生成 prefab
        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
    }
}
