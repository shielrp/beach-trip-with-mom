using UnityEngine;
using UnityEngine.InputSystem;

public class Hand : MonoBehaviour
{
    public float FollowSpeed;
    public float CloseEnoughDistance;
    public Joint2D HoldJoint;
    public Rigidbody2D MoveRigidbody;
    public bool LockYMovement;

    public bool HasAttachedDropObject => _attachedDropObject != null;

    bool _followMouse = true;

    InputAction _mouseAction;
    Camera _perspectiveCamera;

    DropObject _attachedDropObject;

    void Start()
    {
        _mouseAction = InputSystem.actions.FindAction("Point");
        _perspectiveCamera = Camera.main;
    }

    void FixedUpdate()
    {
        if (!_followMouse)
            return;

        Vector2 mouseScreenPos = _mouseAction.ReadValue<Vector2>();
        Vector2 mouseFollowPosition = _perspectiveCamera.ScreenToWorldPoint(mouseScreenPos);

        Vector2 centerPoint = HoldJoint.transform.position;

        Vector2 targetPosition = mouseFollowPosition;
        if (LockYMovement)
            targetPosition.y = centerPoint.y;

        Vector2 positionOffset = MoveRigidbody.position - centerPoint;

        Vector2 diffVector = targetPosition - centerPoint;

        if (diffVector.magnitude <= CloseEnoughDistance)
        {
            MoveRigidbody.MovePosition(targetPosition + positionOffset);
            return;
        }

        Vector2 moveDelta = FollowSpeed * Time.fixedDeltaTime * diffVector.normalized;
        MoveRigidbody.position += moveDelta;
    }

    public void AttachDropObject(DropObject dropObject)
    {
        if (_attachedDropObject != null && _attachedDropObject != dropObject)
        {
            Debug.LogError($"Trying to attach DropObject {dropObject.gameObject.name} to Hand while it's already carrying" +
                $" DropObject {_attachedDropObject.gameObject.name}");
            return;
        }

        dropObject.transform.position = (Vector2)HoldJoint.transform.position - dropObject.PinchPositionOffset;
        dropObject.transform.SetParent(HoldJoint.transform);
        HoldJoint.connectedBody = dropObject.Rb2d;
        _attachedDropObject = dropObject;

        Debug.Log($"Attached object {_attachedDropObject.gameObject.name}");
    }

    public void DropObject()
    {
        if (_attachedDropObject == null) 
            return;

        Debug.Log($"Detached object {_attachedDropObject.gameObject.name}");

        HoldJoint.connectedBody = null;
        _attachedDropObject.transform.SetParent(null);
        _attachedDropObject = null;
    }
}
