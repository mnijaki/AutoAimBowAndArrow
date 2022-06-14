using UnityEngine;

public class ArrowController : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		DestroyArrowAfterDelay();
	}

	private void OnCollisionEnter(Collision collision)
	{
		DestroyArrowAfterDelay();
	}

	private void DestroyArrowAfterDelay()
	{
		GetComponent<ArrowMover>().enabled = false;
		Destroy(gameObject, 2.0F);
	}
}
