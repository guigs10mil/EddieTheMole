using UnityEngine;
using System.Collections;
using Prime31;
using Cinemachine;
using DG.Tweening;


public class EnemyMovement : MonoBehaviour
{
	// movement config
	public float gravity = -25f;
	public float runSpeed = 8f;
	public float groundDamping = 20f;
	public float inAirDamping = 5f;
	public float jumpHeight = 3f;
	public float normalizedHorizontalSpeed = 1;

	public GameObject emergePS;
	public GameObject submergePS;

	public string deathClipName;


	private CharacterController2D _controller;
	// private Animator _animator;
	private RaycastHit2D _lastControllerColliderHit;
	private Vector3 _velocity;
	private SpriteRenderer spriteRenderer;
	private Transform sprite;
	private Transform edgeDetectorOrigin;
	private AudioManager audioManager;

	private bool dead = false;
	private bool ducked = false;


	void Awake()
	{
		// _animator = GetComponentInChildren<Animator>();
		_controller = GetComponent<CharacterController2D>();
		spriteRenderer = GetComponentInChildren<SpriteRenderer>();
		audioManager = FindObjectOfType<AudioManager>();
		sprite = transform.GetChild(0);
		if (transform.childCount > 1) {
			edgeDetectorOrigin = transform.GetChild(1);
		}

		// listen to some events for illustration purposes
		_controller.onControllerCollidedEvent += onControllerCollider;
		_controller.onTriggerEnterEvent += onTriggerEnterEvent;
		_controller.onTriggerStayEvent += onTriggerStayEvent;
		_controller.onTriggerExitEvent += onTriggerExitEvent;
	}


	#region Event Listeners

	void onControllerCollider( RaycastHit2D hit )
	{
		// bail out on plain old ground hits cause they arent very interesting
		if( hit.normal.y == 1f ) {
			return;
		}

		// logs any collider hits if uncommented. it gets noisy so it is commented out for the demo
		//Debug.Log( "flags: " + _controller.collisionState + ", hit.normal: " + hit.normal );
	}


	void onTriggerEnterEvent( Collider2D col )
	{
		// Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
		if (!dead && !ducked && col.tag == "Player") {
			col.GetComponent<Player>().TakeDamage(gameObject);
		}
	}

	void onTriggerStayEvent( Collider2D col )
	{
		// Debug.Log( "onTriggerEnterEvent: " + col.gameObject.name );
		if (!dead && !ducked && col.tag == "Player") {
			col.GetComponent<Player>().TakeDamage(gameObject);
		}
	}


	void onTriggerExitEvent( Collider2D col )
	{
		// Debug.Log( "onTriggerExitEvent: " + col.gameObject.name );
	}

	#endregion


	// the Update loop contains a very simple example of moving the character around and controlling the animation
	void Update()
	{

		if( _controller.isGrounded ) {
			_velocity.y = 0;
		}


		_velocity.y += gravity * Time.deltaTime;

		if (transform.childCount > 1 && _controller.isGrounded) 
			EdgeDetection();

		CheckForObstacle();

		// apply horizontal speed smoothing it. dont really do this with Lerp. Use SmoothDamp or something that provides more control
		if (!dead) {
			var smoothedMovementFactor = _controller.isGrounded ? groundDamping : inAirDamping; // how fast do we change direction?
			float speed = runSpeed;
			_velocity.x = Mathf.Lerp( _velocity.x, normalizedHorizontalSpeed * speed, Time.deltaTime * smoothedMovementFactor );
		}

		if (ducked) {
			_velocity.x = 0;
		}

		// if holding down bump up our movement amount and turn off one way platform detection for a frame.
		// this lets us jump down through one way platforms
		// if( _controller.isGrounded && Input.GetKey( KeyCode.DownArrow ) )
		// {
		// 	_velocity.y *= 3f;
		// 	_controller.ignoreOneWayPlatformsThisFrame = true;
		// }

		// UpdateSpriteOrientation();
		// UpdateAnimation();

		_controller.move( _velocity * Time.deltaTime );

		// grab our current _velocity to use as a base for all calculations
		_velocity = _controller.velocity;
	}

	// private void UpdateSpriteOrientation() {
	// 	if (!_controller.isGrounded) {
	// 		sprite.rotation = Quaternion.LookRotation(Vector3.forward, _velocity);
	// 	} else {
	// 		sprite.rotation = Quaternion.identity;
	// 	}
	// }

	// void UpdateAnimation() {
	// 	_animator.SetBool("Grounded", _controller.isGrounded);
	// 	_animator.SetBool("Drilling", drilling);
	// 	_animator.SetBool("Gliding", gliding);
	// 	_animator.SetBool("Falling", _velocity.y < 0);
	// 	_animator.SetBool("Moving", normalizedHorizontalSpeed != 0);
	// }

	void CheckForObstacle() {
		RaycastHit2D raycast = Physics2D.Raycast(transform.position, Vector3.right * normalizedHorizontalSpeed, 0.6f, 1 << 8);
		Debug.DrawRay(transform.position, Vector3.right * normalizedHorizontalSpeed * 0.6f, Color.red);
		normalizedHorizontalSpeed =  raycast ? -normalizedHorizontalSpeed : normalizedHorizontalSpeed;
		transform.localScale = new Vector3( normalizedHorizontalSpeed, transform.localScale.y, transform.localScale.z );
	}

	public void DieFromEmerging(Vector3 sourcePosition) {
		dead = true;
		audioManager.Play("Kill");
		GetComponentInChildren<Animator>().Play(deathClipName);
        Vector2 direction = (transform.position - sourcePosition).normalized * 5f;
		sprite.DOLocalRotate(new Vector3(0, 0, -transform.localScale.x * Mathf.Sign(direction.x) * 720), 0.6f, RotateMode.FastBeyond360);
        direction.y = Mathf.Sqrt( 2f * jumpHeight * -gravity );
        _velocity = direction;
		_controller.move( _velocity * Time.deltaTime );
		_velocity = _controller.velocity;
        Destroy(gameObject, 0.6f);
    }

	public void Die() {
		dead = true;
		audioManager.Play("Kill");
        Destroy(gameObject);
    }

	public void Duck() {
		audioManager.Play("Block");
		StartCoroutine("DuckTimer");
	}

	IEnumerator DuckTimer() {
		ducked = true;
		GetComponentInChildren<Animator>().Play("shelledDuck");
		yield return new WaitForSeconds(0.6f);
		GetComponentInChildren<Animator>().Play("shelledWalk");
		ducked = false;
	}

	void EdgeDetection() {
		RaycastHit2D raycast = Physics2D.Raycast(edgeDetectorOrigin.position, Vector2.down, 0.6f, _controller.platformMask);
		Debug.DrawRay(edgeDetectorOrigin.position, Vector2.down * 0.6f);
		normalizedHorizontalSpeed =  raycast ? normalizedHorizontalSpeed : -normalizedHorizontalSpeed;
		transform.localScale = new Vector3( normalizedHorizontalSpeed, transform.localScale.y, transform.localScale.z );
	}
}