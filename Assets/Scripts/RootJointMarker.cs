using UnityEngine;

public class RootJointMarker : MonoBehaviour
{
    public float rotationSpeed = 1;
    public float pulseSpeed = 1;
    public float pulseScaling = 0.5f;

    private SpriteRenderer spriteRenderer;

    private Vector3 baseLocaleScale;

    void Start()
    {
        baseLocaleScale = transform.localScale;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        spriteRenderer.transform.Rotate(0, 0, Time.deltaTime * rotationSpeed);
        spriteRenderer.transform.localScale = baseLocaleScale * (1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScaling);
    }
}