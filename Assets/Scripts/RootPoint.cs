using System;
using UnityEngine;

[Serializable]
public class RootPoint
{
    public Vector2 pos;
    public Vector2 bezierIntermediatePoint;

    public RootSegment segment;
    public RootSegment left;
    public RootSegment right;
    public float width;

    public Vector3 GetWorldPos()
    {
        return segment.transform.TransformPoint(pos);
    }
}