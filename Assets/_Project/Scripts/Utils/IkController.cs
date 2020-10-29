using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IkController : MonoBehaviour
{

    [HideInInspector]
    public Animator anim;

    [Header("IK")]
    public bool Ik = true;
    [Header("IK Head")]
    public bool LookAt = true;
    public float LookAtSpeed = 2.5f;
    public float LookAtFadeSpeed = 2.5f;
    [Range(0f, 1f)]
    public float LookAtMaxStrength = .5f;
    public Vector3 LookAtTarget;
    Vector3 currentLookAt = Vector3.zero;
    public float currentLookAtStrength;

    [Header("IK RightHand")]
    public bool RightHand = true;
    public float RightHandSpeed = 50;
    public float RightHandFadeSpeed = 2.5f;
    [Range(0f, 1f)]
    public float RightHandMaxStrength = .5f;
    public Transform RightHandTarget;
    Vector3 currentRightHand = Vector3.zero;
    public float currentRightHandStrength;
    [Header("IK LeftHand")]
    public bool LeftHand = true;
    public float LeftHandSpeed = 50;
    public float LeftHandFadeSpeed = 2.5f;
    [Range(0f, 1f)]
    public float LeftHandMaxStrength = .5f;
    public Transform LeftHandTarget;
    Vector3 currentLeftHand = Vector3.zero;
    public float currentLeftHandStrength;

    [Header("IK Foot")]
    public bool Foot = true;
    public bool FixXZ = true;
    public float FootGroundSpeed = 20;
    public float FootAirSpeed = 30;
    [Range(0f, 1f)]
    public float FootMaxStrength = 1;
    [Range(0f, 1f)]
    public float RayOffset = 0.3f;
    [Range(0f, 1f)]
    public float RayLength = 0.15f;
    [Tooltip("Offset para ajustar posicion de pie")]
    public Vector3 FinalPosOffset;
    [Tooltip("Capa de los objetos donde se puede ajustar el pie")]
    public LayerMask RayMask;
    public Vector3 lastRightFootPos;
    public Vector3 lastLeftFootPos;
    public Quaternion lastRightFootRot;
    public Quaternion lastLeftFootRot;
    public RaycastHit hit;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }


    private void OnAnimatorIK()
    {
        if (anim && Ik)
        {
            HeadIK();
            HandsIk();
            FootIK();
        }
    }

    void HeadIK()
    {
        if (Ik)
        {
            if (LookAt)
            {
                currentLookAtStrength = Mathf.Lerp(currentLookAtStrength, LookAtMaxStrength, LookAtFadeSpeed * Time.deltaTime);
                currentLookAt = Vector3.Lerp(currentLookAt, LookAtTarget, LookAtSpeed * Time.deltaTime);
                anim.SetLookAtPosition(currentLookAt);
            }
            else
            {
                currentLookAtStrength = Mathf.Lerp(currentLookAtStrength, 0, LookAtFadeSpeed * Time.deltaTime);
            }
        }
        currentLookAtStrength = Mathf.Clamp(currentLookAtStrength, 0, LookAtMaxStrength);
        anim.SetLookAtWeight(currentLookAtStrength);
    }
    void HandsIk()
    {
        if (Ik)
        {
            if (RightHand && RightHandTarget)
            {
                currentRightHandStrength = Mathf.Lerp(currentRightHandStrength, RightHandMaxStrength, Time.deltaTime);
                currentRightHand = Vector3.Lerp(currentRightHand, RightHandTarget.position, RightHandSpeed * Time.deltaTime);
                anim.SetIKPosition(AvatarIKGoal.RightHand, currentRightHand);
                anim.SetIKRotation(AvatarIKGoal.RightHand, RightHandTarget.rotation);
            }
            else
            {
                anim.SetIKPosition(AvatarIKGoal.RightHand, currentRightHand);
                currentRightHandStrength = Mathf.Lerp(currentRightHandStrength, 0, RightHandFadeSpeed * Time.deltaTime);
            }

            if (LeftHand && LeftHandTarget)
            {
                currentLeftHandStrength = Mathf.Lerp(currentLeftHandStrength, LeftHandMaxStrength, LeftHandFadeSpeed * Time.deltaTime);
                currentLeftHand = Vector3.Lerp(currentLeftHand, LeftHandTarget.position, LeftHandSpeed * Time.deltaTime);
                anim.SetIKPosition(AvatarIKGoal.LeftHand, currentLeftHand);
                anim.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandTarget.rotation);
            }
            else
            {
                anim.SetIKPosition(AvatarIKGoal.LeftHand, currentLeftHand);
                currentLeftHandStrength = Mathf.Lerp(currentLeftHandStrength, 0, LeftHandFadeSpeed * Time.deltaTime);
            }
        }
        currentRightHandStrength = Mathf.Clamp(currentRightHandStrength, 0, RightHandMaxStrength);
        anim.SetIKPositionWeight(AvatarIKGoal.RightHand, currentRightHandStrength);
        anim.SetIKRotationWeight(AvatarIKGoal.RightHand, currentRightHandStrength);

        currentLeftHandStrength = Mathf.Clamp(currentLeftHandStrength, 0, LeftHandMaxStrength);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, currentLeftHandStrength);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, currentLeftHandStrength);
    }

    void FootIK()
    {

        if (Foot)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            Vector3 FootPos = anim.GetIKPosition(AvatarIKGoal.RightFoot);
            Quaternion FootRot = anim.GetIKRotation(AvatarIKGoal.RightFoot);

            if (Physics.Raycast(FootPos + Vector3.up * RayOffset, Vector3.down * (RayOffset + RayLength), out hit, (RayOffset + RayLength), RayMask, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(FootPos + Vector3.up * RayOffset, Vector3.down * (RayOffset + RayLength), Color.green);
                lastRightFootPos = Vector3.Lerp(lastRightFootPos, hit.point + FinalPosOffset, FootGroundSpeed * Time.deltaTime);
                if (FixXZ)
                {
                    lastRightFootPos.x = (hit.point + FinalPosOffset).x;
                    lastRightFootPos.z = (hit.point + FinalPosOffset).z;
                }
                lastRightFootRot = Quaternion.Lerp(lastRightFootRot, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal), FootGroundSpeed * Time.deltaTime);
            }
            else
            {
                Debug.DrawRay(FootPos + Vector3.up * RayOffset, Vector3.down * (RayOffset + RayLength), Color.red);
                lastRightFootPos = Vector3.Lerp(lastRightFootPos, FootPos, FootAirSpeed * Time.deltaTime);
                lastRightFootRot = Quaternion.Lerp(lastRightFootRot, FootRot, FootAirSpeed * Time.deltaTime);
            }
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, FootMaxStrength);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, FootMaxStrength);
            anim.SetIKPosition(AvatarIKGoal.RightFoot, lastRightFootPos);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, lastRightFootRot);

            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
            FootPos = anim.GetIKPosition(AvatarIKGoal.LeftFoot);
            FootRot = anim.GetIKRotation(AvatarIKGoal.LeftFoot);

            if (Physics.Raycast(FootPos + Vector3.up * RayOffset, Vector3.down * (RayOffset + RayLength), out hit, (RayOffset + RayLength), RayMask, QueryTriggerInteraction.Ignore))
            {
                Debug.DrawRay(FootPos + Vector3.up * RayOffset, Vector3.down * (RayOffset + RayLength), Color.green);
                lastLeftFootPos = Vector3.Lerp(lastLeftFootPos, hit.point + FinalPosOffset, FootGroundSpeed * Time.deltaTime);
                if (FixXZ)
                {
                    lastLeftFootPos.x = (hit.point + FinalPosOffset).x;
                    lastLeftFootPos.z = (hit.point + FinalPosOffset).z;
                }
                lastLeftFootRot = Quaternion.Lerp(lastLeftFootRot, Quaternion.LookRotation(Vector3.ProjectOnPlane(transform.forward, hit.normal), hit.normal), FootGroundSpeed * Time.deltaTime);
            }
            else
            {
                Debug.DrawRay(FootPos + Vector3.up * RayOffset, Vector3.down * (RayOffset + RayLength), Color.red);
                lastLeftFootPos = Vector3.Lerp(lastLeftFootPos, FootPos, FootAirSpeed * Time.deltaTime);
                lastLeftFootRot = Quaternion.Lerp(lastLeftFootRot, FootRot, FootAirSpeed * Time.deltaTime);
            }
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, FootMaxStrength);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, FootMaxStrength);
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, lastLeftFootPos);
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, lastLeftFootRot);


        }
        else //IK Apagado, no hacemos nada
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0f);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0f);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
        }
    }
}
