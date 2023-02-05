using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;

    public RootJointMarker rootJointMarker;
    public LineRenderer rootGrowthLine;

    public TreeObj theTree;

    public float mouseMaxContactRadius = 3;
    public float mouseMinContactRadius = 1.5f;

    private readonly Collider2D[] results = new Collider2D[50];
    private Camera cam;

    public int rootNodesLayerMask;
    public int rocksLayerMask;
    public int resourcesLayerMask;

    void Awake()
    {
        INSTANCE = this;

        Debug.Log(Vector2.SignedAngle(Vector2.right, new Vector3(0, 1, -10)));
        Debug.Log(Vector2.SignedAngle(Vector2.right, new Vector3(0, -1, -10)));
        Debug.Log(Vector2.SignedAngle(Vector2.right, new Vector3(1, 1, -10)));
    }

    void Start()
    {
        cam = Camera.main;
        rootNodesLayerMask = 1 << LayerMask.NameToLayer("RootNodes");
        rocksLayerMask = 1 << LayerMask.NameToLayer("Rocks");
        resourcesLayerMask = 1 << LayerMask.NameToLayer("Resources");
        rootJointMarker.gameObject.SetActive(false);
        rootGrowthLine.gameObject.SetActive(false);
    }

    void Update()
    {
        HandleUserInput();
    }

    public void GameOverWon()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GameOverLost()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void HandleUserInput()
    {
        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        var node = FindRootNodeToExtend(mousePos);

        if (node && Input.GetMouseButtonDown(0))
        {
            var rootPoint = node.rootPoint;
            if (rootPoint.segment.points.IndexOf(rootPoint) == rootPoint.segment.points.Count - 1)
            {
                // just extend the root
                var localPos = rootPoint.segment.transform.InverseTransformPoint(mousePos);
                rootPoint.segment.AddPoint(localPos.x, localPos.y);
                Debug.Log("extend current root");
            }
            else
            {
                // fork
                Debug.Log("forked root " + rootPoint.segment + " at index " +
                          rootPoint.segment.points.IndexOf(rootPoint));
                var newSegment = rootPoint.segment.tree.CreateRoot(rootPoint, true, mousePos);
                if (rootPoint.left)
                {
                    rootPoint.right = newSegment;
                }
                else
                {
                    rootPoint.left = newSegment;
                }
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

        int resultCount = Physics2D.OverlapCircleNonAlloc(mousePos, mouseMaxContactRadius, results, rootNodesLayerMask);

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
            if (rootPoint.left && rootPoint.right) return null;

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

            if (Input.GetMouseButtonDown(0))
            {
                // no rocks allowed on the path
                var rock = Physics2D.OverlapCircle(mousePos, 0.2f, rocksLayerMask);
                if (rock)
                {
                    // TODO play sound
                    return null;
                }
            }
        }

        return node;
    }
}