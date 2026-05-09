using DG.Tweening;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hand : MonoBehaviour
{
    public float FollowSpeed;
    public float CloseEnoughDistance;
    public Joint2D HoldJoint;
    public Rigidbody2D MoveRigidbody;
    public Transform DroppedObjectParent;
    public bool LockYMovement;

    public float HandHiddenHeight;
    public float HandDefaultHeight;

    public bool HasAttachedDropObject => _attachedDropObject != null;

    bool _followMouse = false;

    InputAction _mouseAction;
    Camera _perspectiveCamera;

    DropObject _attachedDropObject;

    public bool IsShowing { get; private set; } = false;

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
        if (mouseScreenPos.x < 0 || mouseScreenPos.x > Screen.width || mouseScreenPos.y < 0 || mouseScreenPos.y > Screen.height)
            return;

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

    public async Awaitable SetShowing(bool shouldShow)
    {
        if (shouldShow == IsShowing) 
            return;

        IsShowing = shouldShow;
        float moveTime = shouldShow ? 0.66f : 0f;

        if (shouldShow)
        {
            float xTarget = _perspectiveCamera.ScreenToWorldPoint(_mouseAction.ReadValue<Vector2>()).x;
            transform.position = transform.position.MMSetX(xTarget);
        }

        float targetHeight = shouldShow ? HandDefaultHeight : HandHiddenHeight;
        transform.DOMoveY(targetHeight, moveTime).SetEase(Ease.OutQuint);
        await Awaitable.WaitForSecondsAsync(moveTime);
    }

    public void SetActive(bool active)
    {
        _followMouse = active;
    }

    public void AttachDropObject(DropObject dropObject)
    {
        if (_attachedDropObject != null && _attachedDropObject != dropObject)
        {
            Debug.LogError($"Trying to attach DropObject {dropObject.gameObject.name} to Hand while it's already carrying" +
                $" DropObject {_attachedDropObject.gameObject.name}");
            return;
        }

        dropObject.RandomizePinchPoint();
        dropObject.transform.rotation = Quaternion.Euler(0f, 0f, dropObject.PinchRotation);

        dropObject.transform.position = (Vector2)HoldJoint.transform.position - dropObject.PinchOffset;
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
        _attachedDropObject.transform.SetParent(DroppedObjectParent);
        _attachedDropObject.Drop();
        _attachedDropObject = null;
    }
}
