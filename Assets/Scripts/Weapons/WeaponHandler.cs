using System;
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
    private ArrowPositionPredicter _arrowPositionPredicter;

    private void Awake()
    {
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

        float displacementY = targetPos.y - _firingPointPos.y;
        Vector3 displacementXZ = new Vector3(targetPos.x - _firingPointPos.x, 0.0F, targetPos.z - _firingPointPos.z);

        float height = _heightBasedWeaponType.Height;
        float gravity = Physics.gravity.y;
        
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        float timeToReachTarget = Mathf.Sqrt(-2 * height / gravity) + Mathf.Sqrt(2 * (displacementY - height) / gravity);
        Vector3 velocityXZ = displacementXZ / timeToReachTarget;
        Vector3 initialVelocity = velocityXZ + velocityY;

        ArrowLaunchData arrowLaunchData = new( _firingPointPos, initialVelocity, timeToReachTarget , Physics.gravity);
        LaunchProjectile(arrowLaunchData);
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
