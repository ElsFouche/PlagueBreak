using System.Collections;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class CameraShake : MonoBehaviour
{
    private Coroutine CR_Shaker = null;
    private Coroutine CR_ReturnToStart = null;
    private Vector3 startPosition; 
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void Shake(float magnitude = 1.0f, float duration = 1.0f)
    {
        if (CR_Shaker == null)
        {
            CR_Shaker = StartCoroutine(Shaker(magnitude, duration));
        }else
        {
            StopCoroutine(CR_Shaker);
            transform.rotation = new Quaternion(startRotation.x, startRotation.y, startRotation.z, startRotation.w);
            CR_Shaker = StartCoroutine(Shaker(magnitude, duration));
        }
    }

    private IEnumerator Shaker(float magnitude, float duration)
    {
        Debug.Log("Shaker started.");
        if (CR_ReturnToStart != null)
        {
            StopCoroutine(CR_ReturnToStart);
            CR_ReturnToStart = null;
        }

        float timer = 0.0f;
        Quaternion endRotation = transform.rotation;
        while (timer < duration)
        {
            // Update the goal rotation if the current rotation and the end rotation are nearly eaqual. 
            if (Mathf.Abs(endRotation.y) - Mathf.Abs(transform.rotation.y) < 0.1f)
            {
                endRotation = new Quaternion((startRotation.x), 
                                              (startRotation.y + 0.1f) * (Random.Range(-magnitude, magnitude) / 100.0f), 
                                              startRotation.z, 
                                              startRotation.w);
            }

            transform.rotation = new Quaternion(startRotation.x,
                                                (Mathf.SmoothStep(transform.rotation.y, endRotation.y, (timer/10.0f)/duration)),
                                                startRotation.z,
                                                startRotation.w);

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        CR_ReturnToStart = StartCoroutine(ReturnToStart());

        CR_Shaker = null;
    }

    private IEnumerator ReturnToStart()
    {
        float timer = 0.0f;
        while (Mathf.Abs(transform.rotation.y) - Mathf.Abs(startRotation.y) > 0.01f)
        {
            transform.rotation = new Quaternion(startRotation.x,
                                                (Mathf.SmoothStep(transform.rotation.y, startRotation.y, timer)),
                                                startRotation.z,
                                                startRotation.w);

            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }

        CR_ReturnToStart = null;
    }
}