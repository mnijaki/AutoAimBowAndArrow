using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

[SuppressMessage("ReSharper", "InlineTemporaryVariable")]
public class VelocityBasedProjectileLauncher : MonoBehaviour
{
	[SerializeField]
	private GameObject _arrowPrefab;
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
		
		// Float precision and equality would lead to almost never using calculations based of flat surface.
		// We assume that minor change in height is irrelevant, so calculation based on flat surface
		// would be used more often (it is the fastest one). 
		if(Mathf.Abs(differenceInHeight) < 0.1F)
		{
			// I created two methods for shooting on flat surfaces, just to check out if the differ somehow in output.
			// Seems they output the same value so you can use either of those two.
			ShootOnTargetAtFlatSurfaceUsingVectorForward(velocityBasedWeaponType, firingPointPos, targetPos);
			//ShootOnTargetAtFlatSurfaceUsingDirection(velocityBasedWeaponType, firingPointPos, targetPos);
		}
		else if(differenceInHeight > 0.1F)
		{
			ShootOnTargetBelow(velocityBasedWeaponType, firingPointPos, targetPos);
		}
		else
		{
			ShootOnTargetAbove(velocityBasedWeaponType, firingPointPos, targetPos);
		}
	}
	
	private void ShootOnTargetAtFlatSurfaceUsingVectorForward(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// ************************************************************************************************************************************
		// This method works only if target and shooter are on the same level (both have equal 'position.y' value).
		// ************************************************************************************************************************************
		// Equation of oblique throw angle (for targets on the same level):
		// https://www.youtube.com/watch?v=3UYjw30h0jU&list=PLX2gX-ftPVXWO27_anI2Y3yR3790qdkVe
		// https://www.youtube.com/watch?v=4JsoRFlHJnE&list=PLX2gX-ftPVXWO27_anI2Y3yR3790qdkVe&index=2
		// ************************************************************************************************************************************
		// angle = (1/2) * sin^-1(G*X / Vo^2)
		// angle - angle at which projectile was launched (angle is relative to horizontal axis)
		// G - gravity
		// X - distance to target
		// Vo - initial velocity (consist of velocity in all axis)
		// ************************************************************************************************************************************
		// Information needed to use above equation in Unity:
		// sin^-1 - sinus to the power of -1, in Unity we use for that: Mathf.Asin(...) * Mathf.Rad2Deg;
		//          Mathf.Asin(...) can only receive values between -1.0F and 1.0F
		// Vo^2 - initial velocity to the power of 2, in Unity we use for that: Mathf.Pow(...);
		// For easier calculations in Unity we change equation:
		// angle = (1/2) * sin^-1(G*X / Vo^2)
		// into 
		// 2 * angle = sin^-1(G*X / Vo^2)
		// ************************************************************************************************************************************

		// Compute base data.
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(targetPos,firingPointPos);
		
		// Compute angle of the arc (we assume shooter and target are on the same surface level).
		var sin2Angle = (distance * _gravity.y) / Mathf.Pow(initialVelocity,2);
		if(sin2Angle is < -1.0F or > 1.0F)
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		float angle = Mathf.Asin(sin2Angle)  / 2 * Mathf.Rad2Deg;
		
		// Calculate base vector.
		Vector3 velocityVector = (Vector3.forward * initialVelocity);
		// Rotate velocityVector upwards along right axis, by the angle we computed before (this will determine angle of the arc).
		velocityVector = Quaternion.AngleAxis(angle, Vector3.right) * velocityVector;
		// Rotate velocityVector towards target direction.
		Vector3 directionToTarget = (targetPos - firingPointPos).normalized;
		velocityVector = Quaternion.LookRotation(directionToTarget) * velocityVector;
		
		// Calculate flight time.
		float timeToReachTarget = CalculateFlightTimeOfProjectile(distance, initialVelocity, angle);

		// Launch projectile.
		LaunchProjectile(firingPointPos, velocityVector, timeToReachTarget);
	}

	private void ShootOnTargetAtFlatSurfaceUsingDirection(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// ************************************************************************************************************************************
		// This method works only if target and shooter are on the same level (both have equal 'position.y' value).
		// ************************************************************************************************************************************
		// Equation of oblique throw angle (for targets on the same level):
		// https://www.youtube.com/watch?v=3UYjw30h0jU&list=PLX2gX-ftPVXWO27_anI2Y3yR3790qdkVe
		// https://www.youtube.com/watch?v=4JsoRFlHJnE&list=PLX2gX-ftPVXWO27_anI2Y3yR3790qdkVe&index=2
		// ************************************************************************************************************************************
		// angle = (1/2) * sin^-1(G*X / Vo^2)
		// angle - angle at which projectile was launched (angle is relative to horizontal axis)
		// G - gravity
		// X - distance to target
		// Vo - initial velocity (consist of velocity in all axis)
		// ************************************************************************************************************************************
		// Information needed to use above equation in Unity:
		// sin^-1 - sinus to the power of -1, in Unity we use for that: Mathf.Asin(...) * Mathf.Rad2Deg;
		//          Mathf.Asin(...) can only receive values between -1.0F and 1.0F
		// Vo^2 - initial velocity to the power of 2, in Unity we use for that: Mathf.Pow(...);
		// For easier calculations in Unity we change equation:
		// angle = (1/2) * sin^-1(G*X / Vo^2)
		// into 
		// 2 * angle = sin^-1(G*X / Vo^2)
		// ************************************************************************************************************************************
		
		// Compute base data.
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(targetPos,firingPointPos);
		Vector3 directionToTarget = (targetPos - firingPointPos).normalized;
		Vector3 velocityVector = directionToTarget * initialVelocity;
		
		// Compute angle of the arc (we assume shooter and target are on the same surface level).
		var sin2Angle = ((distance * _gravity.y) / Mathf.Pow(velocityVector.magnitude,2));
		if(sin2Angle is < -1.0F or > 1.0F)
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		float angle = Mathf.Asin(sin2Angle)  / 2 * Mathf.Rad2Deg;
		
		// Create axis that is perpendicular to velocityVector and Vector3.up (we will rotate velocityVector around this axis).
		Vector3 upAxis = Vector3.Cross(velocityVector, Vector3.up);
		// Handle case where velocityVector is collinear with up.
		if (upAxis == Vector3.zero) upAxis = Vector3.right;
		// Rotate velocityVector upwards, by the angle we computed before (this will determine angle of the arc).
		velocityVector = Quaternion.AngleAxis(-angle, upAxis) * velocityVector;

		// Calculate flight time.
		float timeToReachTarget = CalculateFlightTimeOfProjectile(distance, initialVelocity, angle);
		
		// Launch projectile.
		LaunchProjectile(firingPointPos, velocityVector, timeToReachTarget);
	}

	private void ShootOnTargetBelow(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// ************************************************************************************************************************************
		// This method works only if target is below shooter.
		// ************************************************************************************************************************************
		// Equation of oblique throw angle (for targets below shooter):
		// https://www.youtube.com/watch?v=bqYtNrhdDAY
		// https://www.youtube.com/watch?v=Edlt5r7pNwI&list=PLX2gX-ftPVXUUlf-9Eo_6ut4kP6wKaSWh&index=6
		// ************************************************************************************************************************************
		// angle - angle at which projectile was launched (angle is relative to horizontal axis)
		// Y - final height of the projectile (height above the ground when projectile will reach the target) 
		// Yo - initial height (height above the ground when projectile was launched)
		// Vo - initial velocity (consist of velocity in all axis)
		// Voy - initial vertical velocity
		// Vox - initial horizontal velocity
		// G - gravity
		// X - distance to target
		// T - time in the air
		// ************************************************************************************************************************************
		// 1. Determine initial horizontal and vertical velocity (vertical velocity is with minus sign because we shoot downwards):
		// Voy = -Vo * sin(angle) 
		// Vox = Vo * cos(angle)
		// 2. Determine time in the air of the projectile, based on vertical movement (y):
		// Y = Yo + (Voy * T) + ((1/2) * G * T^2)
		// Now we plug in initial vertical velocity.
		// Y = Yo + (-Vo * sin(angle) * T) + ((1/2) * G * T^2)
		// 3. Determine time in the air of the projectile, based on horizontal movement (x):
		// X = Vox * T
		// Now we plug in initial horizontal velocity:
		// X = Vo * cos(angle) * T
		// 4. Determine time from above:
		// T = X / (Vo * cos(angle))
		// 5. Plug horizontal time equation into vertical time equation (since both times must be equal):
		// Y = Yo + (-Vo * sin(angle) * (X / (Vo * cos(angle)))) + ((1/2) * G * (X / (Vo * cos(angle)))^2)
		// 6. Do simple math operations:
		// Y = Yo + (-sin(angle) * X / cos(angle)) + ((1/2) * G * (X / (Vo * cos(angle)))^2)
		// Y = Yo + (-sin(angle) * X / cos(angle)) + ((1/2) * G * (X^2 / (Vo^2 * cos^2(angle))))
		// 0 = Yo - Y + (-sin(angle) * X / cos(angle)) + ((1/2) * G * (X^2 / (Vo^2 * cos^2(angle))))
		// 0 = Yo - Y + (-X * sin(angle) / cos(angle)) + ((1/2) * G * (X^2 / (Vo^2 * cos^2(angle))))
		// 0 = (Yo - Y) * cos^2(angle) - (X * sin(angle) * cos(angle)) + ((1/2) * G * X^2 / Vo^2) 
		// 0 = (Yo - Y) * cos^2(angle) - (X * sin(angle) * cos(angle)) + (G * X^2 / (2 * Vo^2)) 
		// 6. Create 'a' variable as substitution for last part of equation.
		// A = G * X^2 / (2 * Vo^2)
		// 7. Plug variable into equation:
		// 0 = (Yo - Y) * cos^2(angle) - (X * sin(angle) * cos(angle)) + A 
		// 8. Use cos^2(angle) equation:
		// cos^2(angle) = 1 - sin^2(angle)
		// 9. Plug above to main equation:
		// 0 = (Yo - Y) * (1 - sin^2(angle)) - (X * sin(angle) * cos(angle)) + A 
		// 10. Create variable 'H' to store difference in height:
		// H = Yo - Y
		// 11. Plug above into main equation:
		// 0 = H * (1 - sin^2(angle)) - (X * sin(angle) * cos(angle)) + A
		// 12. Do simple math operations:
		// 0 = H - (H * sin^2(angle)) - (X * sin(angle) * cos(angle)) + A
		// 0 = -(H * sin^2(angle)) - (X * sin(angle) * cos(angle)) + (H + A)
		// 13. Use sin(angle) * cos(angle) equation:
		// sin(angle) * cos(angle) = (1/2) * sin(2*angle)
		// 14. Plug above to main equation:
		// 0 = -(H * sin^2(angle)) - (X * (1/2) * sin(2*angle)) + (H + A)
		// 15. Use sin^2(angle) equation:
		// sin^2(angle) = (1/2) * (1 - cos(2*angle))
		// 16. Plug above to main equation:
		// 0 = -(H * (1/2) * (1 - cos(2*angle))) - (X * (1/2) * sin(2*angle)) + (H + A)
		// 17. Do simple math operations:
		// 0 = -((H/2) * (1 - cos(2*angle))) - (X * (1/2) * sin(2*angle)) + (H + A)
		// 0 = -(H/2) + ((H/2) * cos(2*angle)) - (X * (1/2) * sin(2*angle)) + (H + A)
		// 0 = ((H/2) * cos(2*angle)) - (X * (1/2) * sin(2*angle)) + (H - (H/2) + A)
		// 0 = ((H/2) * cos(2*angle)) - (X * (1/2) * sin(2*angle)) + ((H/2) + A)
		// 0 = ((H/2) * cos(2*angle)) - ((X/2) * sin(2*angle)) + ((H/2) + A)
		// 0 = (H * cos(2*angle)) + (-X * sin(2*angle)) + (H + (2*A))
		// 0 = (-X * sin(2*angle)) + (H * cos(2*angle)) + (H + (2*A))
		// 18. Use single angle function (single trigonometric function).
		//     Express sum of sine and cosine as single trigonometric function.
		//     https://www.mathcentre.ac.uk/resources/uploaded/mc-ty-rcostheta-alpha-2009-1.pdf
		//     https://slideplayer.com/slide/14443418/
		//     https://matematykaszkolna.pl/strona/3670.html
		// (c1 * sin(someAngle)) + (c2 * cos(someAngle)) = R * cos(someAngle - phaseAngle)
		// phaseAngle = tan^-1(c1/c2)
		// R = sqrt(c1^2 + c2^2)
		// 19. Plug above to main equation:
		// 0 = R * cos(2*angle - phaseAngle) + (H + (2*A))
		// R = sqrt(X^2 + H^2)
		// 0 = sqrt(X^2 + H^2) * cos(2*angle - phaseAngle) + (H + (2*A))
		// 20. Do simple math operations:
		// sqrt(X^2 + H^2) * cos(2*angle - phaseAngle) + (H + (2*A)) = 0
		// sqrt(X^2 + H^2) * cos(2*angle - phaseAngle) = -(H + (2*A))  
		// cos(2*angle - phaseAngle) = (-(H + (2*A))) / sqrt(X^2 + H^2)
		// cos(2*angle - phaseAngle) = (-(2A + H)) / sqrt(X^2 + H^2)
		// 21. Plug phase angle to main equation:
		// phaseAngle = tan^-1(-X/H)
		// cos(2*angle - tan^-1(-X/H)) = (-(2A + H)) / sqrt(X^2 + H^2)
		// 22. Do simple math operations:
		// cos(2*angle - tan^-1(-X/H)) = (-2A - H) / sqrt(X^2 + H^2)
		// (2*angle - tan^-1(-X/H)) = cos^-1 [(-2A - H) / sqrt(X^2 + H^2)]
		// ************************************************************************************************************************************
		
		// Check difference in horizontal value between shooter and target.
		float differenceInHeight = firingPointPos.y - targetPos.y;
		if(differenceInHeight <= 0.0F)
		{
			Debug.LogWarning("Method "+nameof(ShootOnTargetBelow)+" can only be used for targets below shooter");
			return;
		}
		
		// Compute base data.
		float G = _gravity.y;
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(new Vector3(targetPos.x, 0.0F, targetPos.z), 
		                                  new Vector3(firingPointPos.x, 0.0F, firingPointPos.z));
		
		// Compute phase angle.
		float phaseAngle = Mathf.Atan(-distance / differenceInHeight) * Mathf.Rad2Deg;
		
		// Compute helper variable (to shorten equation).
		float A = (G * Mathf.Pow(distance, 2)) / (2 * Mathf.Pow(initialVelocity, 2));

		// Compute arc-cosine of angle.
		float acosAngle = (-2 * A - differenceInHeight) / Mathf.Sqrt(Mathf.Pow(distance, 2) + Mathf.Pow(differenceInHeight, 2));
		if(acosAngle is < -1 or > 1)
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		
		// Compute launch angle.
		acosAngle = Mathf.Acos(acosAngle) * Mathf.Rad2Deg;
		float angle = (acosAngle + phaseAngle) / 2;
		
		// Calculate base velocityVector.
		Vector3 velocityVector = (Vector3.forward * initialVelocity);
		// Rotate velocityVector upwards along right axis, by the angle we computed before (this will determine angle of the arc).
		velocityVector = Quaternion.AngleAxis(angle, Vector3.right) * velocityVector;
		// Rotate velocityVector towards target direction (we omit 'Y' axis, because it was already included in the angle).
		Vector3 directionToTarget = (new Vector3(targetPos.x, 0.0F, targetPos.z) - 
		                             new Vector3(firingPointPos.x, 0.0F, firingPointPos.z)).normalized;
		velocityVector = Quaternion.LookRotation(directionToTarget) * velocityVector;
		
		// Calculate flight time.
		float timeToReachTarget = CalculateFlightTimeOfProjectile(distance, initialVelocity, angle);

		// Launch projectile.
		LaunchProjectile(firingPointPos, velocityVector, timeToReachTarget);
	}
	
	private void ShootOnTargetAbove(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
		// ************************************************************************************************************************************
		// This method works only if target is above shooter.
		// ************************************************************************************************************************************
		// Equation of oblique throw angle (for targets above shooter):
		// https://www.youtube.com/watch?v=krzC92hZ8pA&list=PLX2gX-ftPVXUUlf-9Eo_6ut4kP6wKaSWh&index=8
		// ************************************************************************************************************************************
		// angle - angle at which projectile was launched (angle is relative to horizontal axis)
		// Y - final height of the projectile (height above the ground when projectile will reach the target) 
		// Yo - initial height (height above the ground when projectile was launched)
		// Vo - initial velocity (consist of velocity in all axis)
		// Voy - initial vertical velocity
		// Vox - initial horizontal velocity
		// G - gravity
		// X - distance to target
		// T - time in the air
		// ************************************************************************************************************************************
		// 1. Determine initial horizontal and vertical velocity:
		// Voy = Vo * sin(angle)
		// Vox = Vo * cos(angle)
		// 2. Determine time in the air of the projectile, based on vertical movement (y):
		// Y = Yo + (Voy * T) + ((1/2) * G * T^2)
		// Now we plug in initial vertical velocity.
		// Y = Yo + (Vo * sin(angle) * T) + ((1/2) * G * T^2)
		// 3. Determine time in the air of the projectile, based on horizontal movement (x):
		// X = Vox * T
		// Now we plug in initial horizontal velocity:
		// X = Vo * cos(angle) * T
		// 4. Determine time from above:
		// T = X / (Vo * cos(angle))
		// 5. Plug horizontal time equation into vertical time equation (since both times must be equal):
		// Y = Yo + (Vo * sin(angle) * (X / (Vo * cos(angle)))) + ((1/2) * G * (X / (Vo * cos(angle)))^2)
		// 6. sin(angle) / cos(angle) can be written as tan(angle)
		// Y = Yo + (Vo * tan(angle) * (X / Vo)) + ((1/2) * G * (X / (Vo * cos(angle)))^2)
		// 7. Extract (X / Vo) from last part of equation
		// Y = Yo + (Vo * tan(angle) * (X / Vo)) + ((1/2) * G * (X / Vo)^2 * ((1 / * cos(angle))^2)
		// 8. (1 / * cos(angle))^2 can be written as sec^2(angle)
		// Y = Yo + (Vo * tan(angle) * (X / Vo)) + ((1/2) * G * (X / Vo)^2 * sec^2(angle)
		// 9. sec^2(angle) equation:
		//  sec^2(angle) = 1 + tan^2(angle)
		// 10. Plug above in to main equation
		// Y = Yo + (Vo * tan(angle) * (X / Vo)) + ((1/2) * G * (X / Vo)^2 * (1 + tan^2(angle))
		// 11. Create variables to shorten up equation:
		// tmp = (1/2) * G * (X / Vo)^2
		// 12. Shorten equation:
		// Y = Yo + (Vo * tan(angle) * (X / Vo)) + tmp * (1 + tan^2(angle))
		// Y = Yo + tan(angle) * X + tmp * (1 + tan^2(angle))
		// 13. Do simple math operations:
		// Y = Yo + (X * tan(angle)) + tmp * (1 + tan^2(angle))
		// Y = Yo + (X * tan(angle)) + tmp + (tmp * tan^2(angle))
		// 0 = Yo + (X * tan(angle)) + tmp + (tmp * tan^2(angle)) - Y
		// 14. Change order of elements to read it more nicely:
		// 0 = (tmp * tan^2(angle)) + (X * tan(angle)) + (tmp + Yo - Y)
		// 15. Now we can see that we have quadratic equation.
		//     To solve it we can pass new variable in place of tangent.
		//     Lets call this variable 'Z':
		// 0 = (tmp * Z^2) + (X * Z) + (tmp + Yo - Y)
		// 16. Now we can use quadratic formula:
		// Z = (-b +/- sqrt(b^2 - 4*a*c)) / (2*a)
		// a = tmp;
		// b = X
		// c = (tmp + Yo - Y)
		// 17. We should receive two results.
		//     After that we can plug both of those results into tan^-1 function and receive two angles.
		//     Angles between angle_1 and angle_2 will allow projectile to go above target.
		//     Thus we can assume that the lowest of the angles is the angle that shooter needs to hit the target.
		// ************************************************************************************************************************************
		// Information needed to use above equation in Unity:
		// tan^-1 - tangent to the power of -1, in Unity we use for that: Mathf.Atan(...) * Mathf.Rad2Deg;
		//          Mathf.Atan(...) can only receive values between -1.0F and 1.0F
		// Vo^2 - initial velocity to the power of 2, in Unity we use for that: Mathf.Pow(...);
		// ************************************************************************************************************************************
		
		// Check difference in horizontal value between shooter and target.
		float differenceInHeight = firingPointPos.y - targetPos.y;
		if(differenceInHeight >= 0.0F)
		{
			Debug.LogWarning("Method "+nameof(ShootOnTargetAbove)+" can only be used for targets above shooter");
			return;
		}
		
		// Compute base data.
		float G = _gravity.y;
		float initialVelocity = velocityBasedWeaponType.InitialVelocity;
		float distance = Vector3.Distance(new Vector3(targetPos.x, 0.0F, targetPos.z), 
		                                  new Vector3(firingPointPos.x, 0.0F, firingPointPos.z));

		// Compute equation data.
		float tmp = (G/2) * MathF.Pow(distance/initialVelocity, 2);
		
		// Compute quadratic equation ('Z' value from equation is result_1 and result_2).
		float a = tmp;
		float b = distance;
		float c = differenceInHeight + tmp;
		float result_1 = (b + Mathf.Sqrt(Mathf.Pow(b, 2) - 4 * a * c)) / (2 * a);
		float result_2 = (b - Mathf.Sqrt(Mathf.Pow(b, 2) - 4 * a * c)) / (2 * a);
		
		// If the value passed to Mathf.Sqrt(...) is negative, Mathf.Sqrt will return 'float.IsNaN' (not a number).
		// If both results are not a number, this means there is no angle to reach target.
		if(float.IsNaN(result_1) && float.IsNaN(result_2))
		{
			Debug.Log("No shooting arc. Target too far. Exiting...");
			return;
		}
		
		// Compute final angles (angles between angle_1 and angle_2 will lead to shoot projectile above target).
		float angle_1 = Mathf.Atan(result_1) * Mathf.Rad2Deg;
		float angle_2 = Mathf.Atan(result_2) * Mathf.Rad2Deg;
		
		// Compute angle needed to hit the target (we choose angle that result in steeper arc, it looks better).
		float angle = Mathf.Max(angle_1, angle_2);
		// Calculate base velocityVector.
		Vector3 velocityVector = (Vector3.forward * initialVelocity);
		// Rotate velocityVector upwards along right axis, by the angle we computed before (this will determine angle of the arc).
		velocityVector = Quaternion.AngleAxis(angle, Vector3.right) * velocityVector;
		// Rotate velocityVector towards target direction (we omit 'Y' axis, because it was already included in the angle).
		Vector3 directionToTarget = (new Vector3(targetPos.x, 0.0F, targetPos.z) - 
		                             new Vector3(firingPointPos.x, 0.0F, firingPointPos.z)).normalized;
		velocityVector = Quaternion.LookRotation(directionToTarget) * velocityVector;
		
		// Calculate flight time.
		float timeToReachTarget = CalculateFlightTimeOfProjectile(distance, initialVelocity, angle);
		
		// Launch projectile.
		LaunchProjectile(firingPointPos, velocityVector, timeToReachTarget);
	}

	private static float CalculateFlightTimeOfProjectile(float distance, float initialVelocity, float angle)
	{
		return distance / (initialVelocity * Mathf.Cos(angle * Mathf.Deg2Rad));
	}
	
	private void LaunchProjectile(Vector3 firingPointPos, Vector3 velocityVector, float timeToReachTarget)
	{
		ArrowLaunchData arrowLaunchData = new(firingPointPos, velocityVector, timeToReachTarget , _gravity);
		
		GameObject projectile = Instantiate(_arrowPrefab, arrowLaunchData.LaunchPosition, Quaternion.identity);
		projectile.GetComponent<ArrowMover>().Launch(arrowLaunchData);

		if(_shouldDrawPredictedPath)
			_arrowPositionPredicter.DrawPredictedArrowPositions(arrowLaunchData);
	}

	/// <summary>
	///   Function that returns upwards angle between shooter and the target.
	/// </summary>
	/// <param name="firingPointPos">Position of firing point (point from where projectile will be launched)</param>
	/// <param name="targetPos">Position of the target</param>
	/// <returns>Upwards angle between shooter and the target</returns>
	private float GetUpwardAngleTowardTarget(Vector3 firingPointPos, Vector3 targetPos)
	{
		// I used this function before when experimenting. 
		// Currently it can be removed, but I left it here if anyone needed it for their custom calculations.
		float angleTowardsTarget = Mathf.Atan2((targetPos - firingPointPos).normalized.y, (targetPos - firingPointPos).normalized.z) 
		                           * Mathf.Rad2Deg;

		return angleTowardsTarget;
	}
}
