using UnityEngine;

public class ArrowMover : MonoBehaviour
{
    private Rigidbody _rigidbody;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    public void Launch(ArrowLaunchData arrowLaunchData)
    {
        _rigidbody.velocity = arrowLaunchData.InitialVelocity;
    }

    private void FixedUpdate()
    {
        _rigidbody.MoveRotation(Quaternion.LookRotation(_rigidbody.velocity));
    }
}
