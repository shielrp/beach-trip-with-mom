using UnityEngine;
using UnityEngine.Events;

public class GroundEndTrigger : MonoBehaviour
{
    public LayerMask ChunkLayers;

    public UnityEvent KillFloorHitEvent;

    void OnCollisionEnter2D(Collision2D collision)
    {
        KillFloorHitEvent?.Invoke();
    }
}
