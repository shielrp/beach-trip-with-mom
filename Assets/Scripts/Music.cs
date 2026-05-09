using UnityEngine;

public class Music : MonoBehaviour
{
    static Music MusicInstance = null;

    void Awake()
    {
        if (MusicInstance == null)
        {
            MusicInstance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
