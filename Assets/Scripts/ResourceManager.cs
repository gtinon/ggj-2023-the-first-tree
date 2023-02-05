using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager INSTANCE;

    public TMP_Text waterStockField;
    public TMP_Text waterGainField;
    public TMP_Text mineralsStockField;
    public TMP_Text mineralsGainField;
    public TMP_Text energyStockField;
    public TMP_Text energyGainField;

    public TreeObj tree;

    private float unhighlightLerpSpeed = 0.02f;

    void Awake()
    {
        INSTANCE = this;
    }

    void Update()
    {
        waterStockField.color = Color.Lerp(waterStockField.color, Color.white, unhighlightLerpSpeed);
        mineralsStockField.color = Color.Lerp(waterStockField.color, Color.white, unhighlightLerpSpeed);
        energyStockField.color = Color.Lerp(waterStockField.color, Color.white, unhighlightLerpSpeed);
    }

    void FixedUpdate()
    {
        waterStockField.text = round(tree.water) + " / " + round(tree.maxWater);
        mineralsStockField.text = round(tree.minerals) + " / " + round(tree.maxMinerals);
        energyStockField.text = round(tree.energy) + " / " + round(tree.maxEnergy);

        waterGainField.text = round(tree.waterGain, true);
        mineralsGainField.text = round(tree.mineralsGain, true);
        energyGainField.text = round(tree.energyGain, true);
    }

    private string round(float v, bool withSign = false)
    {
        var roundedV = (Mathf.Floor(v * 10) / 10);
        var prefix = withSign && v > 0 ? "+" : "";
        return prefix + roundedV;
    }

    public void Highlight()
    {
        waterStockField.color = Color.red;
    }
}