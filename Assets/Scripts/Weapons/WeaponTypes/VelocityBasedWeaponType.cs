using UnityEngine;

[CreateAssetMenu(fileName = "VelocityBasedWeaponType", menuName="Weapons/Create velocity based weapon type")]
public class VelocityBasedWeaponType : ScriptableObject
{
    [SerializeField]
    [Range(11.0F, 50.0F)]
    [Tooltip("Initial velocity to apply for the arrow. Initial velocity must be higher than gravity to make sure arrow will reach target")]
    private float _initialVelocity = 11.0F;
	
    public float InitialVelocity => _initialVelocity;
}
