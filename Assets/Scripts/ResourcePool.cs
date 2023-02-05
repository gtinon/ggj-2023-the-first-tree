using UnityEngine;

public class ResourcePool : MonoBehaviour
{
    public float water;
    public float minerals;

    [HideInInspector] public float waterExtraction;
    [HideInInspector] public float mineralsExtraction;

    public bool Extract(ref float deltaWater, ref float deltaMinerals)
    {
        var waterExtracted = Mathf.Min(waterExtraction, deltaWater, water);
        deltaWater -= waterExtracted;
        water -= waterExtracted;

        var mineralsExtracted = Mathf.Min(mineralsExtraction, deltaMinerals, minerals);
        deltaMinerals -= mineralsExtracted;
        minerals -= mineralsExtracted;

        return (water <= 0 && minerals <= 0);
    }
}