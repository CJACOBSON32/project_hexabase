using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HexabaseMovement : MonoBehaviour
{
    public CharacterController characterController;
    public  Cinemachine.CinemachineFreeLook freeLookCam;

    
    [Min(0f)] [Tooltip("The maximum speed the Hexabase can reach")]
    public float maxSpeed = 5f;
    [Min(0f)] [Tooltip("The amount of time it takes to reach max speed")]
    public float speedUpTime = 0.2f;
    [Min(0f)] [Tooltip("The amount of time it takes to slow from max speed to 0")]
    public float slowDownTime = 0.2f;
    [Min(0f)] [Tooltip("The time it takes to turn in the direction of the camera")]
    public float turnSmoothTime = 0.4f;

    private float turnSmoothVelocity;
    private Vector3 currentVelocity;

    private Vector2 movementInput = new Vector2(0, 0);
    private Vector3 velocity = new Vector3(0f, 0f, 0f);
    private float cameraAngle;

    // Start is called before the first frame update
    void Awake()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        cameraAngle = freeLookCam.m_XAxis.Value;
    }

    // Update is called once per frame
    void Update()
    {
        cameraAngle = freeLookCam.m_XAxis.Value;

        moveCharacter();

        // Smoothly rotate to the camera angle
        float setAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, cameraAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, setAngle, 0f);
    }

    private void moveCharacter()
    {
        if (movementInput.magnitude > 0)
        {
            // Move in the direction of the input with respect to the camera
            Vector3 rotatedVector = Quaternion.AngleAxis(cameraAngle, Vector3.up) * new Vector3(movementInput.x, 0f, movementInput.y);
            velocity = Vector3.SmoothDamp(velocity, rotatedVector * maxSpeed, ref currentVelocity, speedUpTime);
        }
        else
        {
            // Slow down to 0 if no input is given
            velocity = Vector3.SmoothDamp(velocity, Vector3.zero, ref currentVelocity, slowDownTime);
        }

        characterController.SimpleMove(velocity);
    }

    public void OnWalk(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>();
    }
}
