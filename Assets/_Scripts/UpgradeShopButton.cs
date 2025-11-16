using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeShopButton : MonoBehaviour
{
    [Header("Upgrades")]
    [Tooltip("This value is a percentage boost to the player's damage. \nA value of 5 means 5% more damage.")]
    [SerializeField] private int playerDamageBoost = 0;
    [SerializeField] private int playerHealthBoost = 0;
    [SerializeField] private int playerDamageMultiplier = 0;
    [SerializeField] private int baseCost = 0;
    [SerializeField] private int maxUpgradeBars = 4;
    [SerializeField] private bool allowInfiniteUpgrades = false;
    [Header("Unique Button Identifier")]
    [Tooltip("This needs to be a unique identifier for the button. \nIt is how the system knows to disable it.")]
    [SerializeField] private string buttonID = string.Empty;
    [Header("Display")]
    [Tooltip("Please attach the desired text field for displaying cost.")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private List<Image> progressSegments = new List<Image>();

    private SaveData saveData;
    private CurrencyDisplay currencyDisplay;
    private int timesClicked = 0;
    private int currCost = 0;
    private int buttonIDIndex = -1;

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();

        if (saveData != null)
        {
            foreach (var button in saveData.clickedButtons)
            {
                if (button.buttonID == this.buttonID)
                {
                    timesClicked = button.timesClicked;
                    buttonIDIndex = saveData.clickedButtons.IndexOf(button);
                    break;
                }
            }

            if (buttonIDIndex == -1)
            {
                F_Buttons newButton = new F_Buttons(this.buttonID, this.timesClicked);

                saveData.clickedButtons.Add(newButton);
                buttonIDIndex = saveData.clickedButtons.IndexOf(newButton);
            }
        }

        currencyDisplay = (CurrencyDisplay)FindFirstObjectByType(typeof(CurrencyDisplay));

        UpdateCost();
        // UpdateProgress();
        InitializeProgressBar();
    }
    
    // This is shit, shouldn't be in update. 
    private void FixedUpdate()
    {
        if (currCost > saveData.crystals)
        {
            this.gameObject.GetComponent<Button>().interactable = false;
        }
        else
        {
            this.gameObject.GetComponent<Button>().interactable = true;
        }
    }

    public void OnUpgradeClick()
    {
        // Exit if we don't have enough money
        if (currCost > saveData.crystals)
        {
            return;
        }

        if (!allowInfiniteUpgrades && timesClicked >= maxUpgradeBars * progressSegments.Count)
        {
            this.gameObject.GetComponent<Button>().interactable = false;
            return;
        }

        // Decrement currency
        saveData.crystals -= currCost;

        // Update currency display
        if (currencyDisplay)
        {
            currencyDisplay.UpdateText();
        }

        // Increment times selected on successful purchase
        this.timesClicked++;

        // Update stored data for the button
        if (buttonIDIndex != -1)
        {
            saveData.clickedButtons[buttonIDIndex] = new F_Buttons(this.buttonID, this.timesClicked);
        }

        this.UpdateProgress();
        this.UpdateSave();
        this.UpdateCost();

        if (!allowInfiniteUpgrades && timesClicked >= maxUpgradeBars * progressSegments.Count)
        {
            this.gameObject.GetComponent<Button>().interactable = false;
            return;
        }
    }

    private void UpdateCost()
    {
        currCost = baseCost + (baseCost * timesClicked);

        if (costText != null)
        {
            costText.text = "Cost: " + currCost;
        }

        if (currCost > saveData.crystals)
        {
            this.gameObject.GetComponent<Button>().interactable = false;
        } else
        {
            this.gameObject.GetComponent<Button>().interactable = true;
        }
    }

    private void InitializeProgressBar()
    {
        // Simulate clicks
        for (int i = 0; i < timesClicked; i++)
        {
            int modTarget = i % progressSegments.Count;

            UnityEngine.Color currColor = progressSegments[modTarget].color;

            Vector3 newHSV = Vector3.zero;
            UnityEngine.Color.RGBToHSV(currColor, out newHSV.x, out newHSV.y, out newHSV.z);

            newHSV.x += (1.0f / maxUpgradeBars);
            newHSV.x %= 1.0f;

            if (newHSV.y < 0.9f)
            {
                newHSV.y += 1.0f / maxUpgradeBars;
                newHSV.y = Mathf.Min(newHSV.y, 0.9f);
            }
            if (newHSV.z < 0.9f)
            {
                newHSV.z += 1.0f / maxUpgradeBars;
                newHSV.z = Mathf.Min(newHSV.y, 0.9f);
            }

            progressSegments[modTarget].color = UnityEngine.Color.HSVToRGB(newHSV.x, newHSV.y, newHSV.z);
        }

        // Update text
        if (progressText != null)
        {
            if (playerDamageBoost > 0)
            {
                progressText.text = "+" + saveData.playerDamageBoost.ToString() + "%";
            }

            if (playerHealthBoost > 0)
            {
                progressText.text = "+" + saveData.playerHealthBoost.ToString() + "%";
            }

            if (playerDamageMultiplier > 0)
            {
                progressText.text = "+" + saveData.playerDamageMultiplier.ToString() + "%";
            }
        }
    }

    private void UpdateProgress()
    {
        if (progressSegments.Count > 0 && timesClicked - 1 >= 0)
        {
            int modTarget = (timesClicked - 1) % progressSegments.Count;

            UnityEngine.Color currColor = progressSegments[modTarget].color;
            
            Vector3 newHSV = Vector3.zero;
            UnityEngine.Color.RGBToHSV(currColor, out newHSV.x, out newHSV.y, out newHSV.z);
            
            newHSV.x += (1.0f / maxUpgradeBars);
            newHSV.x %= 1.0f;
            
            if (newHSV.y < 0.9f)
            {
                newHSV.y += 1.0f / maxUpgradeBars;
                newHSV.y = Mathf.Min(newHSV.y, 0.9f);
            }
            if (newHSV.z < 0.9f)
            {
                newHSV.z += 1.0f / maxUpgradeBars;
                newHSV.z = Mathf.Min(newHSV.y, 0.9f);
            }

            progressSegments[modTarget].color = UnityEngine.Color.HSVToRGB(newHSV.x, newHSV.y, newHSV.z);
        }

        // Update Text
        if (progressText != null)
        {
            if (playerDamageBoost > 0)
            {
                progressText.text = "+" + saveData.playerDamageBoost.ToString() + "%";
            } 
            
            if (playerHealthBoost > 0)
            {
                progressText.text = "+" + saveData.playerHealthBoost.ToString() + "%";
            } 
            
            if (playerDamageMultiplier > 0)
            {
                progressText.text = "+" + saveData.playerDamageMultiplier.ToString() + "%";
            }
        }
    }

    private void UpdateSave()
    {
        // Update stored stat data
        saveData.playerHealthBoost += playerHealthBoost;
        saveData.playerDamageBoost += playerDamageBoost;
        saveData.playerDamageMultiplier += playerDamageMultiplier;
    }
}


/*
            index = (saveData.clickedButtons.FindIndex(s => s == buttonID));
            Debug.Log("Index: " + index);

            var buttonInfo = saveData.clickedButtons[index];

            string clickCount = saveData.clickedButtons[index].Substring(buttonInfo.IndexOf(';') + 1);
            Debug.Log("Substring: " + clickCount);
            // Too useful to lose, but not necessary: Regex.Match(subStr, @"\d+").Value
            timesClicked = Int32.Parse(clickCount);
            Debug.Log("Times Clicked = " + timesClicked);
*/

/*
        // Update stored button identifier
        int index = (saveData.clickedButtons.FindIndex(s => s == buttonID + ";"));
        
        if (index != -1)
        {
            saveData.clickedButtons[index] = buttonID + ";" + timesClicked;
        } else
        {
            saveData.clickedButtons.Add(buttonID + ";" + timesClicked);
        }
*/

/*
 string buttonIDSubStr = string.Empty;

            // int index = (saveData.clickedButtons.FindIndex(s => s == buttonID));
            foreach (var button in saveData.clickedButtons)
            {
                if (button.Length > buttonID.Length && button != "NULL")
                {
                    buttonIDSubStr = button[..buttonID.Length];
                } else
                {
                    continue;
                }
                
                if (buttonID == buttonIDSubStr)
                {
                    string buttonClickCountSubStr = button.Substring(button.IndexOf(';') + 1);
                    timesClicked = Int32.Parse(buttonClickCountSubStr);
                    break;
                }
            }

            // Initialize button if not found.
            if (buttonIDSubStr == string.Empty)
            {
                saveData.clickedButtons.Add(buttonID + ";" + timesClicked);
            }
 */