using System.Collections;
using UnityEngine;

public class SelfDestructor : MonoBehaviour
{
    [SerializeField] private bool destroyOnAwake = false;
    [SerializeField] private float destroyAfterSeconds = 1.0f;

    private void Awake()
    {
        if (destroyOnAwake)
        {
            Destroy(this.gameObject);
        } else if (destroyAfterSeconds > 0.0f)
        {
            StartCoroutine(DestroyAfterSeconds(destroyAfterSeconds));
        }
    }

    private IEnumerator DestroyAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(this.gameObject);
    }
}
