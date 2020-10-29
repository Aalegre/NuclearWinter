using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableTerrainManager : MonoBehaviour
{
    public List<DeformableTerrainController> terrains;
    public CameraController cameraController;
    public PlayerCharacterController playerCharacterController;
    [Range(0,5)]
    public int BasePriority = 0;
    [Range(1, 15)]
    public int PriorityMultiplier = 5;
    private void Awake()
    {
        GameManager.Instance.deformableTerrainManager = this;
        playerCharacterController = GameManager.Instance.playerCharacterController;
        cameraController = GameManager.Instance.cameraController;
    }
    private void LateUpdate()
    {
        foreach (var terrain in terrains)
        {
            if (terrain.rend.isVisible)
            {
                terrain.EnableUpdate = true;
                float distance = terrain.RelativeDistance(playerCharacterController.transform);
                if(distance <= 0.5f)
                {
                    terrain.priority = (int)BasePriority;
                }
                else
                {
                    terrain.priority = (int)BasePriority + (int)(distance*PriorityMultiplier);
                }
            }
            else
            {
                terrain.EnableUpdate = false;
            }
        }
    }
}
