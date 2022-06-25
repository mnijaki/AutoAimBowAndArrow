using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField]
    private Transform _firingPoint;

    private TargetChooser _targetChooser;
    private WeaponType _currentWeaponType = WeaponType.None;
    private HeightBasedWeaponType _heightBasedWeaponType;
    private HeightBasedProjectileLauncher _heightBasedProjectileLauncher;
    private VelocityBasedWeaponType _velocityBasedWeaponType;
    private VelocityBasedProjectileLauncher _velocityBasedProjectileLauncher;

    private void Awake()
    {
        _heightBasedProjectileLauncher = GetComponent<HeightBasedProjectileLauncher>();
        _velocityBasedProjectileLauncher = GetComponent<VelocityBasedProjectileLauncher>();

        _targetChooser = new TargetChooser(Camera.main, _firingPoint);
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

    public void Shoot()
    {
        ITargetable target = _targetChooser.ChooseTarget();
        if(target == null)
        {
            Debug.Log("No target to shoot at");
            return;
        }
        
        switch(_currentWeaponType)
        {
            case WeaponType.None:
                return;
            case WeaponType.HeightBased:
                _heightBasedProjectileLauncher.Shoot(_heightBasedWeaponType,_firingPoint.position,target.GetPosition());
                break;
            case WeaponType.VelocityBased:
                _velocityBasedProjectileLauncher.Shoot(_velocityBasedWeaponType,_firingPoint.position,target.GetPosition());
                break;
            default:
                Debug.Log("No implementation for weapon type "+_currentWeaponType);
                break;
        }
    }
}