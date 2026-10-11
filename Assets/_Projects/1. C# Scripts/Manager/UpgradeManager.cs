// using UnityEngine;

// public class UpgradeManager : MonoBehaviour
// {
//     [Header("Upgrade Cost Settings")]
//     [SerializeField] private int baseUpgradeCost = 50;
//     [SerializeField] private float costPower = 1.3f;

//     [Header("Upgrade UI")]
//     [SerializeField] private UpgradeUI upgradeUI;

//     private void Start()
//     {
//         UpdateUI();
//     }

//     public int GetUpgradeCost(int currentLevel)
//     {
//         return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(currentLevel, costPower));
//     }

//     public void BuyStaminaUpgrade()
//     {
//         int currentLevel = SaveManager.GetStaminaLevel();
//         int cost = GetUpgradeCost(currentLevel);

//         if (SaveManager.SpendMoney(cost))
//         {
//             SaveManager.SetStaminaLevel(currentLevel + 1);

//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlayUpgrade();

//             Debug.Log("Stamina upgraded to level " + (currentLevel + 1));
//         }
//         else
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlayNotEnoughMoney();

//             Debug.Log("Not enough money for Stamina upgrade.");
//         }

//         UpdateUI();
//     }

//     public void BuyCharismaUpgrade()
//     {
//         int currentLevel = SaveManager.GetCharismaLevel();
//         int cost = GetUpgradeCost(currentLevel);

//         if (SaveManager.SpendMoney(cost))
//         {
//             SaveManager.SetCharismaLevel(currentLevel + 1);

//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlayUpgrade();

//             Debug.Log("Charisma upgraded to level " + (currentLevel + 1));
//         }
//         else
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlayNotEnoughMoney();

//             Debug.Log("Not enough money for Charisma upgrade.");
//         }

//         UpdateUI();
//     }

//     public void BuyEnduranceUpgrade()
//     {
//         int currentLevel = SaveManager.GetEnduranceLevel();
//         int cost = GetUpgradeCost(currentLevel);

//         if (SaveManager.SpendMoney(cost))
//         {
//             SaveManager.SetEnduranceLevel(currentLevel + 1);

//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlayUpgrade();

//             Debug.Log("Endurance upgraded to level " + (currentLevel + 1));
//         }
//         else
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlayNotEnoughMoney();

//             Debug.Log("Not enough money for Endurance upgrade.");
//         }

//         UpdateUI();
//     }

//     public float GetMaxStamina()
//     {
//         int staminaLevel = SaveManager.GetStaminaLevel();
//         return 100f + (staminaLevel * 10f);
//     }

//     public float GetCharismaMultiplier()
//     {
//         int charismaLevel = SaveManager.GetCharismaLevel();
//         return 1f + (charismaLevel * 0.05f);
//     }

//     public float GetEnduranceReduction()
//     {
//         int enduranceLevel = SaveManager.GetEnduranceLevel();
//         return Mathf.Min(enduranceLevel * 0.02f, 0.6f);
//     }

//     private void UpdateUI()
//     {
//         if (upgradeUI != null)
//         {
//             upgradeUI.UpdateUpgradeUI(this);
//         }
//     }

//     public void ResetSave()
//     {
//         SaveManager.ResetSave();
//         UpdateUI();
//     }
// }