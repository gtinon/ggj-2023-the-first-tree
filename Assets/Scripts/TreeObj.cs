using UnityEngine;

public class TreeObj : MonoBehaviour
{
    public TreeConfig treeConfig;
    public RootsConfig rootsConfig;

    public TreeSegment overground;
    public RootSegment underground;

    public float water;
    public float energy;
    public float minerals;

    void Start()
    {
        overground = CreateBranch(null, false);
        underground = CreateRoot(null, false);
    }

    void Update()
    {
    }

    public TreeSegment CreateBranch(TreePoint parent, bool left)
    {
        var obj = Instantiate(treeConfig.segmentPrefab);
        var seg = obj.GetComponent<TreeSegment>();
        seg.tree = this;

        var relativeAngle = treeConfig.branchingAngleDeg +
                            Random.Range(-treeConfig.branchingAngleVariance, treeConfig.branchingAngleVariance);
        relativeAngle = relativeAngle * (left ? 1 : -1);
        seg.Init(parent, relativeAngle);

        return seg;
    }

    public RootSegment CreateRoot(RootPoint parent, bool left)
    {
        var obj = Instantiate(rootsConfig.segmentPrefab);
        var seg = obj.GetComponent<RootSegment>();
        seg.tree = this;

        float relativeAngle;
        if (parent == null)
        {
            relativeAngle = 180;
        }
        else
        {
            relativeAngle = treeConfig.branchingAngleDeg +
                            Random.Range(-treeConfig.branchingAngleVariance, treeConfig.branchingAngleVariance);
            relativeAngle *= (left ? 1 : -1);
        }

        seg.Init(parent, relativeAngle);

        return seg;
    }

    public RootSegment CreateRoot(RootPoint parent, bool left, Vector3 mousePos)
    {
        var obj = Instantiate(rootsConfig.segmentPrefab);
        var seg = obj.GetComponent<RootSegment>();
        seg.tree = this;

        var dir = mousePos - parent.segment.transform.TransformVector(parent.pos);
        Debug.Log("world dir=" + dir);
        dir = parent.segment.transform.InverseTransformVector(dir);
        Debug.Log("local dir=" + dir);
        float angleInDeg = Vector2.SignedAngle(Vector2.up, dir);
        Debug.Log("angle deg=" + angleInDeg);
        seg.Init(parent, angleInDeg);

        return seg;
    }
}