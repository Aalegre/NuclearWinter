using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Animator))]
public class PlayerCharacterController : ThirdPersonCharacter
{
	public PlayerController playerController;
	public IkController ik;

	public Cloth cape;

	public Transform root;
	public Transform LeftHand;
	public Transform RightHand;
	public Transform RightHandTorch;
	public Transform GunTorch;
	public Transform GunLeftHand;
	public Transform GunRightHand;

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
	public bool TorchLighted = true;
	public Light TorchLight;
	HDAdditionalLightData TorchLightData;
	float TorchIntensityOriginal;
	public TemperatureModifierController TorchTemperature;
	public bool Gun;
	bool GunOld;
	[HideInInspector] public bool ShowGun;
	bool ShowGunOld;
	public GameObject GunHolder;
	public ParticleSystem GunParticles;
	public Transform GunBoquilla;
	public GameObject GunProp;
	public float windStrength = 5;
	public float windFalloff = 2;

	private void Awake()
    {
		GameManager.Instance.playerCharacterController = this;
		if (!ik)
			ik = GetComponent<IkController>();
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
		TorchProp.SetActive(!ShowTorch);
		TorchHolder.SetActive(ShowTorch);
		GunProp.SetActive(!ShowGun);
		GunHolder.SetActive(ShowGun);
		TorchLightData = TorchLight.GetComponent<HDAdditionalLightData>();
		TorchIntensityOriginal = TorchLightData.intensity;
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
			TorchProp.SetActive(!ShowTorch);
			TorchHolder.SetActive(ShowTorch);
			TorchHolder.transform.parent = RightHandTorch;
		}
        if (ShowTorch)
		{
			TorchHolder.transform.localPosition = Vector3.zero;
			TorchHolder.transform.localRotation = Quaternion.identity;
			TorchTemperature.active = TorchLighted;
            if (TorchLighted)
            {
				TorchLightData.intensity += Time.deltaTime * TorchTemperature.TemperatureDecayActive * 100;
				TorchLightData.intensity = Mathf.Clamp(TorchLightData.intensity, 0, TorchIntensityOriginal);
			}
            else
			{
				TorchLightData.intensity -= Time.deltaTime * TorchTemperature.TemperatureDecayNotActive * 100;
				TorchLightData.intensity = Mathf.Clamp(TorchLightData.intensity, 0, TorchIntensityOriginal);
			}
		}
        else
        {

        }

		if (Gun != GunOld)
		{
			GunOld = Gun;
			m_Animator.SetBool("Gun", Gun);
		}
		if (ShowGun != ShowGunOld)
		{
			ShowGunOld = ShowGun;
			GunProp.SetActive(!ShowGun);
			GunHolder.SetActive(ShowGun);
            if (ShowGun)
            {

            }
            else
			{
				GunProp.transform.position = GunHolder.transform.position;
				//GunProp.transform.rotation = GunHolder.transform.rotation;
			}
		}
		HandTargetDirection = Vector3.Lerp(HandTargetDirection, playerController.AimDirection, HandSpeed * Time.deltaTime);
		HandTargetPosition = root.position + HandTargetDirection;
		//Debug.Log(tempHand.normalized);
		Vector3 direction = root.transform.TransformDirection(-playerController.AimDirection.x, 0, playerController.AimDirection.z);
		if (direction.z > 0)
		{
			HandAim = Mathf.Lerp(HandAim, -direction.normalized.x, HandSpeed * Time.deltaTime);
		}
		else
		{
			HandAim = Mathf.Lerp(HandAim, -direction.normalized.x * 1.5f, HandSpeed * Time.deltaTime);
		}
        if (ShowGun)
        {
            float t = Utils.Map(ik.currentLeftHandStrength, 0, ik.LeftHandMaxStrength, -0.1f, 1.1f);
            Quaternion original = GunHolder.transform.rotation;
            GunHolder.transform.localRotation = Quaternion.identity;
            if (t > 0)
            {
                Quaternion identity = GunHolder.transform.rotation;
                if (direction.z > 0)
                    GunHolder.transform.LookAt(playerController.AimPosition);
                Quaternion look = GunHolder.transform.rotation;
                look = Quaternion.Lerp(original, look, Time.deltaTime * HandSpeed);
                GunHolder.transform.rotation = Quaternion.Lerp(identity, look, t);
            }
        }

		cape.externalAcceleration = Vector3.Lerp(cape.externalAcceleration, Vector3.zero, Time.deltaTime * windFalloff);
		cape.randomAcceleration = Vector3.Lerp(cape.randomAcceleration, Vector3.zero, Time.deltaTime * windFalloff);

        m_Animator.SetFloat("Aim", HandAim);

	}

	public void Shoot()
    {
		//Debug.Log("Shoot");
		GunParticles.Emit(50);
		cape.externalAcceleration += GunBoquilla.forward * -windStrength;
		cape.randomAcceleration += GunBoquilla.forward * -windStrength;
		StartCoroutine(InputManager.Instance.Feedback_Coroutine(1, 0, .4f, 1, 0, .5f));
	}

	public void ToggleShowTorch()
    {
		//Debug.Log("Toggled Torch");
		ShowTorch = Torch;
		StartCoroutine(InputManager.Instance.Feedback_Coroutine(.5f, 0, .2f, 0, 0, 0));
	}
	public void ToggleShowGun()
    {
		//Debug.Log("Toggled Gun");
		if (Gun)
			StartCoroutine(InputManager.Instance.Feedback_Coroutine(.5f, .5f, .2f, .5f, 0, 1));
		ShowGun = Gun;
	}
	public void ToggleGunHands()
    {
		//Debug.Log("Toggled Guns");
		if (Gun)
		{
			ik.RightHandTarget = GunRightHand;
			ik.LeftHandTarget = GunLeftHand;
		}
		else
		{
			ik.RightHandTarget = null;
			ik.LeftHandTarget = null;
		}
    }
	public void ToggleGunHandsTorch()
    {
		//Debug.Log("Toggled Guns");
        if (Gun)
		{
            if (Torch)
			{
				if (ShowTorch)
				{
					ik.RightHandTarget = GunRightHand;
                }
                else
				{
					ik.RightHandTarget = null;
				}
			}
            else
			{
				if (ShowTorch)
				{
					//ik.RightHandTarget = null;
					ik.RightHandTarget = GunRightHand;
				}
				else
				{
					//ik.RightHandTarget = null;
					ik.RightHandTarget = GunRightHand;
				}
			}
		}
        else
        {

        }
    }
	public void ChangeTorchPos()
    {
        if (Gun)
		{
			TorchHolder.transform.parent = GunTorch;
			TorchHolder.transform.localPosition = Vector3.zero;
			TorchHolder.transform.localRotation = Quaternion.identity;
			StartCoroutine(InputManager.Instance.Feedback_Coroutine(.5f, 0, .2f, 0, 0, 0));
		}
        else
		{
			TorchHolder.transform.parent = RightHandTorch;
		}

    }

}
