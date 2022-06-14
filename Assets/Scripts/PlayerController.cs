using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private InputReader _inputReader;
    [SerializeField]
    private float _playerSpeed = 400.0F;
    [SerializeField]
    private float _rotationSpeed = 5.0F;
    [SerializeField]
    private float _jumpForce = 650.0F;
    [SerializeField]
    private GameObject _arrowPrefab;
    [SerializeField]
    private LayerMask _arrowInteractionLayers;
    [SerializeField]
    private Transform _firingPoint;
    [SerializeField]
    private LayerMask _groundLayers;

    private const float _IS_GROUNDED_THRESHOLD = 0.5F;
    private float _groundRayCheckDistance;
    private Vector3 _movementInput;
    private Vector3 _movement;
    private Vector3 _playerVelocity;
    private Transform _cameraTransform;
    private Rigidbody _rigidbody;
    
    private WeaponType _currentWeaponType;
    private HeightBasedWeaponType _heightBasedWeaponType;
    private VelocityBasedWeaponType _velocityBasedWeaponType;

    [SerializeField]
    private Transform _tmpTarget;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        _rigidbody = GetComponent<Rigidbody>();
        
        float playerHeight = GetComponent<CapsuleCollider>().height;
        _groundRayCheckDistance = playerHeight / 2 + _IS_GROUNDED_THRESHOLD;
    }

    private void OnEnable()
    { 
        _inputReader.moveEvent += OnMove;
        _inputReader.shootEvent += OnShoot;
    }

    private void OnDisable()
    {
        _inputReader.moveEvent -= OnMove;
        _inputReader.shootEvent -= OnShoot;
    }
    
    private void OnMove(Vector3 movement)
    {
        _movementInput = movement;
    }
    
    private void OnShoot()
    {
        switch(_currentWeaponType)
        {
            case WeaponType.None:
                return;
            case WeaponType.HeightBased:
                ShootHeightBasedProjectile();
                break;
            default:
                ShootVelocityBasedProjectile();
                break;
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
        HandleJump();
    }

    private void HandleMovement()
    {
        ApplyCameraDirectionToMovement();
        _movement = _movement * (Time.deltaTime * _playerSpeed);
        SetCurrentYVelocityToMovement();
        _rigidbody.velocity = _movement;
    }
    
    private void ApplyCameraDirectionToMovement()
    {
        // This will transform input movement vector to current camera view.
        _movement = _movementInput.x * _cameraTransform.right.normalized + _movementInput.z * _cameraTransform.forward.normalized;
    }

    private void SetCurrentYVelocityToMovement()
    {
        // Make sure movement vector is not influencing vertical position of rigidbody.
        _movement.y = _rigidbody.velocity.y;
    }

    private void HandleRotation()
    {
        // Rotate player in direction where camera is facing.
        Quaternion targetRotation = Quaternion.Euler(0.0F,_cameraTransform.eulerAngles.y,0.0F);
        Quaternion slerpTargetRotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
        _rigidbody.MoveRotation(slerpTargetRotation);
    }

    private void HandleJump()
    {
        if(!_inputReader.Jumped)
        {
            return;
        }

        if(IsInTheAir())
        {
            return;
        }
        
        if(!CanJump())
        {
            return;
        }
        
        _rigidbody.AddForce(Vector3.up * _jumpForce, ForceMode.Impulse);
    }
    
    private bool CanJump()
    {
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down, out hit, _groundRayCheckDistance, _groundLayers))
        {
            return true;
        }

        return false;
    }

    private bool IsInTheAir()
    {
        return (_rigidbody.velocity.y > 0.1F);
    }

    public void ChangeWeapon(HeightBasedWeaponType heightBasedWeaponType)
    {
        _currentWeaponType = WeaponType.HeightBased;
        _heightBasedWeaponType = heightBasedWeaponType;
    }
    
    public void ChangeWeapon(VelocityBasedWeaponType velocityBasedWeaponType)
    {
        _currentWeaponType = WeaponType.VelocityBased;
        _velocityBasedWeaponType = velocityBasedWeaponType;
    }

    private void ShootHeightBasedProjectile()
    {
        // SUVAT kinematic Equations:
        // 1. S = ((U + V)/2) * T
        // 2. V = U + A*T
        // 3. S = U*T + ((A*T*T)/2)
        // 4. S = V*T - ((A*T*T)/2)
        // 5. V*V = U*U + 2*A*S
        // SUVAT parameters meaning:
        // S - displacement in meters (road to travel)
        // U - initial velocity in meters per second
        // V - final velocity in meters per second
        // A - acceleration in meters per seconds^2
        // T - time in seconds
        
        // A - starting point
        // P - target point
        // Px - horizontal displacement
        // Py - vertical displacement
        // G - gravity acceleration
        // H - maximum height of parable
        // Vi - initial velocity to use for launching, computed based of above data
        
        // S - we have (its magnitude of the distance between player and target)
        // U - we are looking for
        // V - final velocity.
        //     We can assume that it will be some small number.
        //     If you miss it would look strange if arrow would stop in the mid air. 
        //     That is why final velocity should be great than 0 m/s. 
        // A - we have (its gravity)
        // T - we don't have
        
        // We split whole problem into 3 computations: upward, horizontal and downward motions.
        // 1. Upward motion:
        // We have:
        // S - displacement is equal to maximum height of parable (on to of the arc)
        // A - acceleration is gravity value
        // V - final velocity is 0 on top of the arc
        // We will use fifth SUVAT equation because it is the closest to compute for our data.
        // V*V = U*U + 2*A*S
        // 0*0 = U*U + 2*G*H
        // U*U = -2*G*H
        // U = sqrt(-2*G*H)

        Vector3 targetPos = _tmpTarget.transform.position;
        float displacementY = targetPos.y - _firingPoint.position.y;
        Vector3 displacementXZ = new Vector3(targetPos.x - _firingPoint.position.x, 0.0F, targetPos.z - _firingPoint.position.z);

        float height = _heightBasedWeaponType.Height;
        float gravity = Physics.gravity.y;
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * height);
        var delta = Mathf.Sqrt(-2 * height / gravity);
        Vector3 velocityXZ = displacementXZ / (delta + Mathf.Sqrt(2 * (displacementY - height) / gravity));

        SpawnProjectile(velocityXZ + velocityY);
    }
    
    private void ShootVelocityBasedProjectile()
    {
        //SpawnProjectile();
    }

    private void SpawnProjectile(Vector3 initialVelocity)
    {
        GameObject projectile = Instantiate(_arrowPrefab, _firingPoint.position, Quaternion.identity);
        projectile.GetComponent<Rigidbody>().velocity = initialVelocity;
    }
    
}
