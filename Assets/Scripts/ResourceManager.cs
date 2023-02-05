using TMPro;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    public TMP_Text waterStockField;
    public TMP_Text waterGainField;
    public TMP_Text mineralsStockField;
    public TMP_Text mineralsGainField;
    public TMP_Text energyStockField;
    public TMP_Text energyGainField;

    public TreeObj tree;

    void Start()
    {
    }

    void FixedUpdate()
    {
        waterStockField.text = round(tree.water) + " / " + round(tree.maxWater);
        mineralsStockField.text = round(tree.minerals) + " / " + round(tree.maxMinerals);
        energyStockField.text = round(tree.energy) + " / " + round(tree.maxEnergy);

        waterGainField.text = "+" + round(tree.waterGain);
        mineralsGainField.text = "+" + round(tree.mineralsGain);
        energyGainField.text = "+" + round(tree.energyGain);
    }

    private string round(float v)
    {
        return Mathf.Floor(v * 10) / 10 + "";
    }
}