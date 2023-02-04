using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RootSegment : MonoBehaviour
{
    public LineRenderer line;

    public TreeObj tree;

    public RootPoint parent;

    public List<RootPoint> points = new List<RootPoint>();

    public int depth;
    public float growth;
    public float growthFactor = 1;

    private AnimationCurve curve;

    public void Init(RootPoint parent, float relativeAngleDeg = 0)
    {
        growthFactor = Random.Range(0.6f, 1.1f);
        growth = 0;

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
            transform.rotation = Quaternion.Euler(0, 0, relativeAngleDeg);
        }

        var subdivisions = tree.rootsConfig.curveSubdivisions;
        line.positionCount = 1 + (tree.rootsConfig.maxDepth - depth - 1) * subdivisions;

        var maxPoints = tree.rootsConfig.maxDepth - depth;
        curve = new AnimationCurve();
        for (int i = 0; i <= maxPoints; i++)
        {
            var time = (float)i / maxPoints;
            if (curve.length <= i)
            {
                var result = curve.AddKey(time, 0.001f * i);
            }
            else
            {
                curve.MoveKey(i, new Keyframe(time, 0.001f * i));
            }
        }

        line.widthCurve = curve;

        if (points.Count < 1)
        {
            points.Add(new RootPoint()
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
        bool maxDepthReached = depth + points.Count >= tree.rootsConfig.maxDepth;
        if (maxDepthReached && growth > 1)
        {
            return;
        }

        if (growth < 1)
        {
            growth += Time.deltaTime * tree.rootsConfig.growthSpeed * growthFactor;
            if (growth > 1)
            {
                growth = 1;
            }
        }

        GrowAllPoints();
    }

    private void AddPoint()
    {
        this.AddPoint(0, 0);
    }

    public void AddPoint(float x, float y)
    {
        float wobbliness = Math.Min(tree.rootsConfig.branchWobbliness, 1f) / 2f;

        var height = tree.rootsConfig.segmentLength * Random.Range(0.8f, 1.2f);

        Vector2 pos;
        if (x != 0 && y != 0)
        {
            pos = new Vector2(x, y);
        }
        else
        {
            pos = points[^1].pos + new Vector2(0, height);
        }

        var rootPoint = new RootPoint()
        {
            segment = this,
            pos = pos,
            bezierIntermediatePoint = points[^1].pos + new Vector2(
                Random.Range(-wobbliness, wobbliness),
                height * Random.Range(0.5f - wobbliness, 0.5f + wobbliness)),
        };
        points.Add(rootPoint);

        // add light
        var light = Instantiate(tree.rootsConfig.rootPointLight, this.transform);
        light.transform.localPosition = points[^2].pos +
                                        new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));

        // add child node with collider
        var colliderObj = Instantiate(tree.rootsConfig.rootNodeCollider, this.transform);
        var rootNodeCollider = colliderObj.GetComponent<RootNodeCollider>();
        rootNodeCollider.rootPoint = rootPoint;
        colliderObj.transform.localPosition = points[^1].pos;

        UpdateLine();
        growth = 0;
    }

    private void GrowAllPoints()
    {
        var maxGrowth = tree.rootsConfig.maxDepth - depth;
        var globalGrowth = points.Count - 2 + growth;

        for (int i = 0; i < points.Count - 1; i++)
        {
            var k = curve.keys[i];
            k.value = (globalGrowth - i) * tree.rootsConfig.thicknessFactor;
            curve.MoveKey(i, k);
            // curve.SmoothTangents(i, 1);
        }

        GrowLastPoint();

        line.widthCurve = curve;
    }

    private void GrowLastPoint()
    {
        var maxPoints = tree.rootsConfig.maxDepth - depth;
        var time0 = (points.Count - 1) / (float)maxPoints;
        var time1 = (points.Count - 0) / (float)maxPoints;
        var time = Mathf.Lerp(time0, time1, growth);
        var k = curve[points.Count - 1];
        k.time = 0;
        curve.MoveKey(points.Count - 1, new Keyframe(time, 0f, -2, 0));
    }

    private void UpdateLine()
    {
        // first point
        line.SetPosition(0, points[0].pos);

        // compute all points along the bezier curves
        var p0 = points[^2];
        var p1 = points[^1];
        var subdivisions = tree.rootsConfig.curveSubdivisions;
        var lastIdx = 0;
        for (int i = 1; i <= subdivisions; i++)
        {
            var p = ComputePoint(p0.pos, p1.bezierIntermediatePoint, p1.pos, (i + 1f) / subdivisions);
            lastIdx = (points.Count - 2) * subdivisions + i;
            line.SetPosition(lastIdx, p);
        }

        var initI = lastIdx;
        var lastP = line.GetPosition(lastIdx);
        for (int i = lastIdx + 1; i < line.positionCount; i++)
        {
            line.SetPosition(i, lastP + new Vector3(0, 0.2f * (i - initI), 0));
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