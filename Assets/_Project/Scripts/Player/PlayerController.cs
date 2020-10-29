using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CameraController cameraController;
    public bool useCamera = true;
    public PlayerCharacterController playerCharacterController;
    public AtmosphericsController atmosphericsController;
    public float walkSpeed = 2;
    public float runSpeed = .5f;
    public Vector3 AimDirection;
    public Vector3 AimPosition;
    public float AimRadius = 10;
    public float AimHeight = 1.5f;
    [Range(0,.5f)]
    public float AimMinRadius = .1f;
    public Vector2 MoveAxis;
    [Header("Stats")]
    [Range(0, 1)] public float Life = 1;
    [Range(1, 10)] public ushort LifeDivisions = 3;
    public float Temperature = 10;
    [Range(0, 10)] public float TemperatureRise = 4;
    [Range(0, 10)] public float TemperatureFall = 1;
    public float TemperatureMaxBase = 36;
    public float TemperatureMinBase = -5;
    private void Awake()
    {
        GameManager.Instance.playerController = this;
    }

    // Fixed update is called in sync with physics
    private void Update()
    {
        // read inputs
        if(MoveAxis.magnitude > .5f)
        {
            MoveAxis = Vector2.Lerp(MoveAxis, InputManager.Instance.Move.axis, Time.deltaTime * runSpeed);
        }
        else
        {
            MoveAxis = Vector2.Lerp(MoveAxis, InputManager.Instance.Move.axis, Time.deltaTime * walkSpeed);
        }

        Vector3 move = Vector3.zero;

        // calculate move direction to pass to character
        if (cameraController && useCamera)
        {
            // calculate camera relative direction to move:
            move = MoveAxis.y * Vector3.Scale(cameraController.cam.transform.forward, new Vector3(1, 0, 1)).normalized + MoveAxis.x * cameraController.cam.transform.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            move = MoveAxis.y * Vector3.forward + MoveAxis.x * Vector3.right;
        }

        // pass all parameters to the character control script
        playerCharacterController.Move(move, false, false);
        if (InputManager.Instance.Look.axis.magnitude > AimMinRadius)
        {
            AimDirection = new Vector3(InputManager.Instance.Look.axis.x, AimHeight, InputManager.Instance.Look.axis.y) * AimRadius;
            AimPosition = playerCharacterController.root.position + AimDirection;
        }

        if (InputManager.Instance.Fire.ButtonDown() && !playerCharacterController.Gun)
        {
            playerCharacterController.Gun = true;
        }
        if (InputManager.Instance.Weapon.ButtonDown())
        {
            playerCharacterController.Gun = !playerCharacterController.Gun;
        }
        if (InputManager.Instance.Torch.ButtonDown())
        {
            playerCharacterController.Torch = !playerCharacterController.Torch;
        }

    }
}
