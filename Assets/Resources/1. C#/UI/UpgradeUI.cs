using UnityEngine;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    [Header("Money")]
    [SerializeField] private TMP_Text moneyText;

    [Header("Stamina UI")]
    [SerializeField] private TMP_Text staminaLevelText;
    [SerializeField] private TMP_Text staminaCostText;
    [SerializeField] private TMP_Text staminaValueText;

    [Header("Charisma UI")]
    [SerializeField] private TMP_Text charismaLevelText;
    [SerializeField] private TMP_Text charismaCostText;
    [SerializeField] private TMP_Text charismaValueText;

    [Header("Endurance UI")]
    [SerializeField] private TMP_Text enduranceLevelText;
    [SerializeField] private TMP_Text enduranceCostText;
    [SerializeField] private TMP_Text enduranceValueText;

    public void UpdateUpgradeUI(UpgradeManager upgradeManager)
    {
        int money = SaveManager.GetMoney();

        int staminaLevel = SaveManager.GetStaminaLevel();
        int charismaLevel = SaveManager.GetCharismaLevel();
        int enduranceLevel = SaveManager.GetEnduranceLevel();

        int staminaCost = upgradeManager.GetUpgradeCost(staminaLevel);
        int charismaCost = upgradeManager.GetUpgradeCost(charismaLevel);
        int enduranceCost = upgradeManager.GetUpgradeCost(enduranceLevel);

        float maxStamina = upgradeManager.GetMaxStamina();
        float charismaMultiplier = upgradeManager.GetCharismaMultiplier();
        float enduranceReduction = upgradeManager.GetEnduranceReduction();

        if (moneyText != null)
            moneyText.text = "Money: " + money;

        if (staminaLevelText != null)
            staminaLevelText.text = "Level: " + staminaLevel;

        if (staminaCostText != null)
            staminaCostText.text = "Cost: " + staminaCost;

        if (staminaValueText != null)
            staminaValueText.text = "Max Stamina: " + maxStamina;


        if (charismaLevelText != null)
            charismaLevelText.text = "Level: " + charismaLevel;

        if (charismaCostText != null)
            charismaCostText.text = "Cost: " + charismaCost;

        if (charismaValueText != null)
            charismaValueText.text = "Multiplier: x" + charismaMultiplier.ToString("F2");


        if (enduranceLevelText != null)
            enduranceLevelText.text = "Level: " + enduranceLevel;

        if (enduranceCostText != null)
            enduranceCostText.text = "Cost: " + enduranceCost;

        if (enduranceValueText != null)
            enduranceValueText.text = "Penalty Reduction: " + Mathf.RoundToInt(enduranceReduction * 100f) + "%";
    }
}