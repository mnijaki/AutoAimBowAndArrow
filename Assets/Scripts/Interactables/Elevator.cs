using System;
using System.Collections;
using UnityEngine;

public class Elevator : MonoBehaviour
{
	[SerializeField]
	private Transform _upperTarget;
	[SerializeField]
	private Transform _bottomTarget;
	[SerializeField]
	private float _moveSpeed = 2.0F;
	[SerializeField]
	private AnimationCurve _moveCurve;

	private Rigidbody _rigidbody;
	private Vector3 _startPos;
	private Coroutine _moveUpCoroutine;
	private Coroutine _moveDownCoroutine;
	private readonly WaitForFixedUpdate _waitForFixedUpdate = new();

	private void Awake()
	{
		_rigidbody = GetComponent<Rigidbody>();
	}

	private void OnTriggerEnter(Collider other)
	{
		if(_moveUpCoroutine!= null) StopCoroutine(_moveUpCoroutine);
		if(_moveDownCoroutine!= null) StopCoroutine(_moveDownCoroutine);

		_startPos = transform.position;
		_moveUpCoroutine = StartCoroutine(MoveWithFixedSpeed(_upperTarget.position));
	}

	private void OnTriggerExit(Collider other)
	{
		if(_moveUpCoroutine!= null) StopCoroutine(_moveUpCoroutine);
		if(_moveDownCoroutine!= null) StopCoroutine(_moveDownCoroutine);
            
		_startPos = transform.position;
		_moveDownCoroutine = StartCoroutine(MoveWithFixedSpeed(_bottomTarget.position));
	}

	private IEnumerator MoveWithFixedSpeed(Vector3 targetPos)
	{
		while (true)
		{
			float distance = Vector3.Distance(_startPos, targetPos);
			float remainingDistance = distance;
			while (remainingDistance > 0)
			{
				float t = _moveCurve.Evaluate(1 - (remainingDistance / distance));
				Vector3 lerpedPosition = Vector3.Lerp(_startPos, targetPos, t);
				_rigidbody.MovePosition(lerpedPosition);
				remainingDistance -= _moveSpeed * Time.deltaTime;
				yield return _waitForFixedUpdate;
			}

			_rigidbody.MovePosition(targetPos);
			break;
		}
	}
}