using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TreeConfig", menuName = "", order = 1)]
public class TreeConfig : SegmentConfig
{

    public float segmentDecreaseFactor = 0.95f;
    public float branchStraightness = 1f;
    
    public float branchingAngleDeg = 20;
    public float branchingAngleVariance = 0f;
    public float branchingChance = 1;
    public bool alternateBranchingSide = false;

}
