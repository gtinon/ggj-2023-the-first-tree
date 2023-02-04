using UnityEngine;

public class GameManager : MonoBehaviour
{
    public RootJointMarker rootJointMarker;
    public LineRenderer rootGrowthLine;

    public float mouseMaxContactRadius = 3;
    public float mouseMinContactRadius = 1.5f;

    private readonly Collider2D[] results = new Collider2D[50];
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        rootJointMarker.gameObject.SetActive(false);
        rootGrowthLine.gameObject.SetActive(false);
    }

    void Update()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        var node = FindRootNodeToExtend(mousePos);

        if (node && Input.GetMouseButtonDown(0))
        {
            var rootPoint = node.rootPoint;
            if (rootPoint.segment.points.IndexOf(rootPoint) == rootPoint.segment.points.Count - 1)
            {
                // just extend the root
                var worldPos = mousePos - rootPoint.segment.transform.position;
                var localPos = rootPoint.segment.transform.InverseTransformVector(worldPos);
                rootPoint.segment.AddPoint(localPos.x, localPos.y);
                Debug.Log("extend current root");
            }
            else
            {
                // fork
                Debug.Log("forked root " + rootPoint.segment + " at index " +
                          rootPoint.segment.points.IndexOf(rootPoint));
                rootPoint.left = rootPoint.segment.tree.CreateRoot(rootPoint, true, mousePos);
            }
        }
        else if (node)
        {
            var nodePos = node.transform.position;

            rootJointMarker.gameObject.SetActive(true);
            rootGrowthLine.gameObject.SetActive(true);
            rootJointMarker.transform.position = nodePos;
            rootGrowthLine.SetPosition(0, nodePos);
            rootGrowthLine.SetPosition(1, mousePos);
        }
        else
        {
            rootJointMarker.gameObject.SetActive(false);
            rootGrowthLine.gameObject.SetActive(false);
        }
    }

    private RootNodeCollider FindRootNodeToExtend(Vector3 mousePos)
    {
        if (mousePos.y > 1) return null;

        int resultCount = Physics2D.OverlapCircleNonAlloc(mousePos, mouseMaxContactRadius, results);

        int minIndex = -1;
        float minValue = float.MaxValue;
        for (int i = 0; i < resultCount; i++)
        {
            var obj = results[i];
            var dist = (mousePos - obj.transform.position).magnitude;
            if (dist < minValue)
            {
                minValue = dist;
                minIndex = i;
            }
        }

        RootNodeCollider node = null;
        if (minIndex >= 0)
        {
            var nodeCollider = results[minIndex];
            node = nodeCollider.gameObject.GetComponent<RootNodeCollider>();

            // too close to the closest root node, cannot grow there
            if (minValue < mouseMinContactRadius)
            {
                return null;
            }

            var rootPoint = node.rootPoint;
            if (rootPoint.left || rootPoint.right) return null;

            // to be able to extend a root from its last point
            // you need growth to be done
            // and some available slots in the segment
            if (rootPoint.segment.points.IndexOf(rootPoint) == rootPoint.segment.points.Count - 1)
            {
                if (rootPoint.segment.growth < 1)
                {
                    return null;
                }

                var segmentDepth = rootPoint.segment.depth + rootPoint.segment.points.Count;
                bool maxDepthReached = segmentDepth >= rootPoint.segment.tree.rootsConfig.maxDepth;
                if (maxDepthReached)
                {
                    return null;
                }
            }
        }

        return node;
    }
}