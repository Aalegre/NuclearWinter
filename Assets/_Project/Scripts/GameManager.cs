using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;
    public PlayerController playerController;
    public PlayerCharacterController playerCharacterController;
    public CameraController cameraController;
    public DeformableTerrainManager deformableTerrainManager;
    public Vector2Int resolution;
    public Vector2Int resolutionHalf;
    public Vector2 resolutionF;
    public Vector2 resolutionHalfF;
    public int resolutionMin;
    public float resolutionMinF;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }
}
