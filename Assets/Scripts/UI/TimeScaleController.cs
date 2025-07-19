using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Canvas))]
public class TimeScaleController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider timeScaleSlider;
    [SerializeField] private TMP_Text timeScaleText;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button normalSpeedButton;
    [SerializeField] private Button fastForwardButton;
    [SerializeField] private Button ultraSpeedButton;

    [Header("Settings")]
    [SerializeField] private float minTimeScale = 0.1f;
    [SerializeField] private float maxTimeScale = 10f;
    [SerializeField] private float normalTimeScale = 1f;
    [SerializeField] private float fastForwardScale = 2f;
    [SerializeField] private float ultraSpeedScale = 4f;
    [SerializeField] private float smoothTransitionDuration = 0.5f;

    private float targetTimeScale;
    private float transitionVelocity;
    private bool isPaused;

    private void Awake()
    {
        // Initialize UI
        timeScaleSlider.minValue = minTimeScale;
        timeScaleSlider.maxValue = maxTimeScale;
        timeScaleSlider.value = normalTimeScale;
        targetTimeScale = normalTimeScale;

        // Setup button callbacks
        pauseButton.onClick.AddListener(TogglePause);
        normalSpeedButton.onClick.AddListener(SetNormalSpeed);
        fastForwardButton.onClick.AddListener(SetFastForward);
        ultraSpeedButton.onClick.AddListener(SetUltraSpeed);

        // Setup slider callback
        timeScaleSlider.onValueChanged.AddListener(OnSliderValueChanged);

        UpdateUI();
    }

    private void Update()
    {
        // Smoothly transition to target time scale
        if (!Mathf.Approximately(Time.timeScale, targetTimeScale))
        {
            Time.timeScale = Mathf.SmoothDamp(Time.timeScale, targetTimeScale, ref transitionVelocity, smoothTransitionDuration);
            UpdateUI();
        }

        // Handle keyboard shortcuts
        HandleKeyboardInput();
    }

    private void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TogglePause();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetNormalSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetFastForward();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetUltraSpeed();
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            DecreaseTimeScale();
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            IncreaseTimeScale();
        }
    }

    private void OnSliderValueChanged(float value)
    {
        targetTimeScale = value;
        isPaused = false;
        UpdateUI();
    }

    private void TogglePause()
    {
        if (isPaused)
        {
            targetTimeScale = timeScaleSlider.value > 0 ? timeScaleSlider.value : normalTimeScale;
        }
        else
        {
            targetTimeScale = 0f;
        }
        isPaused = !isPaused;
        UpdateUI();
    }

    private void SetNormalSpeed()
    {
        targetTimeScale = normalTimeScale;
        timeScaleSlider.value = normalTimeScale;
        isPaused = false;
        UpdateUI();
    }

    private void SetFastForward()
    {
        targetTimeScale = fastForwardScale;
        timeScaleSlider.value = fastForwardScale;
        isPaused = false;
        UpdateUI();
    }

    private void SetUltraSpeed()
    {
        targetTimeScale = ultraSpeedScale;
        timeScaleSlider.value = ultraSpeedScale;
        isPaused = false;
        UpdateUI();
    }

    private void IncreaseTimeScale()
    {
        float newValue = Mathf.Clamp(timeScaleSlider.value + 0.1f, minTimeScale, maxTimeScale);
        timeScaleSlider.value = newValue;
        targetTimeScale = newValue;
        isPaused = false;
        UpdateUI();
    }

    private void DecreaseTimeScale()
    {
        float newValue = Mathf.Clamp(timeScaleSlider.value - 0.1f, minTimeScale, maxTimeScale);
        timeScaleSlider.value = newValue;
        targetTimeScale = newValue;
        isPaused = false;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update text display
        timeScaleText.text = isPaused ? "Paused" : $"×{Time.timeScale:0.0}";

        // Update button states
        pauseButton.interactable = !isPaused || Time.timeScale > 0;
        normalSpeedButton.interactable = !Mathf.Approximately(targetTimeScale, normalTimeScale);
        fastForwardButton.interactable = !Mathf.Approximately(targetTimeScale, fastForwardScale);
        ultraSpeedButton.interactable = !Mathf.Approximately(targetTimeScale, ultraSpeedScale);

        // Visual feedback for current speed
        pauseButton.GetComponent<Image>().color = isPaused ? Color.red : Color.white;
        normalSpeedButton.GetComponent<Image>().color = Mathf.Approximately(targetTimeScale, normalTimeScale) ? Color.green : Color.white;
        fastForwardButton.GetComponent<Image>().color = Mathf.Approximately(targetTimeScale, fastForwardScale) ? Color.yellow : Color.white;
        ultraSpeedButton.GetComponent<Image>().color = Mathf.Approximately(targetTimeScale, ultraSpeedScale) ? new Color(1, 0.5f, 0) : Color.white;
    }

    // For external access
    public void SetTimeScale(float scale)
    {
        targetTimeScale = Mathf.Clamp(scale, minTimeScale, maxTimeScale);
        timeScaleSlider.value = targetTimeScale;
        isPaused = false;
        UpdateUI();
    }

    public void ResetTimeScale()
    {
        SetNormalSpeed();
    }
}