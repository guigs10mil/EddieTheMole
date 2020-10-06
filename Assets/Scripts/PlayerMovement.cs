using UnityEngine;
using System.Collections;
using Prime31;
using Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


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
	public float bounceOffEnemyHeight = 6f;

	public GameObject emergePS;
	public GameObject submergePS;

	public GameObject pauseCanvas;

	[HideInInspector]
	private float normalizedHorizontalSpeed = 0;
	private float gravityDigMultiplier = 1f;

	private CharacterController2D _controller;
	private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	private SpriteRenderer spriteRenderer;
	private Transform sprite;
	private AudioManager audioManager;

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
	private bool stuned = false;
	private bool paused = false;

	private float playerControlX = 1f;
	private Coroutine exitWall;


	void Awake()
	{
		_animator = GetComponentInChildren<Animator>();
		_controller = GetComponent<CharacterController2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		sprite = transform.GetChild(0);
		audioManager = FindObjectOfType<AudioManager>();

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}

	private void Start() {
		StartCoroutine("StartSequence");
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f ) {
			canDig = hit.transform.tag != "Undiggable";
			if (hit.transform.tag == "ExitHole" && drilling) {
				hit.transform.GetComponent<ExitHole>().EnterHole();
				audioManager.Play("Dig");
				Instantiate(submergePS, transform.position - Vector3.up, transform.rotation);
				Destroy(gameObject);
			}

			if (hit.transform.tag == "LevelSelectHole" && drilling) {
				hit.transform.GetComponent<LevelSelectHole>().EnterHole();
				audioManager.Play("Dig");
				Instantiate(submergePS, transform.position - Vector3.up, transform.rotation);
				Destroy(gameObject);
			}

			if (hit.transform.tag == "LevelEndHole" && drilling) {
				hit.transform.GetComponent<LevelEndHole>().EnterHole();
				audioManager.Play("Dig");
				Instantiate(submergePS, transform.position - Vector3.up, transform.rotation);
				Destroy(gameObject);
			}

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
		var keyboard = Keyboard.current;
		var gamepad = Gamepad.current;
        if (gamepad == null)
			print("No gamepad connected.");
            // return; // No gamepad connected.

		if (keyboard.pKey.wasPressedThisFrame || gamepad != null && gamepad.startButton.wasPressedThisFrame)
			Pause();

		gravityDigMultiplier = 1f;

		// if( Input.GetKey( KeyCode.RightArrow ) )
		if (keyboard.rightArrowKey.isPressed || gamepad != null && (gamepad.dpad.right.isPressed || gamepad.leftStick.right.isPressed))
		{
			normalizedHorizontalSpeed = 1;
			if( transform.localScale.x < 0f )
				transform.localScale = new Vector3( -transform.localScale.x, transform.localScale.y, transform.localScale.z );
		}
		// else if( Input.GetKey( KeyCode.LeftArrow ) )
		else if (keyboard.leftArrowKey.isPressed || gamepad != null && (gamepad.dpad.left.isPressed || gamepad.leftStick.left.isPressed))
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
				if ( (keyboard.upArrowKey.isPressed || gamepad != null && gamepad.crossButton.isPressed) && _velocity.y < 0 ) {
					gliding = true;
				}

				// Stop Gliding
				if ( keyboard.upArrowKey.wasReleasedThisFrame || gamepad != null && gamepad.crossButton.wasReleasedThisFrame) {
					gliding = false;
				}

				// Fall down faster to start drilling.
				if( (keyboard.downArrowKey.isPressed || gamepad != null && gamepad.squareButton.isPressed) && !gliding && !drillOnCooldown ) {
					gravityDigMultiplier = 2f;
					drilling = true;
					// _controller.ignoreOneWayPlatformsThisFrame = true;
				}
			} else {
				gliding = false;

				// Wall Jump
				if( keyboard.upArrowKey.wasPressedThisFrame || gamepad != null && gamepad.crossButton.wasPressedThisFrame )
				{
					_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
					_velocity.x = wallSliding * jumpHeight * 3f;
					StartCoroutine(WallSlideJumpUnlock(0.1f));
					StartCoroutine(RegainXControl(0.3f));
				}
			}

		}


		if( _controller.isGrounded ) {
			if (_controller.collisionState.becameGroundedThisFrame && !drilling && !stuned) {
				audioManager.Play("Land");
			}

			_velocity.y = 0;
			gliding = false;
			wallSliding = 0;

			// Jump
			if( (keyboard.upArrowKey.wasPressedThisFrame || gamepad != null && gamepad.crossButton.wasPressedThisFrame) && !drilling )
			{
				_velocity.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
			}

			// Dig
			if( (keyboard.downArrowKey.isPressed || gamepad != null && gamepad.squareButton.isPressed) && !drillOnCooldown ) {
				if (canDig) {
					drilling = true;
					if (!underground) {
						audioManager.Play("Dig");
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
			if( (keyboard.downArrowKey.wasReleasedThisFrame || gamepad != null && gamepad.squareButton.wasReleasedThisFrame) && drilling ) {
				_velocity.y = Mathf.Sqrt( 2f * emergeHeight * -gravity );
				drilling = false;
				underground = false;
				audioManager.Play("Dig");
				Instantiate(emergePS, transform.position - Vector3.up, transform.rotation);
				GetComponent<CinemachineImpulseSource>().GenerateImpulse();
				GetComponent<Player>().Attack();
			}
		}


		if (gliding && !stuned) {
			// Fall slowly if Gliding
			if (_velocity.y < glideFallSpeed) {
				_velocity.y = glideFallSpeed;
			} else {
				_velocity.y += gravity * gravityDigMultiplier * Time.deltaTime;
			}
		} else if (wallSliding != 0 && !stuned) {
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

		if (wallSliding == 0 || stuned) {
			if (stuned) normalizedHorizontalSpeed = 0;

			// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
			var smoothedMovementFactor = _controller.isGrounded ? drilling ? undergroundDamping : groundDamping : inAirDamping;
			float speed = _controller.isGrounded && drilling ? drillingSpeed : runSpeed;
			_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * speed, Time.deltaTime * smoothedMovementFactor * playerControlX );
		} else {
			// Wall Sliding lock X movement logic
			_velocity.x = -wallSliding * 5f;
			if (normalizedHorizontalSpeed == wallSliding && !exitingWallSlide) {
				exitWall = StartCoroutine(WallSlideExitUnlock(0.4f));
			} else if (normalizedHorizontalSpeed != wallSliding && exitingWallSlide) {
				if (exitWall != null)
					StopCoroutine(exitWall);
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
			RaycastHit2D raycastL = Physics2D.Raycast(transform.position, Vector2.left, 0.5f, 1 << 8);
			RaycastHit2D raycastR = Physics2D.Raycast(transform.position, Vector2.right, 0.5f, 1 << 8);
			Debug.DrawRay(transform.position, Vector2.left * 0.5f);
			Debug.DrawRay(transform.position, Vector2.right * 0.5f);
			
			if (raycastL || raycastR) {
				if (wallSliding == 0)
					audioManager.Play("Land");
				if (raycastL && raycastL.normal.x > 0.9f) {
					wallSliding = 1;
				}
				if (raycastR && raycastR.normal.x < -0.9f) {
					wallSliding = -1;
				}
				transform.localScale = new Vector3(wallSliding, 1, 1);
			} else {
				wallSliding = 0;
			}
		}
	}

	IEnumerator WallSlideJumpUnlock(float waitTime)
    {
        wallSlideTimeout = true;
		wallSliding = 0;
		if (exitWall != null)
			StopCoroutine(exitWall);
		yield return new WaitForSeconds(waitTime);
		wallSlideTimeout = false;
    }

	IEnumerator WallSlideExitUnlock(float waitTime)
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

	IEnumerator StunTimer(float seconds)
    {
		stuned = true;
		StartCoroutine(RegainXControl(seconds / 2));
		yield return new WaitForSeconds(seconds);
		stuned = false;
    }

	public void BounceOffEnemy() {
		_velocity.y = Mathf.Sqrt( 2f * bounceOffEnemyHeight * -gravity );
	}

	public void DamageKnokback(Vector3 source) {
		float Xdirection = transform.position.x - source.x;
		Xdirection = Mathf.Sign(Xdirection);

		float hitForce = 10f;

		_velocity.y = Mathf.Sqrt( 0.5f * hitForce * -gravity );
		_velocity.x = Xdirection * hitForce * 2f;
		// StartCoroutine(WallSlideJumpUnlock(0.1f));
		GetComponent<CinemachineImpulseSource>().GenerateImpulse();
		StartCoroutine(StunTimer(0.5f));
	}

	public void DamageKnokbackFromHazard() {
		float hitForce = 20f;
		_velocity.y = Mathf.Sqrt( 0.5f * hitForce * -gravity );
		// StartCoroutine(WallSlideJumpUnlock(0.1f));
		GetComponent<CinemachineImpulseSource>().GenerateImpulse();
		StartCoroutine(StunTimer(0.2f));
	}

	IEnumerator StartSequence()
    {
		stuned = true;
		sprite.gameObject.SetActive(false);
		yield return new WaitForSeconds(1f);
		sprite.gameObject.SetActive(true);
		stuned = false;
		_velocity.y = Mathf.Sqrt( 2f * emergeHeight * -gravity );
		drilling = false;
		underground = false;
		audioManager.Play("Dig");
		Instantiate(emergePS, transform.position - Vector3.up, transform.rotation);
		GetComponent<CinemachineImpulseSource>().GenerateImpulse();
		_controller.move( _velocity * Time.deltaTime );
    }

	public void Pause() {
		if (pauseCanvas != null) {
			if (paused) {
				audioManager.ResumeMusic();
				Time.timeScale = 1f;
				paused = false;
				pauseCanvas.SetActive(false);
				EventSystem.current.SetSelectedGameObject(null);
			}
			else {
				audioManager.PauseMusic();
				Time.timeScale = 0f;
				paused = true;
				pauseCanvas.SetActive(true);
				EventSystem.current.SetSelectedGameObject(pauseCanvas.transform.GetChild(0).GetChild(0).GetChild(0).gameObject);
			}
		}
	}
}

