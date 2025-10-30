using UnityEngine;

public class SelfDestructor : MonoBehaviour
{
    [SerializeField] private bool destroyOnAwake = false;

    private void Awake()
    {
        if (destroyOnAwake)
        {
            Destroy(this.gameObject);
        }
    }
}
