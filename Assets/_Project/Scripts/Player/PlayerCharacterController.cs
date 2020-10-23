using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacterController : ThirdPersonCharacter
{
	public PlayerController playerController;

	public Transform root;
	public Transform LeftHand;
	public Transform RightHand;
	public Transform RightHandTorch;
	public Transform GunTorch;

	public Vector3 HandTargetDirection;
	public Vector3 HandTargetPosition;
	public float HandSpeed = 3;
	public float HandAim = 0;

	public bool Torch;
	bool TorchOld;
	[HideInInspector] public bool ShowTorch;
	bool ShowTorchOld;
	public GameObject TorchHolder;
	public GameObject TorchProp;
	public bool Gun;
	bool GunOld;
	[HideInInspector] public bool ShowGun;
	bool ShowGunOld;
	public GameObject GunHolder;
	public GameObject GunProp;

	private void Awake()
    {
		GameManager.Instance.playerCharacterController = this;
    }

    void Start()
	{
		m_Animator = GetComponent<Animator>();
		m_Rigidbody = GetComponent<Rigidbody>();
		m_Capsule = GetComponent<CapsuleCollider>();
		m_CapsuleHeight = m_Capsule.height;
		m_CapsuleCenter = m_Capsule.center;

		m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
		m_OrigGroundCheckDistance = m_GroundCheckDistance;
		TorchHolder.SetActive(ShowTorch);
		TorchProp.SetActive(!ShowTorch);
		GunHolder.SetActive(ShowGun);
		GunProp.SetActive(!ShowGun);
	}


	public void Move(Vector3 move, bool crouch, bool jump)
	{
		base.Move(move, crouch, jump);

		//if (move.magnitude > 1f) move.Normalize();
		//move = transform.InverseTransformDirection(move);
		//CheckGroundStatus();
		//move = Vector3.ProjectOnPlane(move, m_GroundNormal);
		//m_TurnAmount = Mathf.Atan2(move.x, move.z);
		//m_ForwardAmount = move.z;

		//ApplyExtraTurnRotation();

		//if (m_IsGrounded)
		//{
		//	HandleGroundedMovement(crouch, jump);
		//}
		//else
		//{
		//	HandleAirborneMovement();
		//}

		//ScaleCapsuleForCrouching(crouch);
		//PreventStandingInLowHeadroom();

		//UpdateAnimator(move);
	}
	protected override void UpdateAnimator(Vector3 move)
	{
		base.UpdateAnimator(move);
		if (Torch != TorchOld)
		{
			TorchOld = Torch;
			m_Animator.SetBool("Torch", Torch);
		}
		if (ShowTorch != ShowTorchOld)
		{
			ShowTorchOld = ShowTorch;
			TorchHolder.SetActive(ShowTorch);
			TorchProp.SetActive(!ShowTorch);
			TorchHolder.transform.parent = RightHandTorch;
			TorchHolder.transform.localPosition = Vector3.zero;
			TorchHolder.transform.localRotation = Quaternion.identity;
		}

		if (Gun != GunOld)
		{
			GunOld = Gun;
			m_Animator.SetBool("Gun", Gun);
		}
		if (ShowGun != ShowGunOld)
		{
			ShowGunOld = ShowGun;
			GunHolder.SetActive(ShowGun);
			GunProp.SetActive(!ShowGun);
		}


		HandTargetDirection = Vector3.Lerp(HandTargetDirection, playerController.AimDirection, Mathf.Clamp(HandSpeed * Time.deltaTime, 0, 1));
		HandTargetPosition = root.position + HandTargetDirection;
		//Debug.Log(tempHand.normalized);
		HandAim = Mathf.Lerp(HandAim, -root.transform.TransformDirection(-playerController.AimDirection.x, 0, playerController.AimDirection.z).normalized.x, Mathf.Clamp(HandSpeed * Time.deltaTime, 0, 1));

		m_Animator.SetFloat("Aim", HandAim);

	}

	public void ToggleShowTorch()
    {
		Debug.Log("Toggled torch");
		ShowTorch = Torch;
    }

}
