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

    private AnimationCurve curve;

    public void Init(TreePoint parent, float relativeAngleDeg = 0)
    {
        this.parent = parent;
        this.depth = parent != null ? parent.segment.depth + parent.segment.points.IndexOf(parent) + 1 : 0;

        if (parent != null)
        {
            transform.parent = parent.segment.transform;
            transform.position = parent.segment.transform.position +
                                 parent.segment.transform.TransformVector(new Vector3(parent.pos.x, parent.pos.y, 0));
            transform.rotation =
                parent.segment.transform.rotation * Quaternion.Euler(Vector3.forward * relativeAngleDeg);
        }
        else
        {
            transform.parent = tree.transform;
            transform.position = tree.transform.position;
        }

        var subdivisions = tree.config.curveSubdivisions;
        line.positionCount = 1 + (tree.config.maxDepth - depth - 1) * subdivisions;

        var maxPoints = tree.config.maxDepth - depth;
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
        bool maxDepthReached = depth + points.Count >= tree.config.maxDepth;
        if (maxDepthReached && growth > 1)
        {
            return;
        }

        growth += Time.deltaTime * tree.config.growthSpeed;

        // add next point
        if (!maxDepthReached && growth >= 1)
        {
            AddPoint();
            UpdateLine();

            // maybe branch out
            if (points.Count > 4 && Random.Range(0, 1f) < tree.config.branchingChance)
            {
                var branchingPoint = points[^4];
                Debug.Log("branching at=" + branchingPoint.pos);
                branchingPoint.left = tree.CreateBranch(branchingPoint, true);
                branchingPoint.right = tree.CreateBranch(branchingPoint, false);
            }

            growth = 0;
        }

        GrowAllPoints();
    }

    private void AddPoint()
    {
        float unstraightness = 1f - tree.config.branchStraightness;
        float wobbliness = Math.Min(tree.config.branchWobbliness, 1f) / 2f;

        var lengthFactor = Mathf.Pow(tree.config.segmentDecreaseFactor, depth + points.Count);
        var height = tree.config.segmentLength * lengthFactor * Random.Range(0.8f, 1.2f);

        points.Add(new TreePoint()
        {
            segment = this,
            pos = points[^1].pos + new Vector2(Random.Range(-unstraightness, unstraightness), height),
            bezierIntermediatePoint = points[^1].pos + new Vector2(
                Random.Range(-unstraightness * 2, unstraightness * 2),
                height * Random.Range(0.5f - wobbliness, 0.5f + wobbliness)),
        });
    }

    private void GrowAllPoints()
    {
        var maxGrowth = tree.config.maxDepth - depth;
        var globalGrowth = points.Count - 2 + growth;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var k = curve.keys[i];
            k.value = (globalGrowth - i) * tree.config.thicknessFactor;
            curve.MoveKey(i, k);
            // curve.SmoothTangents(i, 1);
        }

        GrowLastPoint();

        line.widthCurve = curve;
    }

    private void GrowLastPoint()
    {
        var maxPoints = tree.config.maxDepth - depth;
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
        var subdivisions = tree.config.curveSubdivisions;
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

[Serializable]
public class TreePoint
{
    public Vector2 pos;
    public Vector2 bezierIntermediatePoint;

    public TreeSegment segment;
    public TreeSegment left;
    public TreeSegment right;
}