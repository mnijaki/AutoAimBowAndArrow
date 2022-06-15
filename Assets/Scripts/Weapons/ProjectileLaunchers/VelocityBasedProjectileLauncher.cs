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
	
	public void Shoot(VelocityBasedWeaponType velocityBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
	{
	}
}
