using UnityEngine;

public class HeightBasedProjectileLauncher : MonoBehaviour
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

    public void Shoot(HeightBasedWeaponType heightBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
    {
        // SUVAT kinematic Equations:
        // See video made by Sebastian League (you cant watch all 3 videos if you want to know more).
        // https://www.youtube.com/watch?v=IvT8hjy6q4o&list=PLFt_AvWsXl0eMryeweK7gc9T04lJCIg_W&index=3

        
        // TODO: here should be also taken to account that enemy can be on different height than player !!!
        
        float displacementY = targetPos.y - firingPointPos.y;
        Vector3 displacementXZ = new(targetPos.x - firingPointPos.x, 0.0F, targetPos.z - firingPointPos.z);

        float arcHeight = CalculateArcHeight(heightBasedWeaponType, firingPointPos, targetPos);
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * _gravity.y * arcHeight);
        float timeToReachTarget = Mathf.Sqrt(-2 * arcHeight / _gravity.y) + Mathf.Sqrt(2 * (displacementY - arcHeight) / _gravity.y);
        Vector3 velocityXZ = displacementXZ / timeToReachTarget;
        Vector3 initialVelocity = velocityXZ + velocityY;

        ArrowLaunchData arrowLaunchData = new(firingPointPos, initialVelocity, timeToReachTarget , _gravity);
        LaunchProjectile(arrowLaunchData);
    }

    private float CalculateArcHeight(HeightBasedWeaponType heightBasedWeaponType, Vector3 firingPointPos, Vector3 targetPos)
    {
        float height = heightBasedWeaponType.Height;
        
        // BE CAREFUL: flattening of arc must happen before applying difference in height between firing point and target.
        // Don't change invocation order of those two methods.
        if(_shouldFlattenArcHeight)
            height = FlattenArcHeightBasedOnTheDistanceToTarget(height, firingPointPos, targetPos);

        height = ApplyDifferenceInHeightBetweenFiringPointAndTarget(height, firingPointPos, targetPos);

        return height;
    }

    private static float FlattenArcHeightBasedOnTheDistanceToTarget(float height, Vector3 firingPointPos, Vector3 targetPos)
    {
        // Designer wanted to change height of arrow parable based on distance from target.
        // If target is far away, parable should be high.
        // If target is close, parable should be low (close to straight line).
        // If you don't need this kind of behaviour just remove or comment usage of this method.
        
        // We assume that firing from distance closer than 2m should lead to almost straight firing arc.
        const float MIN_FLATTENING_RANGE = 2.0F;
        // We assume that firing from distance greater than 50m should lead to firing arc equal to passed height.
        const float MAX_FLATTENING_RANGE = 50.0F;
        const float FLATTENING_RANGE = MAX_FLATTENING_RANGE - MIN_FLATTENING_RANGE;
        
        float distance = Vector3.Distance(targetPos,firingPointPos);
        distance = distance - MIN_FLATTENING_RANGE;
        
        float percent = distance / FLATTENING_RANGE;
        percent = Mathf.Clamp01(percent);
        
        // Here you can even use curve between 0..1 if you want custom flattening of height function.
        // percent = _flatteningCurve.Evaluate(percent);
        
        float flattenedHeight = height * percent;
        
        // Height must be greater than zero.
        // If height was zero, then arrow would have to be launched at speed close to infinity to mitigate gravity.
        flattenedHeight = Mathf.Clamp(flattenedHeight, 0.1F, float.MaxValue);
        
        return flattenedHeight;
    }
    
    private static float ApplyDifferenceInHeightBetweenFiringPointAndTarget(float height, Vector3 firingPointPos, Vector3 targetPos)
    {
        float differenceInHeight = targetPos.y - firingPointPos.y;
        differenceInHeight = Mathf.Clamp(differenceInHeight, 0.0F, float.MaxValue);
        height = height + differenceInHeight;

        return height;
    }

    private void LaunchProjectile(ArrowLaunchData arrowLaunchData)
    {
        GameObject projectile = Instantiate(_arrowPrefab, arrowLaunchData.LaunchPosition, Quaternion.identity);
        projectile.GetComponent<ArrowMover>().Launch(arrowLaunchData);

        if(_shouldDrawPredictedPath)
            _arrowPositionPredicter.DrawPredictedArrowPositions(arrowLaunchData);
    }
}
