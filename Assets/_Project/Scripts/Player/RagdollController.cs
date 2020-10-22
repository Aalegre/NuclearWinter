using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RagdollController : MonoBehaviour
{
    public Animator anim;
    public Rigidbody[] rbs;
    public bool autoSearch = false;
    public Collider[] collDisableOnDeath;

    private void Awake()
    {
        if (!anim)
            anim = GetComponent<Animator>();
        if (autoSearch)
            rbs = GetComponentsInChildren<Rigidbody>();
        foreach (var rb in rbs)
        {
            rb.isKinematic = true;
        }
    }
    public void Kill()
    {
        anim.enabled = false;
        foreach (var rb in rbs)
        {
            Vector3 vel = rb.velocity;
            rb.isKinematic = false;
            rb.AddForce(vel * 2, ForceMode.VelocityChange);
        }
        foreach (var col in collDisableOnDeath)
        {
            col.enabled = false;
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(RagdollController))]
public class RagdollControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        RagdollController myTarget = (RagdollController)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Kill"))
        {
            myTarget.Kill();
        }
    }
}
#endif
