using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public enum State
    {
        Starting, Playing, Ending
    }

    public Hand Hand;
    public DropObject[] DropObjectPrefabs;

    public TMP_Text ScoreText;
    public Button StartButton;
    public Button EndButton;

    public CastleHeightReader CastleHeight;

    InputAction _clickAction;
    bool _canClick = true;

    public State CurrentState { get; private set; } = State.Starting;

    float _currentScore = 0;

    void Start()
    {
        _clickAction = InputSystem.actions.FindAction("Click");
        ScoreText.text = $"Height: 0' 0\"";
        StartButton.onClick.AddListener(OnStartButtonPressed);
        EndButton.onClick.AddListener(OnEndButtonPressed);
    }

    void OnStartButtonPressed()
    {
        if (CurrentState != State.Starting)
            return;

        AdvanceState();
    }

    void OnEndButtonPressed()
    {
        if (CurrentState != State.Ending)
            return;

        ResetGame();
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
            StartButton.gameObject.SetActive(false);
        }

        else if (CurrentState == State.Playing)
        {
            //end game
            CurrentState = State.Ending;
            Hand.SetActive(false);
            _ = Hand.SetShowing(false);
            EndButton.gameObject.SetActive(true);

            _currentScore = CastleHeight.GetWorldHeightOfCastle();

            int feet = Mathf.FloorToInt(_currentScore);
            int inches = Mathf.RoundToInt((_currentScore % 1f) * 12);

            ScoreText.text = $"Height: {feet}' {inches}\"";
        }
    }

    void Update()
    {
        if (CurrentState != State.Playing)
            return;

        _currentScore = CastleHeight.GetWorldHeightOfCastle();

        int feet = Mathf.FloorToInt(_currentScore);
        int inches = Mathf.RoundToInt((_currentScore % 1f) * 12);

        ScoreText.text = $"Height: {feet}' {inches}\"";

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
