using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TargetChooser
{
    private readonly Camera _camera;
    private readonly Transform _firingPoint;
    private Plane[] _planes;

    public TargetChooser(Camera camera, Transform firingPoint)
    {
        _camera = camera;
        _firingPoint = firingPoint;
    }

    public ITargetable ChooseTarget()
    {
        _planes = GeometryUtility.CalculateFrustumPlanes(_camera);

        IEnumerable<ITargetable> targetables = Object.FindObjectsOfType<MonoBehaviour>().OfType<ITargetable>();
        List<ITargetable> targetablesInCameraView = new();
        foreach(ITargetable targetable in targetables)
        {
            if (GeometryUtility.TestPlanesAABB(_planes, targetable.GetCollider().bounds))
            {
                targetablesInCameraView.Add(targetable);
            }
        }

        var closestTarget = targetablesInCameraView.OrderBy(t => Vector3.Distance(t.GetPosition(), _firingPoint.position)).FirstOrDefault();
        return closestTarget;
    }
}
