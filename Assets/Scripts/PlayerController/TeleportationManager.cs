using UnityEngine;
using UnityEngine.UI;

public class TeleportationManager : MonoBehaviour
{
    public Transform playerTransform;
    public Transform teleportPointA;
    public Transform teleportPointB;
    public Button buttonDown;
    public Button buttonUp;

    void Start()
    {
        buttonDown.onClick.AddListener(TeleportToPointB);
        buttonUp.onClick.AddListener(TeleportToPointA);
    }

    void TeleportToPointB()
    {
        TeleportPlayer(teleportPointB);
    }

    void TeleportToPointA()
    {
        TeleportPlayer(teleportPointA);
    }

    void TeleportPlayer(Transform targetPoint)
    {
        playerTransform.position = targetPoint.position;
        playerTransform.rotation = targetPoint.rotation;
    }
}