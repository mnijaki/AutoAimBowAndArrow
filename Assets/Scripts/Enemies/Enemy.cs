using UnityEngine;

public class Enemy : MonoBehaviour, ITargetable
{
    private Collider _collider;
    
    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public Vector3 GetPosition()
    {
	    return transform.position;
    }

    public Collider GetCollider()
    {
        return _collider;
    }
}
