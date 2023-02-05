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

    private AnimationCurve curve = new AnimationCurve();

    public void Init(RootPoint parent, float relativeAngleDeg = 0)
    {
        growthFactor = Random.Range(0.6f, 1.1f);
        growth = 0;

        this.parent = parent;
        this.depth = parent != null ? parent.segment.depth + parent.segment.points.IndexOf(parent) + 1 : 0;

        if (parent != null)
        {
            transform.parent = parent.segment.transform;
            transform.localPosition = parent.pos;
            transform.localRotation = Quaternion.Euler(Vector3.forward * relativeAngleDeg);
        }
        else
        {
            transform.parent = tree.transform;
            transform.position = tree.transform.position;
            transform.rotation = Quaternion.Euler(0, 0, relativeAngleDeg);
        }

        if (points.Count < 1)
        {
            points.Add(new RootPoint()
            {
                segment = this,
                pos = new Vector2(0, 0),
                width = 1,
            });
        }

        if (points.Count < 2)
        {
            AddPoint();
        }

        line.widthCurve = curve;

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
            var deltaGrowth = Math.Min(Time.deltaTime * tree.rootsConfig.growthSpeed * growthFactor, 1 - growth);
            growth += deltaGrowth;
            GrowAllPointsBefore(points.Count - 1, deltaGrowth);
        }
    }

    private void AddPoint()
    {
        this.AddPoint(0, 0, false);
    }

    public void AddPoint(float x, float y, bool withResources = true)
    {
        float wobbliness = Math.Min(tree.rootsConfig.branchWobbliness, 1f) / 2f;

        var height = tree.rootsConfig.segmentLength * Random.Range(0.8f, 1.2f);

        Vector2 pos;
        var parentP = points[^1];
        if (x != 0 && y != 0)
        {
            pos = new Vector2(x, y);
        }
        else
        {
            pos = parentP.pos + new Vector2(0, height);
        }

        var intermediateV = pos - parentP.pos;
        var space = intermediateV.magnitude;
        intermediateV = RotateDegs(intermediateV.normalized, Random.Range(-90 * wobbliness, 90 * wobbliness));
        var s = Random.Range(space / 4, space * 3 / 4);
        intermediateV.Scale(new Vector2(s, s));

        var rootPoint = new RootPoint()
        {
            segment = this,
            pos = pos,
            bezierIntermediatePoint = parentP.pos + intermediateV,
            width = 0f,
        };
        points.Add(rootPoint);

        // add light
        var light = Instantiate(tree.rootsConfig.rootPointLight, this.transform);
        light.transform.localPosition = parentP.pos +
                                        new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f));

        // add child node with collider
        var colliderObj = Instantiate(tree.rootsConfig.rootNodeCollider, this.transform);
        colliderObj.transform.localPosition = rootPoint.pos;
        var rootNodeCollider = colliderObj.GetComponent<RootNodeCollider>();
        rootNodeCollider.rootPoint = rootPoint;

        UpdateLine();
        growth = 0;

        tree.OnRootPointAdded(rootPoint);
    }

    public static Vector2 RotateDegs(Vector2 v, float degs)
    {
        return RotateRads(v, degs * Mathf.Deg2Rad);
    }

    public static Vector2 RotateRads(Vector2 v, float rads)
    {
        return new Vector2(
            v.x * Mathf.Cos(rads) - v.y * Mathf.Sin(rads),
            v.x * Mathf.Sin(rads) + v.y * Mathf.Cos(rads)
        );
    }

    private void GrowAllPointsBefore(int idx, float g)
    {
        for (int i = 0; i <= idx; i++)
        {
            points[i].width += g;
        }

        UpdateLineWidth();

        if (parent != null)
        {
            var indexInParent = parent.segment.points.IndexOf(parent);
            parent.segment.GrowAllPointsBefore(indexInParent, g);
        }
    }

    private void UpdateLine()
    {
        var subdivisions = tree.rootsConfig.curveSubdivisions;
        int expectedPointCount = 1 + (points.Count - 1) * subdivisions;

        // resize line if necessary
        line.positionCount = expectedPointCount;

        // update all points along the bezier curves
        line.SetPosition(0, points[0].pos);
        for (int i = 1; i < points.Count; i++)
        {
            var p0 = points[i - 1];
            var p1 = points[i];
            for (int j = 0; j <= subdivisions; j++)
            {
                var p = ComputePoint(p0.pos, p1.bezierIntermediatePoint, p1.pos, (float)j / subdivisions);
                var idx = (i - 1) * subdivisions + j;
                line.SetPosition(idx, p);
            }
        }

        UpdateLineWidth();
    }

    private void UpdateLineWidth()
    {
        // update point widths
        for (int i = 0; i < points.Count; i++)
        {
            var p = points[i];
            var t = i / (points.Count - 1f);
            var w = p.width <= 0 ? 0.1f : Mathf.Log10(p.width) / 2f;
            if (curve.length <= i)
            {
                curve.AddKey(new Keyframe(t, w, 0, 0));
            }
            else
            {
                var k = curve[i];
                k.time = t;
                k.value = w;
                curve.MoveKey(i, k);
            }
        }

        // handle last point
        var time0 = (points.Count - 2) / (points.Count - 1f);
        var lastK = curve[points.Count - 1];
        lastK.time = Mathf.Lerp(time0, 1, growth);
        lastK.value = 0;
        lastK.inTangent = 0f;
        curve.MoveKey(points.Count - 1, lastK);

        line.widthCurve = curve;
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