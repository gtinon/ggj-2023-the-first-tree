using UnityEngine;

public class RootNodeCollider : MonoBehaviour
{
    public RootPoint rootPoint;
    public Collider2D nodeCollider;

    void Start()
    {
        nodeCollider = GetComponent<Collider2D>();
    }
}