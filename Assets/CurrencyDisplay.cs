using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    public TextMeshProUGUI crystalCurrencyText;
    private SaveData saveData = new();

    private void Awake()
    {
        saveData = SaveManager.instance.GetSaveData();
        UpdateText();
    }

    public void UpdateText()
    {
        if (saveData == null || crystalCurrencyText == null)
        {
            return;
        }

        crystalCurrencyText.text = saveData.crystals.ToString();
    }
}