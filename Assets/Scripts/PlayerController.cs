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
    private LayerMask _groundLayers;

    private const float _IS_GROUNDED_THRESHOLD = 0.5F;
    private float _groundRayCheckDistance;
    private Vector3 _movementInput;
    private Vector3 _movement;
    private Vector3 _playerVelocity;
    private Transform _cameraTransform;
    private Rigidbody _rigidbody;
    private WeaponHandler _weaponHandler;

    [SerializeField]
    private Transform _tmpTarget;

    private void Awake()
    {
        _cameraTransform = Camera.main.transform;
        _rigidbody = GetComponent<Rigidbody>();
        
        float playerHeight = GetComponent<CapsuleCollider>().height;
        _groundRayCheckDistance = playerHeight / 2 + _IS_GROUNDED_THRESHOLD;

        _weaponHandler = GetComponent<WeaponHandler>();
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
        return Physics.Raycast(transform.position, Vector3.down, _groundRayCheckDistance, _groundLayers);
    }

    private bool IsInTheAir()
    {
        return (_rigidbody.velocity.y > 0.1F);
    }
    
    private void OnShoot()
    {
        _weaponHandler.Shoot(_tmpTarget.transform.position);
    }
}
