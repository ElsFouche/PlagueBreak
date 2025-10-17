using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopButton : MonoBehaviour
{
    [Header("Upgrades")]
    [Tooltip("This value is a percentage boost to the player's damage. \nA value of 5 means 5% more damage.")]
    [SerializeField] private int playerDamageBoost = 0;
    [SerializeField] private int playerHealthBoost = 0;
    [Header("Unique Button Identifier")]
    [SerializeField] private string buttonID = string.Empty;

    private SaveData saveData;
    private bool bWasClicked = false;

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();
        
        if (saveData != null)
        {
            foreach (var item in saveData.clickedButtons)
            {
                if (item == buttonID)
                {
                    bWasClicked = true;
                    this.gameObject.GetComponent<Button>().interactable = false;
                    break;
                }
            }
        }
    }

    public void OnUpgradeClick()
    {
        // Prevent clicking again
        if (bWasClicked)
        {
            return;
        }
        bWasClicked = true;

        this.gameObject.GetComponent<Button>().interactable = false;

        saveData.clickedButtons.Add(buttonID);
        saveData.playerHealthBoost += playerHealthBoost;
        saveData.playerDamageBoost += playerDamageBoost;
    }
}