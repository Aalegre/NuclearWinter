using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float PerlinNoise3d(Vector3 pos)
    {
        return PerlinNoise3d(pos.x, pos.y, pos.z);
    }
    public static float PerlinNoise3d(float x, float y, float z)
    {
        return (Mathf.PerlinNoise(x, y) + Mathf.PerlinNoise(y, z) + Mathf.PerlinNoise(x, z) + Mathf.PerlinNoise(y, x) + Mathf.PerlinNoise(z, y) + Mathf.PerlinNoise(z, x)) / 6f;
    }
    //public static float Map(float val, float valMin, float valMax, float outMin, float outMax)
    //{
    //    return (val - valMin) / (outMin - valMin) * (outMax - valMax) + valMax;
    //    //return outMin + (val - valMin) * (outMax - outMin) / (valMax - valMin);
    //}
    public static float Map(float input, float inputMin, float inputMax, float min, float max)
    {
        return min + (input - inputMin) * (max - min) / (inputMax - inputMin);
    }

    public static Transform FindNearest(in Vector3 pos, in IEnumerable<Transform> objects)
    {
        Transform bestTarget = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (var obj in objects)
        {
            Vector3 directionToTarget = obj.position - pos;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestTarget = obj;
            }
        }
        return bestTarget;
    }
    //public static GameObject FindNearest(in Vector3 pos, in IEnumerable<GameObject> objects)
    //{
    //    GameObject bestTarget = null;
    //    float closestDistanceSqr = Mathf.Infinity;
    //    foreach (var obj in objects)
    //    {
    //        Vector3 directionToTarget = obj.transform.position - pos;
    //        float dSqrToTarget = directionToTarget.sqrMagnitude;
    //        if (dSqrToTarget < closestDistanceSqr)
    //        {
    //            closestDistanceSqr = dSqrToTarget;
    //            bestTarget = obj;
    //        }
    //    }
    //    return bestTarget;
    //}
}
