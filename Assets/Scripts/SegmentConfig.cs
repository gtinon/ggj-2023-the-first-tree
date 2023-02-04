using UnityEngine;

public abstract class SegmentConfig : ScriptableObject
{
    public GameObject segmentPrefab;

    public float segmentLength = 1;
    public float branchWobbliness = 0.4f;
    public int curveSubdivisions = 6;

    public float thicknessFactor = 1;
    
    public float growthSpeed = 1;
    public int maxDepth = 10;
}