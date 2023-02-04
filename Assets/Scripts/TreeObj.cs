using UnityEngine;

public class TreeObj : MonoBehaviour
{
    public TreeConfig config;

    public TreeSegment overground;
    public TreeSegment underground;

    public float water;
    public float energy;
    public float minerals;

    void Start()
    {
        overground = CreateBranch(null, false);
    }

    void Update()
    {
    }

    public TreeSegment CreateBranch(TreePoint parent, bool left)
    {
        var obj = Instantiate(config.overgroundSegmentPrefab);
        var seg = obj.GetComponent<TreeSegment>();
        seg.tree = this;

        var relativeAngle = config.branchingAngleDeg +
                            Random.Range(-config.branchingAngleVariance, config.branchingAngleVariance);
        relativeAngle = relativeAngle * (left ? 1 : -1);
        seg.Init(parent, relativeAngle);

        return seg;
    }
}