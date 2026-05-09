using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.InputSystem;

public class Hand : MonoBehaviour
{
    public float FollowSpeed;
    public float CloseEnoughDistance;
    public Joint2D HoldJoint;
    public Rigidbody2D MoveRigidbody;
    public bool LockYMovement;

    bool _followMouse = true;

    InputAction _mouseAction;
    Camera _perspectiveCamera;

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

        Vector2 targetPosition = mouseFollowPosition;
        if (LockYMovement)
            targetPosition.y = MoveRigidbody.position.y;

        Vector2 centerPoint = HoldJoint.transform.position;
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
}
