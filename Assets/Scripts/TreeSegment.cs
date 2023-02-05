using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreeSegment : MonoBehaviour
{
    public LineRenderer line;

    public TreeObj tree;

    public TreePoint parent;

    public List<TreePoint> points = new List<TreePoint>();

    public int depth;
    public float growth;
    public float growthFactor = 1;

    private AnimationCurve curve;

    public void Init(TreePoint parent, float relativeAngleDeg = 0)
    {
        growthFactor = Random.Range(0.6f, 1.1f);

        this.parent = parent;
        this.depth = parent != null ? parent.segment.depth + parent.segment.points.IndexOf(parent) + 1 : 0;

        if (parent != null)
        {
            growth = Random.Range(-1f, 0f);
            transform.parent = parent.segment.transform;
            transform.localPosition = parent.pos;
            transform.localRotation = Quaternion.Euler(Vector3.forward * relativeAngleDeg);
        }
        else
        {
            growth = 0;
            transform.parent = tree.transform;
        }

        var subdivisions = tree.treeConfig.curveSubdivisions;
        line.positionCount = 1 + (tree.treeConfig.maxDepth - depth - 1) * subdivisions;

        var maxPoints = tree.treeConfig.maxDepth - depth;
        curve = new AnimationCurve();
        for (int i = 0; i <= maxPoints; i++)
        {
            var time = (float)i / maxPoints;
            if (curve.length <= i)
            {
                var result = curve.AddKey(time, 0);
            }
            else
            {
                curve.MoveKey(i, new Keyframe(time, 0));
            }
        }

        line.widthCurve = curve;

        if (points.Count < 1)
        {
            points.Add(new TreePoint()
            {
                segment = this,
                pos = new Vector2(0, 0),
            });
        }

        if (points.Count < 2)
        {
            AddPoint();
        }

        UpdateLine();
    }

    void Update()
    {
        bool maxDepthReached = depth + points.Count >= tree.treeConfig.maxDepth;
        if (maxDepthReached && growth >= 1)
        {
            growth = 1;
            return;
        }

        growth += Time.deltaTime * tree.treeConfig.growthSpeed * growthFactor;

        if (growth <= 1)
        {
            GrowAllPoints();
        }
    }

    public float ComputeGrowthScore()
    {
        bool maxDepthReached = depth + points.Count >= tree.treeConfig.maxDepth;
        if (maxDepthReached) return -1;

        return growth;
    }

    public bool GrowBranch()
    {
        // add next point
        bool maxDepthReached = depth + points.Count >= tree.treeConfig.maxDepth;
        if (maxDepthReached || growth < 1)
        {
            return false;
        }

        AddPoint();
        UpdateLine();

        // maybe branch out
        if (points.Count > 4 && Random.Range(0, 1f) < tree.treeConfig.branchingChance)
        {
            var branchingPoint = points[^4];
            branchingPoint.left = tree.CreateBranch(branchingPoint, true);
        }

        if (points.Count > 4 && Random.Range(0, 1f) < tree.treeConfig.branchingChance)
        {
            var branchingPoint = points[^4];
            branchingPoint.right = tree.CreateBranch(branchingPoint, false);
        }

        growth = 0;

        return true;
    }

    private void AddPoint()
    {
        float unstraightness = 1f - tree.treeConfig.branchStraightness;
        float wobbliness = Math.Min(tree.treeConfig.branchWobbliness, 1f) / 2f;

        var lengthFactor = Mathf.Pow(tree.treeConfig.segmentDecreaseFactor, depth + points.Count);
        var height = tree.treeConfig.segmentLength * lengthFactor * Random.Range(0.8f, 1.2f);

        var point = new TreePoint()
        {
            segment = this,
            pos = points[^1].pos + new Vector2(Random.Range(-unstraightness, unstraightness), height),
            bezierIntermediatePoint = points[^1].pos + new Vector2(
                Random.Range(-unstraightness * 2, unstraightness * 2),
                height * Random.Range(0.5f - wobbliness, 0.5f + wobbliness)),
        };
        points.Add(point);

        tree.OnBranchPointAdded(point);
    }

    private void GrowAllPoints()
    {
        var maxGrowth = tree.treeConfig.maxDepth - depth;
        var globalGrowth = points.Count - 2 + growth;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var k = curve.keys[i];
            k.value = (globalGrowth - i) * tree.treeConfig.thicknessFactor;
            curve.MoveKey(i, k);
            // curve.SmoothTangents(i, 1);
        }

        GrowLastPoint();

        line.widthCurve = curve;
    }

    private void GrowLastPoint()
    {
        var maxPoints = tree.treeConfig.maxDepth - depth;
        var time0 = (points.Count - 1) / (float)maxPoints;
        var time1 = (points.Count - 0) / (float)maxPoints;
        var time = Mathf.Lerp(time0, time1, growth);
        curve.MoveKey(points.Count - 1, new Keyframe(time, 0f, -2, 0));
    }

    private void UpdateLine()
    {
        // first point
        line.SetPosition(0, points[0].pos);

        // compute all points along the bezier curves
        var p0 = points[^2];
        var p1 = points[^1];
        var subdivisions = tree.treeConfig.curveSubdivisions;
        var lastIdx = 0;
        for (int i = 1; i <= subdivisions; i++)
        {
            var p = ComputePoint(p0.pos, p1.bezierIntermediatePoint, p1.pos, (i + 1f) / subdivisions);
            lastIdx = (points.Count - 2) * subdivisions + i;
            line.SetPosition(lastIdx, p);
        }

        var lastP = line.GetPosition(lastIdx);
        for (int i = lastIdx + 1; i < line.positionCount; i++)
        {
            line.SetPosition(i, lastP);
        }
    }

    public static Vector2 ComputePoint(Vector2 p0, Vector2 p1, Vector2 p2, float t)
    {
        t = Mathf.Clamp01(t);
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * p0 +
               2f * oneMinusT * t * p1 +
               t * t * p2;
    }
}