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


		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(targetPos,firingPointPos);
		var sinAngle = (distance * -_gravity.y) / (initialVelocity * initialVelocity);
		float angle = (Mathf.Asin(sinAngle) * Mathf.Rad2Deg) / 2;

		Vector3 directionToTarget = (targetPos - firingPointPos).normalized;
		Vector3 velocityWithAngle = directionToTarget * initialVelocity;
		velocityWithAngle = Quaternion.AngleAxis(-angle, Vector3.right) * velocityWithAngle;
		
		// angle 45? too big too low?
		//velocityWithAngle = new Vector3(velocityWithAngle.x, velocityWithAngle.y+3.1F, velocityWithAngle.z);
		
		Debug.Log("targetPos = "+targetPos);
		Debug.Log("firingPointPos = "+firingPointPos);
		Debug.Log("distance = "+distance);
		Debug.Log("angle = "+angle);
		Debug.Log("directionToTarget = "+directionToTarget);
		Debug.Log("velocityWithAngle = "+velocityWithAngle);

		//float timeToReachTarget = Mathf.Abs(2 * (velocityWithAngle.y / _gravity.y));
		float timeToReachTarget =  Mathf.Abs(2 * ((velocityWithAngle.y * Mathf.Sin(angle)) / _gravity.y));
		
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
