using System.Collections;
using UnityEngine;

public class AutoDestroyParticles : MonoBehaviour
{
    public float AutoDestroyTime;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(AutoDestroyTime);
        Destroy(gameObject);
    }
}
