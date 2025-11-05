using UnityEngine;

public static class Vibration
{
#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaObject vibrator;
#endif

    public static void Vibrate()
    {
        if (IsAndroid())
        {
            vibrator.Call("vibrate");
        } else
        {
            Handheld.Vibrate();
        }
    }

    public static void Vibrate(long milliseconds)
    {
        if (IsAndroid())
        {
            vibrator.Call("vibrate", milliseconds);
        }
        else
        {
            Handheld.Vibrate();
        }
    }

    public static void Vibrate(long[] pattern, int repeat)
    {
        if (IsAndroid())
        {
            vibrator.Call("vibrate", pattern, repeat);
        }
        else
        {
            Handheld.Vibrate();
        }
    }

    public static bool HasVibrator()
    {
        return IsAndroid();
    }

    public static void Cancel()
    {
        if (IsAndroid())
        {
            vibrator.Call("cancel");
        }
    }

    private static bool IsAndroid()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
            return true;
        #else
            return false;
        #endif
    }
}