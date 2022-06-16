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
	
	Vector3 tmp0 ;
	Vector3 tmp1 ;
	Vector3 tmp2 ;
	Vector3 tmp3 ;

	private void OnDrawGizmos()
	{
		Debug.DrawRay(tmp0, tmp1, Color.red);
		Debug.DrawRay(tmp0, tmp2, Color.magenta);
		Debug.DrawRay(tmp0, tmp3, Color.green);
	}

	public void Shoot(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		Shoot2(velocityBasedWeaponType, firingPointPos, targetPos);
		return;
		// Oblique Throw - Trajectory (polish version)
		// https://www.youtube.com/watch?v=jRfwUncRTyE
		// Oblique throw range/distance equation:
		// dist = [Vo*Vo * sin(2*Angle)] / gravity
		// we multiply by gravity
		// dist * gravity = Vo*Vo * sin(2*Angle)
		// we divide by (Vo*Vo)
		// (dist * gravity) / (Vo*Vo) = sin(2*Angle)
		// we mirror equation
		// sin(2*Angle) = (dist * gravity) / (Vo*Vo)
		// we use arcs sin to get angle form sin value. In Unity arcs sin is [Math.Asin() * Mathf.Rad2Deg]
		// 2*Angle = Math.Asin((dist * gravity) / (Vo*Vo)) * Mathf/Rad2Deg
		// we divide by 2
		// angle  = [Math.Asin((dist * gravity) / (Vo*Vo)) * Mathf.Rad2Deg] / 2;

		var flattenTargetPos = new Vector3(targetPos.x, 0.0F, targetPos.z);
		var flattenTargetPos2 = new Vector3(0, targetPos.y, 0.0F);
		var flattenFiringPos = new Vector3(firingPointPos.x, 0.0F, firingPointPos.z);
		var flattenFiringPos2 = new Vector3(0.0F, firingPointPos.y, 0.0F);
		
		float atanb = Mathf.Atan2((targetPos - firingPointPos).normalized.y, (targetPos - firingPointPos).normalized.z) * Mathf.Rad2Deg;
		
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(targetPos,firingPointPos);
		//float distance = Vector3.Distance(targetPos,firingPointPos) - Vector3.Distance(flattenTargetPos2,flattenFiringPos2);
		//float distance = Vector3.Distance(flattenTargetPos,flattenFiringPos);
		var sin2Angle = ((distance * _gravity.y) / (initialVelocity * initialVelocity));
		float angle = Mathf.Asin( sin2Angle)  / 2 * Mathf.Rad2Deg;
		// -90 and clamp to 90 because of lefthanded coordinate system?

		
		Vector3 directionToTarget = (targetPos - firingPointPos).normalized;
		//Vector3 directionToTarget = (flattenTargetPos - flattenFiringPos).normalized;

		var c = new Vector3(0.0F, (flattenTargetPos2 - flattenFiringPos2).y, 0.0F);
		var cdist = Vector3.Distance(flattenTargetPos2, flattenFiringPos2);
		var csinAngle = (cdist * _gravity.y) / (initialVelocity * initialVelocity);
		float cangle = (Mathf.Asin(csinAngle) * Mathf.Rad2Deg) / 2;
		cangle = Mathf.Atan2((targetPos - firingPointPos).normalized.y, (targetPos - firingPointPos).normalized.z) * Mathf.Rad2Deg -90;
		//cangle = -cangle;
		//cangle = 45 - cangle;
		// cangle = 180 - cangle;
		
		Debug.Log("cangle = "+cangle);
		tmp0 = firingPointPos;
		tmp1 = Quaternion.AngleAxis(angle, Vector3.right) * (Vector3.forward * initialVelocity); // red
		tmp2 = Quaternion.AngleAxis(cangle, Vector3.right) * (Vector3.forward * initialVelocity);; // magenta
		tmp3 = Quaternion.AngleAxis(angle+cangle, Vector3.right) * (Vector3.forward * initialVelocity); // green

		//var sum = Mathf.Clamp(angle + cangle, -45, 45);
		//var sum = Mathf.Clamp(angle, -45, 45);
		
		Vector3 velocityWithAngle = Quaternion.AngleAxis(angle, Vector3.right) * (Vector3.forward * initialVelocity);
		//Vector3 velocityWithAngle = Quaternion.AngleAxis(angle+cangle, Vector3.right) * (Vector3.forward * initialVelocity);
		//velocityWithAngle = Quaternion.AngleAxis(cangle, Vector3.right) * velocityWithAngle;
		velocityWithAngle = Quaternion.LookRotation(directionToTarget) * velocityWithAngle;
		//velocityWithAngle = Quaternion.LookRotation((new Vector3(directionToTarget.x, angle, directionToTarget.z)).normalized) * velocityWithAngle;
		
		Debug.Log("velocity = "+(directionToTarget * initialVelocity));
		Debug.Log("angle = "+angle);
		Debug.Log("velocityWithAngle = "+velocityWithAngle);

		float timeToReachTarget = Mathf.Abs(2 * (velocityWithAngle.y / _gravity.y));
		
		ArrowLaunchData arrowLaunchData = new(firingPointPos, velocityWithAngle, timeToReachTarget , _gravity);
		LaunchProjectile(arrowLaunchData);
	}
	
	public void Shoot2(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(targetPos,firingPointPos);
		var sin2Angle = ((distance * _gravity.y) / (initialVelocity * initialVelocity));
		if(sin2Angle is < -1 or > 1)
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		
		float angle = Mathf.Asin(sin2Angle)  / 2 * Mathf.Rad2Deg;

		Vector3 directionToTarget = (targetPos - firingPointPos).normalized;

		Vector3 velocityWithAngle = Quaternion.AngleAxis(angle, Vector3.right) * (Vector3.forward * initialVelocity);
		velocityWithAngle = Quaternion.LookRotation(directionToTarget) * velocityWithAngle;
		
		float timeToReachTarget = Mathf.Abs(2 * (velocityWithAngle.y / _gravity.y));
		
		ArrowLaunchData arrowLaunchData = new(firingPointPos, velocityWithAngle, timeToReachTarget , _gravity);
		LaunchProjectile(arrowLaunchData);
	}
	
	private void LaunchProjectile(ArrowLaunchData arrowLaunchData)
	{
		GameObject projectile = Instantiate(_arrowPrefab, arrowLaunchData.LaunchPosition, Quaternion.identity);
		projectile.GetComponent<ArrowMover>().Launch(arrowLaunchData);

		if(_shouldDrawPredictedPath)
			_arrowPositionPredicter.DrawPredictedArrowPositions(arrowLaunchData);
	}
}
