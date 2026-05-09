using System;
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
    public TMP_Text TimeLeftTMP;

    public Button StartButton;
    public Animator UiAnimator;
    public Button EndButton;

    public CastleHeightReader CastleHeight;

    InputAction _clickAction;
    bool _canClick = false;

    public State CurrentState { get; private set; } = State.Starting;

    int _roundTimeSeconds = 91;

    float _currentScore = 0;
    float _startTime;

    void Start()
    {
        _clickAction = InputSystem.actions.FindAction("Click");
        ScoreText.text = $"height: 0' 0\"";

        TimeSpan timeLeft = new TimeSpan(0, 0, _roundTimeSeconds);
        TimeLeftTMP.text = $"time left: {timeLeft.Minutes}:{timeLeft.Seconds:00}";
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
            TimeLeftTMP.text = _roundTimeSeconds.ToString();
            UiAnimator.Play("StartIScreenOut");

            _startTime = Time.time;
            CurrentState = State.Playing;

            Invoke(nameof(StartGame), 0.66f);
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

            ScoreText.text = $"Height: {feet}'{inches}\"";
        }
    }

    void StartGame()
    {
        _canClick = true;
        _ = LoadNewDropObjectOntoHand();
    }

    void Update()
    {
        if (CurrentState != State.Playing)
            return;

        _currentScore = CastleHeight.GetWorldHeightOfCastle();

        int feet = Mathf.FloorToInt(_currentScore);
        int inches = Mathf.RoundToInt((_currentScore % 1f) * 12);

        ScoreText.text = $"height:{feet}' {inches}\"";

        float timeSpent = Time.time - _startTime;

        if (timeSpent > _roundTimeSeconds)
            EndGame();

        TimeSpan timeLeft = new TimeSpan(0, 0, Mathf.RoundToInt(_roundTimeSeconds - timeSpent));
        TimeLeftTMP.text = $"time left: {timeLeft.Minutes}:{timeLeft.Seconds:00}";

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
        return Instantiate(DropObjectPrefabs[UnityEngine.Random.Range(0, DropObjectPrefabs.Length)]);
    }

    public void EndGame()
    {
        if (CurrentState == State.Playing)
        {
            Debug.Log($"Game Over!");
            AdvanceState();
        }
    }
}
