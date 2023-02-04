using System;
using UnityEngine;

[Serializable]
public class TreePoint
{
    public Vector2 pos;
    public Vector2 bezierIntermediatePoint;

    public TreeSegment segment;
    public TreeSegment left;
    public TreeSegment right;
}