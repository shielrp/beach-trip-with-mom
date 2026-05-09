using UnityEngine;

public class DropObject : MonoBehaviour
{
    public Transform PinchTarget;
    public Rigidbody2D Rb2d;

    public Vector2 PinchPositionOffset => PinchTarget.position - transform.position;
}
