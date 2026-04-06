using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchingDirections : MonoBehaviour
{
	public ContactFilter2D castFiler;
	public float groundDistance = 0.05f;
	public float wallDistance = 0.2f;
	public float ceillingDistance = 0.05f;

	CapsuleCollider2D touchingCol;
	Animator animator;

	RaycastHit2D[] groundHits = new RaycastHit2D[5];
	RaycastHit2D[] wallHits = new RaycastHit2D[5];
	RaycastHit2D[] ceillingHits = new RaycastHit2D[5];

	[SerializeField]
	private bool _isGrounded;

	public bool IsGrounded
	{
		get
		{
			return _isGrounded;
		}
		private set
		{
			_isGrounded = value;
			animator.SetBool(AnimationStrings.isGrounded, value);
		}
	}
	[SerializeField]
	private bool _isOnWall;

	public bool IsOnWall
	{
		get
		{
			return _isOnWall;
		}
		private set
		{
			_isOnWall = value;
			animator.SetBool(AnimationStrings.isOnWall, value);
		}
	}
	[SerializeField]
	private bool _isOnCeilling;
	private Vector2 wallCheckDirection => gameObject.transform.localScale.x > 0 ? Vector2.right : Vector2.left;
	public bool IsOnCeilling
	{
		get
		{
			return _isOnCeilling;
		}
		private set
		{
			_isOnCeilling = value;
			animator.SetBool(AnimationStrings.isOnCeilling, value);
		}
	}

	private void Awake()
	{
		touchingCol = GetComponent<CapsuleCollider2D>();
		animator = GetComponent<Animator>();
	}

	private void FixedUpdate()
	{
		IsGrounded = touchingCol.Cast(Vector2.down, castFiler, groundHits, groundDistance) > 0;
		IsOnWall = touchingCol.Cast(wallCheckDirection, castFiler, wallHits, wallDistance) > 0;
		IsOnCeilling = touchingCol.Cast(Vector2.up, castFiler, ceillingHits, ceillingDistance) > 0;
	}
}
