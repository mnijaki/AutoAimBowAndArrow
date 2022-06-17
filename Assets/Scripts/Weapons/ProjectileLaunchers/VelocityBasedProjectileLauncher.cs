using System;
using UnityEngine;

public class VelocityBasedProjectileLauncher : MonoBehaviour
{
	[SerializeField]
	private GameObject _arrowPrefab;
	[SerializeField]
	[Tooltip("Flag if arrow arc height should be flattened based on the distance to the target")]
	private bool _shouldFlattenArcHeight;
	[SerializeField]
	private bool _shouldDrawPredictedPath;
	
	private Vector3 _gravity;
	private ArrowPositionPredicter _arrowPositionPredicter;
	
	private void Awake()
	{
		_gravity = Physics.gravity;
		_arrowPositionPredicter = GetComponent<ArrowPositionPredicter>();
	}

	public void Shoot(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		ShootOnFlatSurfaceUsingDirection(velocityBasedWeaponType, firingPointPos, targetPos);
	}
	
	public void ShootOnFlatSurfaceUsingVectorForward(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// ************************************************************************************************************************************
		// This method works only if target and shooter are on the same level (both have equal 'position.y' value).
		// ************************************************************************************************************************************
		
		// Compute base data.
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(targetPos,firingPointPos);
		
		// Compute angle of the throw (we assume shooter and target are on the same surface level).
		var sin2Angle = ((distance * _gravity.y) / (initialVelocity * initialVelocity));
		if(sin2Angle is < -1 or > 1)
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		float angle = Mathf.Asin(sin2Angle)  / 2 * Mathf.Rad2Deg;
		
		// Rotate velocityVector upwards along right axis.
		Vector3 velocityVector = Quaternion.AngleAxis(angle, Vector3.right) * (Vector3.forward * initialVelocity);
		// Rotate velocityVector towards target direction.
		Vector3 directionToTarget = (targetPos - firingPointPos).normalized;
		velocityVector = Quaternion.LookRotation(directionToTarget) * velocityVector;

		// Launch projectile.
		LaunchProjectile(firingPointPos, velocityVector);
	}

	public void ShootOnFlatSurfaceUsingDirection(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// ************************************************************************************************************************************
		// This method works only if target and shooter are on the same level (both have equal 'position.y' value).
		// ************************************************************************************************************************************
		
		// Compute base data.
		float distance = Vector3.Distance(targetPos,firingPointPos);
		Vector3 directionToTarget = (targetPos - firingPointPos).normalized;
		Vector3 velocityVector = directionToTarget * velocityBasedWeaponType.InitialVelocity;
		
		// Compute angle of the throw (we assume shooter and target are on the same surface level).
		var sin2Angle = ((distance * _gravity.y) / (velocityVector.magnitude * velocityVector.magnitude));
		if(sin2Angle is < -1 or > 1)
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		float angle = Mathf.Asin(sin2Angle)  / 2 * Mathf.Rad2Deg;
		
		// Get vector that is perpendicular to velocityVector and Vector3.up (we will rotate velocityVector around this vector).
		Vector3 axis = Vector3.Cross(velocityVector, Vector3.up);
		// handle case where vectorToRotate is collinear with up.
		if (axis == Vector3.zero) axis = Vector3.right;
		// Rotate velocityVector upwards along right axis (right axis is relative to velocityVector not world space).
		velocityVector = Quaternion.AngleAxis(-angle, axis) * velocityVector;

		// Launch projectile.
		LaunchProjectile(firingPointPos, velocityVector);
	}
	
	private void LaunchProjectile(Vector3 firingPointPos, Vector3 velocityVector)
	{
		// Calculate flight time.
		float timeToReachTarget = Mathf.Abs(2 * (velocityVector.y / _gravity.y));
		
		ArrowLaunchData arrowLaunchData = new(firingPointPos, velocityVector, timeToReachTarget , _gravity);
		
		GameObject projectile = Instantiate(_arrowPrefab, arrowLaunchData.LaunchPosition, Quaternion.identity);
		projectile.GetComponent<ArrowMover>().Launch(arrowLaunchData);

		if(_shouldDrawPredictedPath)
			_arrowPositionPredicter.DrawPredictedArrowPositions(arrowLaunchData);
	}

	private float GetUpAngleTowardTarget(Vector3 firingPointPos, Vector3 targetPos)
	{
		float angleTowardsTarget = Mathf.Atan2((targetPos - firingPointPos).normalized.y, (targetPos - firingPointPos).normalized.z) 
		                           * Mathf.Rad2Deg;

		return angleTowardsTarget;
	}
}
