using UnityEngine;

[CreateAssetMenu(fileName = "HeightBasedWeaponType", menuName="Weapons/Create height based weapon type")]
public class HeightBasedWeaponType : ScriptableObject
{
	[SerializeField]
	[Range(0.1F, 50.0F)]
	[Tooltip("Height of the arrow parable (arc height in inflection point). \n"+
	         "Height must be greater than zero. \n"+
	         "If height was zero, then arrow would have to be launched at speed close to infinity to mitigate gravity.")]
	private float _height = 10.0F;
	
	public float Height => _height;
}
