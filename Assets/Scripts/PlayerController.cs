using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public CharacterController characterController;
    public InputActionAsset inputActions;
    public float speed = 3f;

    private InputAction moveAction;

    void OnEnable()
    {
        moveAction = inputActions.FindAction("Move");
        moveAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
    }

    void Update()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = new Vector3(moveInput.x, 0, moveInput.y);
        move = transform.TransformDirection(move);
        characterController.Move(move * speed * Time.deltaTime);
    }
}