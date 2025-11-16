using UnityEngine;

public class Howtoplay : MonoBehaviour
{
    public GameObject howToPlay;


   
    public void DisplayPrefab()
    {
        if(howToPlay != null)
        {
            howToPlay.SetActive(true);
        }
    }

    public void HidePrefab()
    {
        if (howToPlay != null)
        {
            howToPlay.SetActive(false);
        }
    }

}
