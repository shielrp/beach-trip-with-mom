using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public Hand Hand;
    public DropObject[] DropObjectPrefabs;

    InputAction _clickAction;

    void Start()
    {
        _clickAction = InputSystem.actions.FindAction("Click");
    }

    void Update()
    {
        if (!_clickAction.WasCompletedThisFrame())
            return;

        if (Hand.HasAttachedDropObject)
        {
            Hand.DropObject();
        }
        else
        {
            DropObject newDrop = GetNextDropObjectPrefab();
            Hand.AttachDropObject(newDrop);
        }
    }

    DropObject GetNextDropObjectPrefab()
    {
        return Instantiate(DropObjectPrefabs[Random.Range(0, DropObjectPrefabs.Length)]);
    }
}
