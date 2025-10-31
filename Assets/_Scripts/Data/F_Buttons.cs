using System;
using UnityEngine;

[Serializable]
public struct F_Buttons
{
    public F_Buttons(string ID, int numClicks)
    {
        buttonID = ID;
        timesClicked = numClicks;
    }

    public string buttonID;
    public int timesClicked;

    public override string ToString() => $"{buttonID}; {timesClicked}";
}