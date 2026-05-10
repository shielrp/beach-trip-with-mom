using System;
using System.Collections.Generic;
using UnityEngine;

public class DropObject : MonoBehaviour
{
    public static event Action<DropObject> DropObjectSettled;

    [Serializable]
    class PinchPoint
    {
        public Transform Location;
        public float Rotation;

        public Vector2 GetPinchOffset(Transform tform)
        {
            return Location.position - tform.position;
        }
    }

    [SerializeField]
    PinchPoint[] PinchPoints;
    public ParticleSystem SandParticlesPrefab;

    public Transform PinchTransform => PinchPoints[_selectedPinchPoint].Location;
    public float PinchRotation => PinchPoints[_selectedPinchPoint].Rotation;
    public Vector2 PinchOffset => PinchPoints[_selectedPinchPoint].GetPinchOffset(transform);

    public Rigidbody2D Rb2d;

    bool _isFalling = false;
    bool _isSettled = false;
    int _selectedPinchPoint;

    void OnCollisionEnter2D(Collision2D collision)
    {
        DropObject dropObject = collision.gameObject.GetComponent<DropObject>();

        if (_isFalling && !_isSettled && (dropObject != null || collision.gameObject.layer == LayerMask.NameToLayer("Ground")))
        {
            _isFalling = false;
            _isSettled = true;
            DropObjectSettled?.Invoke(this);

            //play sand particles
            if (SandParticlesPrefab != null)
                Instantiate(SandParticlesPrefab, collision.GetContact(0).point, Quaternion.identity);
        }
    }

    public void Drop()
    {
        _isFalling = true;
    }

    public bool IsSettled()
    {
        return _isSettled;
    }

    public void RandomizePinchPoint()
    {
        _selectedPinchPoint = UnityEngine.Random.Range(0, PinchPoints.Length);
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
