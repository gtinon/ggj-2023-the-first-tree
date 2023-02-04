using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TreeConfig", menuName = "", order = 1)]
public class TreeConfig : ScriptableObject
{
    [FormerlySerializedAs("treeSegmentPrefab")] public GameObject overgroundSegmentPrefab;

    public float segmentLength = 1;
    public float segmentDecreaseFactor = 0.95f;
    public float branchStraightness = 1f;
    public float branchWobbliness = 0.4f;
    public int curveSubdivisions = 6;

    public float thicknessFactor = 1;
    
    public float branchingAngleDeg = 20;
    public float branchingAngleVariance = 0f;
    public float branchingChance = 1;
    public bool alternateBranchingSide = false;

    public float growthSpeed = 1;
    public int maxDepth = 10;
}