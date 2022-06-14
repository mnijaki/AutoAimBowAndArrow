using UnityEngine;

[CreateAssetMenu(fileName = "HeightBasedWeaponType", menuName="Weapons/Create height based weapon type")]
public class HeightBasedWeaponType : ScriptableObject
{
	[SerializeField]
	[Range(0.1F, 50.0F)]
	[Tooltip("Maximum height of the arrow parable")]
	private float _height = 10.0F;
	
	public float Height => _height;
}
