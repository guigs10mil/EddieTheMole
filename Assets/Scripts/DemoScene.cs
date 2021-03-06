﻿using UnityEngine;
using System.Collections;
using Prime31;
using Cinemachine;


public class DemoScene : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float drillingSpeed = 8f;
	public float groundDamping = 20f; // how fast do we change direction? higher means faster
	public float inAirDamping = 5f;
	public float undergroundDamping = 5f;
	public float jumpHeight = 3f;
	public float emergeHeight = 4f;
	public float glideFallSpeed = -20f;

	public GameObject emergePS;
	public GameObject submergePS;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;
	private float gravityDigMultiplier = 1f;

	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	private SpriteRenderer spriteRenderer;
	private Transform sprite;

	private bool drilling = false;
	private bool gliding = false;
	private bool underground = false;

	private bool canDig = true;
	private bool drillOnCooldown = false;


	void Awake()
	{
		_animator = GetComponentInChildren<Animator>();
		_controller = GetComponent<CharacterController2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		sprite = transform.GetChild(0);

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f ) {
			canDig = hit.transform.tag != "Undiggable";
			return;
		}

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		// Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
	}


	void onTriggerExitEvent( Collider2D col )
	{
		// Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{
		gravityDigMultiplier = 1f;

		if( _controller.isGrounded ) {
			_velocity.y = 0;
			gliding = false;
		}

		if( !_controller.isGrounded ) {
			drilling = false;
			underground = false;
		}

		if( Input.GetKey( KeyCode.RightArrow ) )
		{
			normalizedHorizontalSpeed = 1;
			if( transform.localScale.x < 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

			// if( _controller.isGrounded && !drilling )
			// 	_animator.Play( Animator.StringToHash( "Idle" ) );
		}
		else if( Input.GetKey( KeyCode.LeftArrow ) )
		{
			normalizedHorizontalSpeed = -1;
			if( transform.localScale.x > 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );

			// if( _controller.isGrounded && !drilling )
			// 	_animator.Play( Animator.StringToHash( "Idle" ) );
		}
		else
		{
			if (drilling) {
				normalizedHorizontalSpeed = _velocity.x < 0 ? -1 : 1;

			} else {
				normalizedHorizontalSpeed = 0;
				// if( _controller.isGrounded )
				// 	_animator.Play( Animator.StringToHash( "Idle" ) );
			}

		}


		if ( !_controller.isGrounded && Input.GetKey( KeyCode.UpArrow ) && _velocity.y < 0 ) {
			gliding = true;
		}

		if ( !_controller.isGrounded && Input.GetKeyUp( KeyCode.UpArrow ) ) {
			gliding = false;
		}

		// we can only jump whilst grounded
		if( _controller.isGrounded && Input.GetKeyDown( KeyCode.UpArrow ) && !drilling )
		{
			_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
			// _animator.Play( Animator.StringToHash( "Jump" ) );
		}

		// Fall down faster to start drilling.
		if( !_controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) && !gliding && !drillOnCooldown ) {
			gravityDigMultiplier = 2f;
			drilling = true;
			_controller.ignoreOneWayPlatformsThisFrame = true;
			// _animator.Play( Animator.StringToHash( "Drill" ) );
		}


		if (gliding) {
			// _animator.Play( Animator.StringToHash( "Gliding" ) );
			if (_velocity.y < glideFallSpeed) {
				_velocity.y = glideFallSpeed;
			} else {
				_velocity.y += gravity * gravityDigMultiplier * Time.deltaTime;
			}
		} else {
			// apply gravity before moving
			_velocity.y += gravity * gravityDigMultiplier * Time.deltaTime;
		}


		// Dig
		if( _controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) && !drillOnCooldown ) {
			if (canDig) {
				drilling = true;
				if (!underground) {
					underground = true;
					Instantiate(submergePS, transform.position - Vector3.up, transform.rotation);
				}
			} else {
				drilling = false;
				underground = false;
				_velocity.y = Mathf.Sqrt( 2f * -gravity );
				StartCoroutine(DrillCooldownWait(1f));
			}
		}

		// Emerge
		if( _controller.isGrounded && Input.GetKeyUp( KeyCode.DownArrow ) && drilling ) {
			_velocity.y = Mathf.Sqrt( 2f * emergeHeight * -gravity );
			drilling = false;
			underground = false;
			Instantiate(emergePS, transform.position - Vector3.up, transform.rotation);
			GetComponent<CinemachineImpulseSource>().GenerateImpulse();
			GetComponent<Player>().Attack();
		}

		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		var smoothedMovementFactor = _controller.isGrounded ? drilling ? undergroundDamping : groundDamping : inAirDamping; // how fast do we change direction?
		float speed = _controller.isGrounded && drilling ? drillingSpeed : runSpeed;
		_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * speed, Time.deltaTime * smoothedMovementFactor );

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets us jump down through one way platforms
		// if( _controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) )
		// {
		// 	_velocity.y *= 3f;
		// 	_controller.ignoreOneWayPlatformsThisFrame = true;
		// }

		UpdateSpriteOrientation();
		UpdateAnimation();

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}

	private void UpdateSpriteOrientation() {
		if (drilling && !_controller.isGrounded) {
			sprite.rotation = Quaternion.LookRotation(Vector3.forward, _velocity);
		} else {
			sprite.rotation = Quaternion.identity;
		}
	}

	IEnumerator DrillCooldownWait(float waitTime)
    {
		drillOnCooldown = true;
        yield return new WaitForSeconds(waitTime);
        drillOnCooldown = false;
    }

	void UpdateAnimation() {
		_animator.SetBool("Grounded", _controller.isGrounded);
		_animator.SetBool("Drilling", drilling);
		_animator.SetBool("Gliding", gliding);
		_animator.SetBool("Falling", _velocity.y < 0);
		_animator.SetBool("Moving", normalizedHorizontalSpeed != 0);
	}

}

