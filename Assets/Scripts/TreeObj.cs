using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeObj : MonoBehaviour
{
    public TreeConfig treeConfig;
    public RootsConfig rootsConfig;

    public TreeSegment overground;
    public RootSegment underground;

    public float resourceCycleTime = 5f;

    public float costRootWater = 1;
    public float costRootMinerals = 0.5f;
    public float costRootEnergy = 2;

    public float costBranchWater = 1;
    public float costBranchMinerals = 0.5f;
    public float costBranchEnergy = 2;

    public float gainRootWaterArea = 2f;
    public float gainRootMinerals = 1f;
    public float gainLeafEnergy = 1;
    public float gainLeafWater = -0.2f;

    public float maxWater = 5;
    public float maxEnergy = 5;
    public float maxMinerals = 2;

    public float water;
    public float energy;
    public float minerals;

    public float waterGain;
    public float energyGain;
    public float mineralsGain;

    [HideInInspector] public float timeToNextCycle = float.MaxValue;

    private readonly HashSet<ResourcePool> connectedResourcePools = new HashSet<ResourcePool>();

    void Start()
    {
        overground = CreateBranch(null, false);
        underground = CreateRoot(null, false);
    }

    void Update()
    {
        // get resources
        timeToNextCycle -= Time.deltaTime;
        if (timeToNextCycle <= 0)
        {
            timeToNextCycle = resourceCycleTime;

            // get root resources - maintenance costs
            float deltaWater = Mathf.Min(maxWater - water, waterGain);
            float deltaEnergy = Mathf.Min(maxEnergy - energy, energyGain);
            float deltaMinerals = Mathf.Min(maxMinerals - minerals, mineralsGain);

            foreach (var pool in connectedResourcePools.ToList())
            {
                bool empty = pool.Extract(ref deltaWater, ref deltaMinerals);
                if (empty)
                {
                    Debug.Log("resource pool " + pool + " is now empty");
                    connectedResourcePools.Remove(pool);
                    Destroy(pool.gameObject);
                }
            }

            water += deltaWater;
            energy += deltaEnergy;
            minerals += deltaMinerals;

            CheckForGameLost();

            // spend some for automatic growth
            int growthThisCycle = 0;
            var rootsCanItGrow = HowManyRootsCanItGrow();
            var branchesCanItGrow = HowManyBranchesCanItGrow();
            Debug.Log("check for growth: " + rootsCanItGrow + " - " + branchesCanItGrow + " - " +
                      WillNewBranchKeepGainsPositive());
            while (growthThisCycle <= 5 && branchesCanItGrow > 0 && rootsCanItGrow > 0 &&
                   WillNewBranchKeepGainsPositive())
            {
                GrowTree();
                growthThisCycle++;
                rootsCanItGrow = HowManyRootsCanItGrow();
                branchesCanItGrow = HowManyBranchesCanItGrow();
            }

            if (growthThisCycle > 0)
            {
                CheckForGameWon();
            }
        }
    }

    public void OnBranchPointAdded(TreePoint point)
    {
        waterGain += gainLeafWater;
        energyGain += gainLeafEnergy;
        maxWater += 0.5f;
    }

    public void OnRootPointAdded(RootPoint point)
    {
        SoundManager.INSTANCE.Play(SFX.ROOT_GROWTH);
        maxWater += 0.5f;
        maxEnergy += 0.5f;
        maxMinerals += 0.1f;

        var pos = point.GetWorldPos();
        var res = Physics2D.OverlapCircleAll(pos, 0.5f, GameManager.INSTANCE.resourcesLayerMask);
        foreach (var r in res)
        {
            var pool = r.gameObject.GetComponent<ResourcePool>();
            pool.waterExtraction += gainRootWaterArea;
            pool.mineralsExtraction += gainRootMinerals;
            waterGain += gainRootWaterArea;
            mineralsGain += gainRootMinerals;
            connectedResourcePools.Add(pool);
        }

        if (res.Length > 0) SoundManager.INSTANCE.Play(SFX.HIT_RESOURCES);
    }

    private void GrowTree()
    {
        var result = FindBestBranchToGrow(overground);
        if (result.Item2 < 1) return;

        water -= costBranchWater;
        energy -= costBranchEnergy;
        minerals -= costBranchMinerals;
        result.Item1.GrowBranch();

        SoundManager.INSTANCE.Play(SFX.BRANCH_GROWTH);
    }

    private Tuple<TreeSegment, float> FindBestBranchToGrow(TreeSegment branch)
    {
        float score = branch.ComputeGrowthScore();
        TreeSegment resultBranch = branch;

        foreach (var point in branch.points)
        {
            if (point.left)
            {
                var t = FindBestBranchToGrow(point.left);
                if (t.Item2 > score)
                {
                    score = t.Item2;
                    resultBranch = t.Item1;
                }
            }

            if (point.right)
            {
                var t = FindBestBranchToGrow(point.right);
                if (t.Item2 > score)
                {
                    score = t.Item2;
                    resultBranch = t.Item1;
                }
            }
        }

        return Tuple.Create(resultBranch, score);
    }

    private void CheckForGameLost()
    {
        if (HowManyRootsCanItGrow() == 0)
        {
            GameManager.INSTANCE.GameOverLost();
        }
    }

    private void CheckForGameWon()
    {
    }

    public bool WillNewBranchKeepGainsPositive()
    {
        return waterGain > Math.Abs(gainLeafWater);
    }

    public int HowManyBranchesCanItGrow()
    {
        var w = Mathf.FloorToInt(water / costBranchWater);
        var e = Mathf.FloorToInt(energy / costBranchEnergy);
        var m = Mathf.FloorToInt(minerals / costBranchMinerals);
        return Mathf.Min(w, e, m);
    }

    public int HowManyRootsCanItGrow()
    {
        var w = Mathf.FloorToInt(water / costRootWater);
        var e = Mathf.FloorToInt(energy / costRootEnergy);
        var m = Mathf.FloorToInt(minerals / costRootMinerals);
        return Mathf.Min(w, e, m);
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
        var localMousePos = (Vector2)parent.segment.transform.InverseTransformPoint(mousePos);
        var localDir = localMousePos - parent.pos;
        float angleInDeg = Vector2.SignedAngle(Vector2.up, localDir);

        var obj = Instantiate(rootsConfig.segmentPrefab);
        var seg = obj.GetComponent<RootSegment>();
        seg.tree = this;
        seg.Init(parent, angleInDeg);

        return seg;
    }
}