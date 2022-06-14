using UnityEngine;

public struct ArrowLaunchData
{
    public Vector3 LaunchPosition { get; }
    public Vector3 InitialVelocity { get; }
    public float TimeToReachTarget { get; }
    public Vector3 Gravity { get; }

    public ArrowLaunchData(Vector3 launchPosition, Vector3 initialVelocity, float timeToReachTarget, Vector3 gravity)
    {
	      LaunchPosition = launchPosition;
        InitialVelocity = initialVelocity;
        TimeToReachTarget = timeToReachTarget;
        Gravity = gravity;
    }
}