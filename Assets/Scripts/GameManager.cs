using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager INSTANCE;

    public RootJointMarker rootJointMarker;
    public LineRenderer rootGrowthLine;

    public RectTransform introPanel;
    public RectTransform helpPanel;
    public RectTransform defeatPanel;
    public RectTransform victoryPanel;
    public TreeObj tree;

    public float mouseMaxContactRadius = 3;
    public float mouseMinContactRadius = 1.5f;

    private readonly Collider2D[] results = new Collider2D[50];
    private Camera cam;

    public int rootNodesLayerMask;
    public int rocksLayerMask;
    public int resourcesLayerMask;

    private bool gameOver;

    public bool GamePaused { get; private set; }

    void Awake()
    {
        INSTANCE = this;
    }

    void Start()
    {
        cam = Camera.main;
        rootNodesLayerMask = 1 << LayerMask.NameToLayer("RootNodes");
        rocksLayerMask = 1 << LayerMask.NameToLayer("Rocks");
        resourcesLayerMask = 1 << LayerMask.NameToLayer("Resources");
        rootJointMarker.gameObject.SetActive(false);
        rootGrowthLine.gameObject.SetActive(false);

        helpPanel.gameObject.SetActive(false);
        defeatPanel.gameObject.SetActive(false);
        victoryPanel.gameObject.SetActive(false);

        introPanel.gameObject.SetActive(true);
    }

    void Update()
    {
        HandleUserInput();

        if (!gameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            GamePaused = !GamePaused;
            Time.timeScale = GamePaused ? 0 : 1;
            helpPanel.gameObject.SetActive(GamePaused);
        }

        if (gameOver && (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return)))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Time.timeScale = 1;
        }
    }

    public void StartGame()
    {
        if (tree.timeToNextResourceCycle > 10)
        {
            SoundManager.INSTANCE.Play(SFX.GAME_START);
            SoundManager.INSTANCE.StartBGM();
            tree.timeToNextResourceCycle = tree.resourceCycleTime;
            tree.timeToNextGrowthCycle = tree.resourceCycleTime * Random.Range(1.3f, 1.7f);

            introPanel.gameObject.SetActive(false);
        }
    }

    public void GameOverWon()
    {
        SoundManager.INSTANCE.Play(SFX.GAME_OVER_VICTORY);
        gameOver = true;
        victoryPanel.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    public void GameOverLost()
    {
        SoundManager.INSTANCE.Play(SFX.GAME_OVER_DEFEAT);
        gameOver = true;
        defeatPanel.gameObject.SetActive(true);
        Time.timeScale = 0;
    }

    private void HandleUserInput()
    {
        if (GamePaused)
        {
            rootJointMarker.gameObject.SetActive(false);
            rootGrowthLine.gameObject.SetActive(false);
            return;
        }

        var mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        var node = FindRootNodeToExtend(mousePos);

        if (node && Input.GetMouseButtonDown(0))
        {
            // start the game if necessary
            StartGame();

            if (!tree.CanGrowRoot())
            {
                ResourceManager.INSTANCE.Highlight();
                return;
            }

            tree.ConsumeRootResources();

            var rootPoint = node.rootPoint;
            if (rootPoint.segment.points.IndexOf(rootPoint) == rootPoint.segment.points.Count - 1)
            {
                // just extend the root
                var localPos = rootPoint.segment.transform.InverseTransformPoint(mousePos);
                rootPoint.segment.AddPoint(localPos.x, localPos.y);
                // Debug.Log("extend current root");
            }
            else
            {
                // fork
                // Debug.Log("forked root " + rootPoint.segment + " at index " +
                // rootPoint.segment.points.IndexOf(rootPoint));
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
                    SoundManager.INSTANCE.Play(SFX.HIT_ROCK);
                    return null;
                }
            }
        }

        return node;
    }

    public static void Quit()
    {
        Application.Quit();
    }
}