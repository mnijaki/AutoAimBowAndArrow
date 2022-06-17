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
		float differenceInHeight = firingPointPos.y - targetPos.y;
		
		if(Mathf.Abs(differenceInHeight) < 0.1F)
		{
			ShootOnFlatSurfaceUsingVectorForward(velocityBasedWeaponType, firingPointPos, targetPos);
		}
		else if(differenceInHeight > 0)
		{
			ShootOnTargetsBelow(velocityBasedWeaponType, firingPointPos, targetPos);
		}
		else
		{
			ShootOnTargetsAbove(velocityBasedWeaponType, firingPointPos, targetPos);
		}
	}
	
	private void ShootOnFlatSurfaceUsingVectorForward(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
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

	private void ShootOnFlatSurfaceUsingDirection(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
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

	private void ShootOnTargetsBelow(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// *************************************************************************************
		// Equation of oblique throw angle (for targets below shooter):
		// https://www.youtube.com/watch?v=bqYtNrhdDAY
		// *************************************************************************************
		// phaseAngle = tan^-1(x/h)
		// 2*launchAngle - phaseAngle = cos^-1 [ ((g*x*x / v*v) - h) / sqrt(h*h + x*x)
		// *************************************************************************************
		// Description of variables from the equation/formula:
		// x - distance of the throw (in our case it is distance to the target).
		// h - difference in vertical positions between target and shooter.
		// v - initial velocity
		// g - gravity
		// launchAngle - angle at which we want to launch arrow so it would reach target
		// ************************************************************************************* 
		// Transformations of above equation to work correctly in unity:
		// 2*launchAngle - phaseAngle = cos^-1 [ ((g*x*x / v*v) - h) / sqrt(h*h + x*x)]          // add phaseAngle 
		// 2*launchAngle = cos^-1 [ ((g*x*x / v*v) - h) / sqrt(h*h + x*x)] + phaseAngle          // divide by 2
		// launchAngle = (cos^-1 [ ((g*x*x / v*v) - h) / sqrt(h*h + x*x)] + phaseAngle) / 2      // change cos^-1 to appropriate method
		// launchAngle = (Mathf.Acos [ (((g*x*x / v*v) - h) / sqrt(h*h + x*x)) * Mathf.Rad2Deg] + phaseAngle) / 2
		// *************************************************************************************
		
		// Compute base data.
		float differenceInHeight = firingPointPos.y - targetPos.y;
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(new Vector3(targetPos.x, 0.0F, targetPos.z), 
		                                  new Vector3(firingPointPos.x, 0.0F, firingPointPos.z));
		
		float angle = 0.0F;
		// Compute phase angle.
		float phaseAngle = Mathf.Atan(distance / differenceInHeight) * Mathf.Rad2Deg;

		// Compute launch angle.
		var x1 = Mathf.Abs(_gravity.y) * Mathf.Pow(distance, 2);
		var x2 = Mathf.Pow(initialVelocity, 2);
		float acosAngle = ((x1 / x2) - differenceInHeight) / Mathf.Sqrt(Mathf.Pow(differenceInHeight, 2) + Mathf.Pow(distance, 2));
		if(acosAngle is < -1 or > 1)
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		acosAngle = Mathf.Acos(acosAngle) * Mathf.Rad2Deg;
		angle = (acosAngle + phaseAngle) / 2;
		
		// Rotate velocityVector upwards along right axis.
		Vector3 velocityVector = Quaternion.AngleAxis(-angle, Vector3.right) * (Vector3.forward * initialVelocity);
		// Rotate velocityVector towards target direction.
		Vector3 directionToTarget = (new Vector3(targetPos.x, 0.0F, targetPos.z) - 
		                             new Vector3(firingPointPos.x, 0.0F, firingPointPos.z)).normalized;
		velocityVector = Quaternion.LookRotation(directionToTarget) * velocityVector;

		// Launch projectile.
		LaunchProjectile(firingPointPos, velocityVector);
	}
	
	private void ShootOnTargetsAbove(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// *************************************************************************************
		// Equation of oblique throw angle (for targets above shooter):
		// https://www.youtube.com/watch?v=krzC92hZ8pA&list=PLX2gX-ftPVXUUlf-9Eo_6ut4kP6wKaSWh&index=8
		// *************************************************************************************
		
		// Compute base data.
		float differenceInHeight = firingPointPos.y - targetPos.y;
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(new Vector3(targetPos.x, 0.0F, targetPos.z), 
		                                  new Vector3(firingPointPos.x, 0.0F, firingPointPos.z));
		
		// Compute equation data.
		float g = Mathf.Abs(_gravity.y);
		float tmp =  (g/2) * MathF.Pow(distance/initialVelocity, 2);
		float tmp2 = Mathf.Abs(differenceInHeight) + tmp;
		
		// Compute final equation data.
		float a = tmp;
		float b = distance;
		float c = tmp2;
		float result_1 = (b + Mathf.Sqrt(Mathf.Pow(b, 2) - 4 * a * c)) / (2 * a);
		float result_2 = (b - Mathf.Sqrt(Mathf.Pow(b, 2) - 4 * a * c)) / (2 * a);
		float angle_1 = Mathf.Atan(result_1) * Mathf.Rad2Deg;
		float angle_2 = Mathf.Atan(result_2) * Mathf.Rad2Deg;

		if(float.IsNaN(result_1) && float.IsNaN(result_2))
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}

		float angle;
		if(!float.IsNaN(result_1) && !float.IsNaN(result_2))
		{
			angle = Mathf.Min(angle_1, angle_2);
		}
		else
		{
			if(float.IsNaN(result_1))
				angle = angle_2;
			else
				angle = angle_1;
		}
		
		// Rotate velocityVector upwards along right axis.
		Vector3 velocityVector = Quaternion.AngleAxis(-angle, Vector3.right) * (Vector3.forward * initialVelocity);
		// Rotate velocityVector towards target direction.
		Vector3 directionToTarget = (new Vector3(targetPos.x, 0.0F, targetPos.z) - 
		                             new Vector3(firingPointPos.x, 0.0F, firingPointPos.z)).normalized;
		velocityVector = Quaternion.LookRotation(directionToTarget) * velocityVector;

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
