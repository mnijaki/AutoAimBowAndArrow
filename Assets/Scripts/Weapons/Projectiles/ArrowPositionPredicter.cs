using System.Collections.Generic;
using UnityEngine;

public class ArrowPositionPredicter : MonoBehaviour
{
    [SerializeField]
    private int _numberOfPointsToDraw = 30;

    [SerializeField]
    private GameObject _pointPrefab;

    private List<GameObject> _instantiatedPoints;

    private void Awake()
    {
        _instantiatedPoints = new List<GameObject>();
    }

    public void DrawPredictedArrowPositions(ArrowLaunchData arrowLaunchData)
    {
        RemovePredictedArrowPositions();
        SpawnPredictionPoint(arrowLaunchData.LaunchPosition);
        for(int i = 0; i <= _numberOfPointsToDraw; i++)
        {
            float drawProgress = (float)i / _numberOfPointsToDraw;
            float currentStepSimulationTime = drawProgress * arrowLaunchData.TimeToReachTarget;
            Vector3 displacement = CalculateDisplacement(arrowLaunchData, currentStepSimulationTime);
            Vector3 position = arrowLaunchData.LaunchPosition + displacement;
            SpawnPredictionPoint(position);
        }
    }

    private static Vector3 CalculateDisplacement(ArrowLaunchData arrowLaunchData, float currentStepSimulationTime)
    {
        // Displacement based on one of the SUVAT kinematic equations:
        // s = u*t + ((a*t*t)/2)
        // See video made by Sebastian League (you cant watch all 3 videos if you want to know more).
        // https://www.youtube.com/watch?v=IvT8hjy6q4o&list=PLFt_AvWsXl0eMryeweK7gc9T04lJCIg_W&index=3
        
        Vector3 displacement = arrowLaunchData.InitialVelocity * currentStepSimulationTime +
                               ((arrowLaunchData.Gravity * currentStepSimulationTime * currentStepSimulationTime) / 2);
        return displacement;
    }

    private void SpawnPredictionPoint(Vector3 position)
    {
        GameObject point = Instantiate(_pointPrefab, position, Quaternion.identity);
        _instantiatedPoints.Add(point);
    }

    private void RemovePredictedArrowPositions()
    {
        for(int i = _instantiatedPoints.Count - 1; i > -1; i--)
        {
            Destroy(_instantiatedPoints[i]);
        }
    }
}
