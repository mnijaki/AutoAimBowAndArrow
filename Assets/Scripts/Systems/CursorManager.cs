using UnityEngine;

public class CursorManager : MonoBehaviour
{
	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}
}