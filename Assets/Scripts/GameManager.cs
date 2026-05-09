using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum State
    {
        Starting, Playing, Ending
    }

    public Hand Hand;
    public DropObject[] DropObjectPrefabs;

    InputAction _clickAction;
    bool _canClick = true;

    public State CurrentState { get; private set; } = State.Starting;

    void Start()
    {
        _clickAction = InputSystem.actions.FindAction("Click");
    }

    void ResetGame()
    {
        SceneManager.LoadScene(0);
    }

    void AdvanceState()
    {
        if (CurrentState == State.Starting)
        {
            //start game
            CurrentState = State.Playing;
        }

        else if (CurrentState == State.Playing)
        {
            //end game
            CurrentState = State.Ending;
            Hand.SetActive(false);
            _ = Hand.SetShowing(false);
        }
    }

    void Update()
    {
        if (!_canClick || !_clickAction.WasCompletedThisFrame())
            return;

        if (Hand.HasAttachedDropObject)
        {
            Hand.DropObject();
        }
        else
        {
            _ = LoadNewDropObjectOntoHand();
        }
    }

    async Awaitable LoadNewDropObjectOntoHand()
    {
        _canClick = false;
        Hand.SetActive(false);

        if (Hand.IsShowing)
            await Hand.SetShowing(false);

        DropObject newDrop = GetNextDropObjectPrefab();
        Hand.AttachDropObject(newDrop);

        await Hand.SetShowing(true);
        Hand.SetActive(true);
        _canClick = true;
    }

    DropObject GetNextDropObjectPrefab()
    {
        return Instantiate(DropObjectPrefabs[Random.Range(0, DropObjectPrefabs.Length)]);
    }

    public void EndGame()
    {
        Debug.Log($"Game Over!");
        if (CurrentState == State.Playing)
        {
            AdvanceState();
        }
    }
}
