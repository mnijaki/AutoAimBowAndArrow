using System;
using System.Collections;
using UnityEngine;

public class PickUpHandler : MonoBehaviour
{
    [SerializeField]
    private HeightBasedWeaponType _heightBasedWeaponType;

    [SerializeField]
    private VelocityBasedWeaponType _velocityBasedWeaponType;

    [SerializeField]
    private GameObject _pressurePlate;

    [SerializeField]
    private GameObject _pressurePlateUI;
    
    public static event Action<HeightBasedWeaponType> HeightBasedWeaponTypePickedUp = delegate {};
    public static event Action<VelocityBasedWeaponType> VelocityBasedWeaponTypePickedUp = delegate {};

    private Collider _collider;
    private readonly WaitForSeconds _waitTime = new WaitForSeconds(2.0F);

    private void Awake()
    {
        _collider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PickUpWeapon();
    }

    private void PickUpWeapon()
    {
        if(_heightBasedWeaponType != null)
        {
            HeightBasedWeaponTypePickedUp.Invoke(_heightBasedWeaponType);
        }
        else
        {
            VelocityBasedWeaponTypePickedUp.Invoke(_velocityBasedWeaponType);
        }

        StartCoroutine(TemporaryDisablePickableObject());
    }
    
    private IEnumerator TemporaryDisablePickableObject()
    {
        _collider.enabled = false;
        _pressurePlate.SetActive(false);
        _pressurePlateUI.SetActive(false);
                
        yield return _waitTime;
                
        _collider.enabled = true;
        _pressurePlate.SetActive(true);
        _pressurePlateUI.SetActive(true);
    }
}
