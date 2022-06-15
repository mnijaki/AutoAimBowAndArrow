using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField]
    private GameObject _arrowPrefab;
    [SerializeField]
    private Transform _firingPoint;
    [SerializeField]
    private bool _shouldDrawPredictedPath;
    
    private WeaponType _currentWeaponType = WeaponType.None;
    private HeightBasedWeaponType _heightBasedWeaponType;
    private VelocityBasedWeaponType _velocityBasedWeaponType;
    private Vector3 _firingPointPos;
    private Vector3 _gravity;
    private ArrowPositionPredicter _arrowPositionPredicter;

    private void Awake()
    {
        _gravity = Physics.gravity;
        _arrowPositionPredicter = GetComponent<ArrowPositionPredicter>();
    }

    private void OnEnable()
    {
        PickUpHandler.HeightBasedWeaponTypePickedUp += OnHeightBasedWeaponTypePickedUp;
        PickUpHandler.VelocityBasedWeaponTypePickedUp += OnVelocityBasedWeaponTypePickedUp;
    }
    
    private void OnDisable()
    {
        PickUpHandler.HeightBasedWeaponTypePickedUp -= OnHeightBasedWeaponTypePickedUp;
        PickUpHandler.VelocityBasedWeaponTypePickedUp -= OnVelocityBasedWeaponTypePickedUp;
    }

    private void OnHeightBasedWeaponTypePickedUp(HeightBasedWeaponType heightBasedWeaponType)
    {
        ChangeWeapon(heightBasedWeaponType);
    }

    private void OnVelocityBasedWeaponTypePickedUp(VelocityBasedWeaponType velocityBasedWeaponType)
    {
        ChangeWeapon(velocityBasedWeaponType);
    }
    
    private void ChangeWeapon(HeightBasedWeaponType heightBasedWeaponType)
    {
        _currentWeaponType = WeaponType.HeightBased;
        _heightBasedWeaponType = heightBasedWeaponType;
    }
    
    private void ChangeWeapon(VelocityBasedWeaponType velocityBasedWeaponType)
    {
        _currentWeaponType = WeaponType.VelocityBased;
        _velocityBasedWeaponType = velocityBasedWeaponType;
    }

    public void Shoot(Vector3 targetPos)
    {
        _firingPointPos = _firingPoint.position;
        switch(_currentWeaponType)
        {
            case WeaponType.None:
                return;
            case WeaponType.HeightBased:
                ShootHeightBasedProjectile(targetPos);
                break;
            case WeaponType.VelocityBased:
                ShootVelocityBasedProjectile(targetPos);
                break;
            default:
                Debug.Log("No implementation for weapon type "+_currentWeaponType);
                break;
        }
    }
    
    private void ShootHeightBasedProjectile(Vector3 targetPos)
    {
        // SUVAT kinematic Equations:
        // See video made by Sebastian League (you cant watch all 3 videos if you want to know more).
        // https://www.youtube.com/watch?v=IvT8hjy6q4o&list=PLFt_AvWsXl0eMryeweK7gc9T04lJCIg_W&index=3

        
        // TODO: here should be also taken to account that enemy can be on different height than player !!!
        
        float displacementY = targetPos.y - _firingPointPos.y;
        Vector3 displacementXZ = new(targetPos.x - _firingPointPos.x, 0.0F, targetPos.z - _firingPointPos.z);

        float height = _heightBasedWeaponType.Height;
        height = FlattenArcHeightBasedOnTheDistanceFromTarget(height, targetPos);
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * _gravity.y * height);
        float timeToReachTarget = Mathf.Sqrt(-2 * height / _gravity.y) + Mathf.Sqrt(2 * (displacementY - height) / _gravity.y);
        Vector3 velocityXZ = displacementXZ / timeToReachTarget;
        Vector3 initialVelocity = velocityXZ + velocityY;

        ArrowLaunchData arrowLaunchData = new( _firingPointPos, initialVelocity, timeToReachTarget , _gravity);
        LaunchProjectile(arrowLaunchData);
    }

    private float FlattenArcHeightBasedOnTheDistanceFromTarget(float height, Vector3 targetPos)
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
        
        float distance = Vector3.Distance(targetPos,_firingPointPos);
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

    private void ShootVelocityBasedProjectile(Vector3 targetPos)
    {
        //LaunchProjectile();
    }

    private void LaunchProjectile(ArrowLaunchData arrowLaunchData)
    {
        GameObject projectile = Instantiate(_arrowPrefab, _firingPoint.position, Quaternion.identity);
        projectile.GetComponent<ArrowMover>().Launch(arrowLaunchData);

        if(_shouldDrawPredictedPath)
            _arrowPositionPredicter.DrawPredictedArrowPositions(arrowLaunchData);
    }
}
