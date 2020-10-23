using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class LineRendererHelper : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Transform[] positions;
    int lastPositionsCount = -1;
    void Awake()
    {
        if (!lineRenderer)
            lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(positions.Length != lastPositionsCount)
        {
            lastPositionsCount = positions.Length;
            lineRenderer.positionCount = lastPositionsCount;
        }
        for (int i = 0; i < positions.Length; i++)
        {
            try
            {
                lineRenderer.SetPosition(i, positions[i].position);
            }
            catch { }
        }
    }
}
