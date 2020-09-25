using UnityEngine;
using System.Collections;
using Prime31;
using Cinemachine;


public class PlayerMovement : MonoBehaviour
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
	public float glideFallSpeed = -2f;
	public float wallSlideFallSpeed = -2f;

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

	[HideInInspector]
	public bool gliding = false;
	public bool underground = false;
	public bool drilling = false;
	public bool attacking = false;
	public int wallSliding = 0;

	// public bool attacking = false;

	private bool canDig = true;
	private bool drillOnCooldown = false;
	private bool wallSlideTimeout = false;
	private bool exitingWallSlide = false;

	private float playerControlX = 1f;


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

		if( Input.GetKey( KeyCode.RightArrow ) )
		{
			normalizedHorizontalSpeed = 1;
			if( transform.localScale.x < 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
		}
		else if( Input.GetKey( KeyCode.LeftArrow ) )
		{
			normalizedHorizontalSpeed = -1;
			if( transform.localScale.x > 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
		}
		else
		{
			if (drilling) {
				normalizedHorizontalSpeed = _velocity.x < 0 ? -1 : 1;

			} else {
				normalizedHorizontalSpeed = 0;
			}

		}

		if ( !_controller.isGrounded ) {
			CheckForWall();

			drilling = false;
			underground = false;

			if (wallSliding == 0) {
				// Start Gliding
				if ( Input.GetKey( KeyCode.UpArrow ) && _velocity.y < 0 ) {
					gliding = true;
				}

				// Stop Gliding
				if ( Input.GetKeyUp( KeyCode.UpArrow ) ) {
					gliding = false;
				}

				// Fall down faster to start drilling.
				if( Input.GetKey( KeyCode.DownArrow ) && !gliding && !drillOnCooldown ) {
					gravityDigMultiplier = 2f;
					drilling = true;
					_controller.ignoreOneWayPlatformsThisFrame = true;
				}
			} else {
				gliding = false;

				// Wall Jump
				if( Input.GetKeyDown( KeyCode.UpArrow ) )
				{
					_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
					_velocity.x = wallSliding * jumpHeight * 3f;
					StartCoroutine(WallSlideUnlockTimer(0.2f));
					StartCoroutine(RegainXControl(0.3f));
				}
			}

		}


		if( _controller.isGrounded ) {

			_velocity.y = 0;
			gliding = false;
			wallSliding = 0;

			// Jump
			if( Input.GetKeyDown( KeyCode.UpArrow ) && !drilling )
			{
				_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
			}

			// Dig
			if( Input.GetKey( KeyCode.DownArrow ) && !drillOnCooldown ) {
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
			if( Input.GetKeyUp( KeyCode.DownArrow ) && drilling ) {
				_velocity.y = Mathf.Sqrt( 2f * emergeHeight * -gravity );
				drilling = false;
				underground = false;
				Instantiate(emergePS, transform.position - Vector3.up, transform.rotation);
				GetComponent<CinemachineImpulseSource>().GenerateImpulse();
				GetComponent<Player>().Attack();
			}
		}


		if (gliding) {
			// Fall slowly if Gliding
			if (_velocity.y < glideFallSpeed) {
				_velocity.y = glideFallSpeed;
			} else {
				_velocity.y += gravity * gravityDigMultiplier * Time.deltaTime;
			}
		} else if (wallSliding != 0) {
			// Fall slowly if Wall Sliding
			if (_velocity.y < wallSlideFallSpeed) {
				_velocity.y = wallSlideFallSpeed;
			} else {
				_velocity.y += gravity * gravityDigMultiplier * Time.deltaTime;
			}
		} else {
			// Apply Gravity
			_velocity.y += gravity * gravityDigMultiplier * Time.deltaTime;
		}

		if (wallSliding == 0) {
			// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
			var smoothedMovementFactor = _controller.isGrounded ? drilling ? undergroundDamping : groundDamping : inAirDamping;
			float speed = _controller.isGrounded && drilling ? drillingSpeed : runSpeed;
			_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * speed, Time.deltaTime * smoothedMovementFactor * playerControlX );
		} else {
			// Wall Sliding lock X movement logic
			_velocity.x = -wallSliding * 5f;
			if (normalizedHorizontalSpeed == wallSliding && !exitingWallSlide) {
				StartCoroutine(WallSlideExitTimer(0.3f));
			} else if (normalizedHorizontalSpeed != wallSliding) {
				StopCoroutine("WallSlideExitTimer");
				exitingWallSlide = false;
			}
		}

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
		_animator.SetBool("WallSliding", wallSliding != 0);
	}

	void CheckForWall() {
		if (!wallSlideTimeout) {
			RaycastHit2D raycastL = Physics2D.Raycast(transform.position, Vector2.left, 0.5f, _controller.platformMask);
			RaycastHit2D raycastR = Physics2D.Raycast(transform.position, Vector2.right, 0.5f, _controller.platformMask);
			Debug.DrawRay(transform.position, Vector2.left * 0.5f);
			Debug.DrawRay(transform.position, Vector2.right * 0.5f);
			if (raycastL || raycastR) {
				if (raycastL && raycastL.normal.x == 1) {
					wallSliding = (int) raycastL.normal.x;
				}
				if (raycastR && raycastR.normal.x == -1) {
					wallSliding = (int) raycastR.normal.x;
				}
				// Vector3 p = transform.parent.;
				// p.x = wallSliding;
				transform.localScale = new Vector3(wallSliding, 1, 1);
				// transform.parent.Translate()
			} else {
				wallSliding = 0;
			}
		}
	}

	IEnumerator WallSlideUnlockTimer(float waitTime)
    {
        wallSlideTimeout = true;
		wallSliding = 0;
		yield return new WaitForSeconds(waitTime);
		wallSlideTimeout = false;
    }

	IEnumerator WallSlideExitTimer(float waitTime)
    {
		exitingWallSlide = true;
        yield return new WaitForSeconds(waitTime);
        wallSlideTimeout = true;
		wallSliding = 0;
		yield return new WaitForSeconds(waitTime);
		wallSlideTimeout = false;
		exitingWallSlide = false;
    }

	IEnumerator RegainXControl(float seconds)
    {
		playerControlX = 0f;
		float controlRate =  0.01f / seconds;
		while (playerControlX < 1f) {
			yield return new WaitForSeconds(0.01f);
			playerControlX += controlRate;
		}
    }
}

