using UnityEngine;

public interface ITargetable
{
    Vector3 GetPosition();

    Collider GetCollider();
}
