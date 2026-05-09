using System;
using System.Collections;
using MoreMountains.FeedbacksForThirdParty;
using TMPro;
using Unity.Cinemachine;
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
    public TMP_Text FinalHeightTmp;

    public Image ScreenshotImage;

    public CastleHeightReader CastleHeight;

    InputAction _clickAction;
    bool _canClick = false;

    public State CurrentState { get; private set; } = State.Starting;

    int _roundTimeSeconds = 91;

    float _currentScore = 0;
    float _startTime;

    float _lastDropObjSpawnTime;

    void Start()
    {
        _clickAction = InputSystem.actions.FindAction("Click");
        ScoreText.text = $"height: 0'0\"";

        TimeSpan timeLeft = new TimeSpan(0, 0, _roundTimeSeconds);
        TimeLeftTMP.text = $"time until high tide: {timeLeft.Minutes}:{timeLeft.Seconds:00}";
        StartButton.onClick.AddListener(OnStartButtonPressed);
        EndButton.onClick.AddListener(OnEndButtonPressed);

        DropObject.DropObjectSettled += OnDropObjectSettled;
    }

    void OnDestroy()
    {
        DropObject.DropObjectSettled -= OnDropObjectSettled;
    }

    void OnDropObjectSettled(DropObject obj)
    {
        _ = DelayedNewObject();
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

            _currentScore = CastleHeight.GetWorldHeightOfCastle();

            int feet = Mathf.FloorToInt(_currentScore);
            int inches = Mathf.RoundToInt((_currentScore % 1f) * 12);

            ScoreText.text = $"height: {feet}'{inches}\"";

            _ = DoGameEnd();
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

        ScoreText.text = $"height:{feet}'{inches}\"";

        float timeSpent = Time.time - _startTime;

        if (timeSpent > _roundTimeSeconds)
            EndGame();

        TimeSpan timeLeft = new TimeSpan(0, 0, Mathf.RoundToInt(_roundTimeSeconds - timeSpent));
        TimeLeftTMP.text = $"time until high tide: {timeLeft.Minutes}:{timeLeft.Seconds:00}";

        if (!_canClick || !_clickAction.WasCompletedThisFrame())
            return;

        if (Hand.HasAttachedDropObject)
        {
            Hand.DropObject();
        }
    }

    async Awaitable DelayedNewObject()
    {
        if (Hand.HasAttachedDropObject || Time.time - _lastDropObjSpawnTime < 0.66f)
            return;

        GetComponent<CinemachineImpulseSource>().GenerateImpulse();

        await Awaitable.WaitForSecondsAsync(0.33f);
        await LoadNewDropObjectOntoHand();
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
        _lastDropObjSpawnTime = Time.time;
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

    async Awaitable DoGameEnd()
    {
        await Awaitable.EndOfFrameAsync();

        Texture2D screenShot = ScreenCapture.CaptureScreenshotAsTexture();

        // All the following is necessary due to a Unity bug when working in Linear color space:

        Texture2D newScreenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        newScreenShot.SetPixels(screenShot.GetPixels());
        newScreenShot.Apply();

        Sprite spr = Sprite.Create(newScreenShot, new Rect(0, 0, newScreenShot.width, newScreenShot.height), new Vector2(0.5f, 0.5f));
        ScreenshotImage.sprite = spr;

        int feet = Mathf.FloorToInt(_currentScore);
        int inches = Mathf.RoundToInt((_currentScore % 1f) * 12);
        FinalHeightTmp.text = $"{feet}'{inches}\"";

        UiAnimator.Play("EndScreenIn");
    }
}
