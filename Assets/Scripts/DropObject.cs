using System.Collections.Generic;
using UnityEngine;

public class DropObject : MonoBehaviour
{
    public Transform PinchTarget;
    public Rigidbody2D Rb2d;

    public Vector2 PinchPositionOffset => PinchTarget.position - transform.position;

    bool _isFalling = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        DropObject dropObject = collision.gameObject.GetComponent<DropObject>();

        if (dropObject != null || collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            _isFalling = false;
    }

    public void Drop()
    {
        _isFalling = true;
    }

    public bool IsSettled()
    {
        return !_isFalling;
    }

    public float GetHighestPoint()
    {
        float highestPoint = transform.position.y;
        List<Collider2D> attachedCols = new List<Collider2D>();

        Rb2d.GetAttachedColliders(attachedCols);

        foreach (Collider2D col in attachedCols)
        {
            Vector2 highestPointThisCol = col.ClosestPoint(new Vector2(0, 10000));

            if (highestPointThisCol.y > highestPoint)
                highestPoint = highestPointThisCol.y;
        }

        return highestPoint;
    }
}
