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
