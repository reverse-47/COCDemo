using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
namespace Spine.Unity.Examples {
	public class CharacterNavController : MonoBehaviour
	{
		private NavMeshAgent navMeshAgent;
		private float lastXPosition;
		private float lastZPosition;
		public enum CharacterState
		{
			None,
			Idle,
			Walk,
			Action,
		}

		[Header("Components")] public CharacterController controller;

		/*[Header("Controls")] public string XAxis = "Horizontal";
		public string YAxis = "Vertical";
		public string JumpButton = "Jump";*/

		[Header("Moving")] public float walkSpeed = 1.5f;
		public float runSpeed = 7f;
		public float gravityScale = 6.6f;

		[Header("Jumping")] public float jumpSpeed = 25;
		public float minimumJumpDuration = 0.5f;
		public float jumpInterruptFactor = 0.5f;
		public float forceCrouchVelocity = 25;
		public float forceCrouchDuration = 0.5f;

		[Header("Animation")] public SkeletonAnimationHandleExample animationHandle;

		// Events
		public event UnityAction OnJump, OnLand, OnHardLand;

		//Vector2 input = default(Vector2);
		Vector3 velocity = default(Vector3);
		float minimumJumpEndTime = 0;
		float forceCrouchEndTime;
		bool wasGrounded = false;

		CharacterState previousState, currentState;

		private void Start()
		{
			// 获取或添加 NavMeshAgent 组件
			navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();

			// 记录初始 X 轴位置
			lastXPosition = transform.position.x;
		}

		void Update()
		{
			float dt = Time.deltaTime;
			bool isGrounded = controller.isGrounded;
			bool landed = !wasGrounded && isGrounded;

			// Dummy input.
			/*input.x = Input.GetAxis(XAxis);
			input.y = Input.GetAxis(YAxis);
			bool inputJumpStop = Input.GetButtonUp(JumpButton);
			bool inputJumpStart = Input.GetButtonDown(JumpButton);*/
			//bool doCrouch = (isGrounded && input.y < -0.5f) || (forceCrouchEndTime > Time.time);
			bool doJumpInterrupt = false;
			bool doJump = false;
			bool hardLand = false;

			/*if (isGrounded)
			{
				if (inputJumpStart)
				{
					doJump = true;
				}
			}
			else
			{
				doJumpInterrupt = inputJumpStop && Time.time < minimumJumpEndTime;
			}*/


			// Dummy physics and controller using UnityEngine.CharacterController.
			Vector3 gravityDeltaVelocity = Physics.gravity * gravityScale * dt;

			if (doJump)
			{
				velocity.y = jumpSpeed;
				minimumJumpEndTime = Time.time + minimumJumpDuration;
			}
			else if (doJumpInterrupt)
			{
				if (velocity.y > 0)
					velocity.y *= jumpInterruptFactor;
			}

			/*velocity.x = 0;
			velocity.z = 0;*/
	
			/*if (input.x != 0)
			{
				velocity.x = Mathf.Abs(input.x) > 0.6f ? runSpeed : walkSpeed;
				velocity.x *= Mathf.Sign(input.x);
			}
			
			if (input.y != 0)
			{
				velocity.z = Mathf.Abs(input.y) > 0.6f ? runSpeed : walkSpeed;
				velocity.z *= Mathf.Sign(input.y);
			}*/
			// 计算 X 轴上的移动方向
			float currentXDirection = transform.position.x - lastXPosition;
			float currentZDirection = transform.position.z - lastXPosition;
			// 更新上一帧的 X 轴位置
			lastXPosition = this.transform.position.x;
			lastZPosition = this.transform.position.z;
			if (!isGrounded)
			{
				if (wasGrounded)
				{
					if (velocity.y < 0)
						velocity.y = 0;
				}
				else
				{
					velocity += gravityDeltaVelocity;
				}
			}

			//controller.Move(velocity * dt);
			wasGrounded = isGrounded;

			// Determine and store character state
			//if (isGrounded)
			//{
			//print(currentXDirection);
			if (currentXDirection != 0 && currentZDirection != 0)
			{
				print("walk");
				currentState = CharacterState.Walk;
				if (currentXDirection < 0)
				{
					animationHandle.SetFlip(1f);
				}
				else
				{
					animationHandle.SetFlip(-1f);
       			}
				// if (currentXDirection > 0)
				// 	animationHandle.SetFlip(currentXDirection); // 2D人物翻转
				//print(currentXDirection);
			}
			else
				//currentState = Mathf.Abs(input.x) > 0.6f ? CharacterState.Run : CharacterState.Walk;
			{
				print("idle");
				currentState = CharacterState.Idle;
				
			}

			//}

			bool stateChanged = previousState != currentState;
			previousState = currentState;

			// Animation
			// Do not modify character parameters or state in this phase. Just read them.
			// Detect changes in state, and communicate with animation handle if it changes.
			if (stateChanged)
				HandleStateChanged();

			
			//animationHandle.SetFlip(currentXDirection); // 2D人物翻转
			

			// Fire events.
			if (doJump)
			{
				OnJump.Invoke();
			}

			if (landed)
			{
				if (hardLand)
				{
					OnHardLand.Invoke();
				}
				else
				{
					//OnLand.Invoke();
				}
			}
		}

		void HandleStateChanged()
		{
			// When the state changes, notify the animation handle of the new state.
			string stateName = null;
			switch (currentState)
			{
				case CharacterState.Idle:
					stateName = "idle";
					break;
				case CharacterState.Walk:
					stateName = "walk";
					break;
				case CharacterState.Action:
					stateName = "action";
					break;
				default:
					break;
			}

			animationHandle.PlayAnimationForState(stateName, 0);
		}

	}

	
}
