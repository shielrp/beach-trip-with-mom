
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StickingPoint : MonoBehaviour
{
    Transform _setParent = null;

    public List<Rigidbody2D> ExcludeBodies;

    public UnityEvent OnStuck;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (_setParent != null)
            return;

        if (collision.attachedRigidbody != null && !ExcludeBodies.Contains(collision.attachedRigidbody))
        {
            transform.parent.SetParent(collision.attachedRigidbody.transform);
            _setParent = collision.attachedRigidbody.transform;
        }
    }
}
