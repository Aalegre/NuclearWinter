using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    public PlayerController pc;
    public PlayerCharacterController pcc;
    public AudioListener audioListener;
    public Vector3 audioListenerOffset;
    public ReflectionProbe reflections;
    public Vector3 reflectionsOffset;
    public float PosSpeed = 7.5f;
    public Vector3 gameplayOffset;
    public Vector3 gameplayDirection;
    public Vector3 gameplayDirectionUp;
    public Vector3 gameplayDirectionDown;
    public Vector3 gameplayDirectionLeft;
    public Vector3 gameplayDirectionRight;
    public Camera cam;
    public float moveSpeed = 1;
    public Vector2Int resolution;
    public int resolutionMin;
    Vector2 moveAxis;
    private void Awake()
    {
        if (!cam)
            cam = GetComponent<Camera>();
        GameManager.Instance.cameraController = this;
        UpdateResolution();
        cam.transform.position = pcc.transform.position + gameplayOffset;
        cam.transform.localRotation = Quaternion.Euler(gameplayDirection);
    }
    private void Update()
    {
        reflections.transform.position = cam.transform.position + reflectionsOffset;
        audioListener.transform.position = pcc.transform.position + audioListenerOffset;
    }

    void LateUpdate()
    {
        moveAxis = Vector2.Lerp(moveAxis, pc.MoveAxis, Time.deltaTime * moveSpeed);
        Vector3 desiredDirection = gameplayDirection;
        desiredDirection = Vector3.Lerp(desiredDirection, gameplayDirectionUp, moveAxis.y);
        desiredDirection = Vector3.Lerp(desiredDirection, gameplayDirectionDown, -moveAxis.y);
        desiredDirection = Vector3.Lerp(desiredDirection, gameplayDirectionRight, moveAxis.x);
        desiredDirection = Vector3.Lerp(desiredDirection, gameplayDirectionLeft, -moveAxis.x);
        cam.transform.position = Vector3.Lerp(cam.transform.position, pcc.transform.position + gameplayOffset, Time.deltaTime * PosSpeed);
        cam.transform.localRotation = Quaternion.Euler(desiredDirection);
    }

    public void UpdateResolution()
    {
        resolution.x = cam.pixelWidth;
        resolution.y = cam.pixelHeight;
        resolutionMin = Mathf.Min(resolution.x, resolution.y);
        GameManager.Instance.resolution = resolution;
        GameManager.Instance.resolutionF = resolution;
        GameManager.Instance.resolutionHalf = resolution / 2;
        GameManager.Instance.resolutionHalfF = resolution / 2;
        GameManager.Instance.resolutionMin = resolutionMin;
        GameManager.Instance.resolutionMinF = resolutionMin;
    }
}
