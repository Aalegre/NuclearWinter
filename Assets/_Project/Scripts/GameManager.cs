using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    static public GameManager Instance;
    public PlayerController playerController;
    public PlayerCharacterController playerCharacterController;
    public CameraController cameraController;
    public DeformableTerrainManager deformableTerrainManager;
    public AtmosphericsController atmosphericsController;
    public Vector2Int resolution;
    public Vector2Int resolutionHalf;
    public Vector2 resolutionF;
    public Vector2 resolutionHalfF;
    public int resolutionMin;
    public float resolutionMinF;
    public bool Paused = false;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            if (SceneManager.GetActiveScene().buildIndex < 1)
            {
                Paused = true;
            }
            else
            {
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            Destroy(this);
        }
    }
}
