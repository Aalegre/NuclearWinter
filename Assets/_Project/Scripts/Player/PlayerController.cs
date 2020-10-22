using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public CameraController cameraController;
    public bool useCamera = true;
    public PlayerCharacterController playerCharacterController;
    public float moveSpeed = 2;
    Vector2 moveAxis;
    private void Awake()
    {
        GameManager.Instance.playerController = this;
    }

    // Fixed update is called in sync with physics
    private void Update()
    {
        // read inputs
        moveAxis = Vector2.Lerp(moveAxis, InputManager.Instance.Move.axis, Time.deltaTime * moveSpeed);

        Vector3 move = Vector3.zero;

        // calculate move direction to pass to character
        if (cameraController && useCamera)
        {
            // calculate camera relative direction to move:
            move = moveAxis.y * Vector3.Scale(cameraController.cam.transform.forward, new Vector3(1, 0, 1)).normalized + moveAxis.x * cameraController.cam.transform.right;
        }
        else
        {
            // we use world-relative directions in the case of no main camera
            move = moveAxis.y * Vector3.forward + moveAxis.x * Vector3.right;
        }

        // pass all parameters to the character control script
        playerCharacterController.Move(move, false, false); 
    }
}
