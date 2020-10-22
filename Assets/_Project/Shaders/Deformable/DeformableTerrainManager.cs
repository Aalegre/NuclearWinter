using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableTerrainManager : MonoBehaviour
{
    public List<DeformableTerrainController> terrains;
    public Camera cam;
    private void Awake()
    {
        GameManager.Instance.deformableTerrainManager = this;
        cam = Camera.main;
    }
    private void LateUpdate()
    {
        
    }
}
