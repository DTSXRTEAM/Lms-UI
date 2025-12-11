using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using UnityEngine.EventSystems;
using System;

public class VideoUIController : MonoBehaviour
{
    [Header("Core")]
    public VideoPlayer player;

    [Header("UI - Play Controls")]
    public Button bottomPlayPauseButton;
    public Sprite bottomPlayIcon;
    public Sprite bottomPauseIcon;

    public Button centerPlayButton;        // the Button component on the overlay (visible when paused)
    public GameObject centerOverlay;       // root GameObject of the overlay (icon + background)
    public Sprite centerPlayIcon;          
    public Sprite centerPauseIcon;

    // Transparent full-area button used only while video is playing to detect taps to pause
    public Button videoAreaButton;

    [Header("UI - Other")]
    public Button rewindButton;
    public Button forwardButton;

    public Slider seekSlider;
    public TextMeshProUGUI currentTimeText;
    public TextMeshProUGUI totalTimeText;

    private bool isDraggingSlider = false;

    void Start()
    {
        // basic null checks
        if (player == null)
        {
            Debug.LogError("VideoPlayer not assigned.");
            enabled = false;
            return;
        }
        if (seekSlider == null)
        {
            Debug.LogError("SeekSlider not assigned.");
            enabled = false;
            return;
        }
        if (bottomPlayPauseButton == null)
        {
            Debug.LogError("Bottom Play/Pause Button not assigned.");
            enabled = false;
            return;
        }

        // If videoAreaButton not set, it's OK but tapping video won't pause
        if (videoAreaButton == null)
            Debug.LogWarning("videoAreaButton not assigned. Tapping the playing video will not pause it.");

        // Normalize slider
        seekSlider.minValue = 0f;
        seekSlider.maxValue = 1f;
        seekSlider.wholeNumbers = false;

        // Prepare and handlers
        player.prepareCompleted += OnPrepared;
        player.Prepare();

        // Wire bottom play/pause
        bottomPlayPauseButton.onClick.AddListener(() => TogglePlayPause());

        // Wire center overlay button if provided (overlay visible when paused)
        if (centerPlayButton != null)
            centerPlayButton.onClick.AddListener(() => TogglePlayPause());

        // Wire video area button if provided (active only while playing)
        if (videoAreaButton != null)
            videoAreaButton.onClick.AddListener(() => TogglePlayPause());

        if (rewindButton != null) rewindButton.onClick.AddListener(() => Skip(-10));
        if (forwardButton != null) forwardButton.onClick.AddListener(() => Skip(10));

        seekSlider.onValueChanged.AddListener(OnSeekDrag);
        AttachEventTriggerToSlider();

        // Start with UI synced (if video auto-plays OnPrepared will update again)
        UpdatePlayUI();
    }

    void OnPrepared(VideoPlayer vp)
    {
        double len = player.length;
        if (len <= 0 || double.IsNaN(len) || double.IsInfinity(len))
            totalTimeText.text = "00:00";
        else
            totalTimeText.text = FormatTime(len);

        seekSlider.SetValueWithoutNotify(0f);
        currentTimeText.text = FormatTime(0);

        // Start playback automatically if desired
        player.Play();
        UpdatePlayUI();
    }

    void Update()
    {
        if (!player.isPrepared) return;

        if (!isDraggingSlider && player.length > 0.0001)
        {
            float normalized = (float)(player.time / player.length);
            seekSlider.SetValueWithoutNotify(normalized);
        }

        currentTimeText.text = FormatTime(player.time);
    }

    // Centralized toggle used by both buttons and tap area
    public void TogglePlayPause()
    {
        if (player.isPlaying)
        {
            // When pausing, we want the center overlay visible again
            player.Pause();
        }
        else
        {
            // When starting from paused state via the overlay, we want overlay hidden
            player.Play();
        }

        UpdatePlayUI();
    }

    // Sync bottom icon, center overlay visibility and videoAreaButton active state
    void UpdatePlayUI()
    {
        bool playing = player != null && player.isPlaying;

        // Bottom control sprite
        if (bottomPlayPauseButton != null)
        {
            Image img = bottomPlayPauseButton.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = playing ? bottomPauseIcon : bottomPlayIcon;
                img.enabled = true;
            }
            else
            {
                var childImg = bottomPlayPauseButton.GetComponentInChildren<Image>();
                if (childImg != null) childImg.sprite = playing ? bottomPauseIcon : bottomPlayIcon;
            }
        }

        // Center overlay: visible when paused, hidden when playing
        if (centerOverlay != null)
        {
            centerOverlay.SetActive(!playing);

            // If overlay has a button image and icons provided, update it
            if (centerPlayButton != null)
            {
                Image centerImg = centerPlayButton.GetComponent<Image>() ?? centerPlayButton.GetComponentInChildren<Image>();
                if (centerImg != null)
                {
                    // Use play icon when paused; optionally support pause icon if you keep overlay visible during play
                    centerImg.sprite = !playing ? (centerPlayIcon ?? bottomPlayIcon) : (centerPauseIcon ?? bottomPauseIcon);
                    centerImg.enabled = true;
                }
            }
        }

        // Enable the transparent videoTap area only while playing so taps pause and show overlay
        if (videoAreaButton != null)
        {
            videoAreaButton.gameObject.SetActive(playing);
        }
    }

    void Skip(double seconds)
    {
        if (!player.isPrepared) return;

        double t = player.time + seconds;
        t = Mathf.Clamp((float)t, 0f, (float)player.length);
        player.time = t;

        if (player.length > 0.0001)
            seekSlider.SetValueWithoutNotify((float)(player.time / player.length));

        isDraggingSlider = false;
        currentTimeText.text = FormatTime(player.time);

        UpdatePlayUI();
    }

    void OnSeekDrag(float val)
    {
        if (player.isPrepared && player.length > 0.0001)
        {
            double previewTime = val * player.length;
            currentTimeText.text = FormatTime(previewTime);
        }
        isDraggingSlider = true;
    }

    void AttachEventTriggerToSlider()
    {
        EventTrigger trigger = seekSlider.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = seekSlider.gameObject.AddComponent<EventTrigger>();

        var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
        entryDown.callback.AddListener((data) => { BeginDrag(); });
        trigger.triggers.Add(entryDown);

        var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
        entryUp.callback.AddListener((data) => { EndDrag(); });
        trigger.triggers.Add(entryUp);

        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => { if (isDraggingSlider) EndDrag(); });
        trigger.triggers.Add(entryExit);
    }

    public void BeginDrag()
    {
        isDraggingSlider = true;
    }

    public void EndDrag()
    {
        if (!player.isPrepared) return;

        double target = seekSlider.value * player.length;
        player.time = target;

        seekSlider.SetValueWithoutNotify((float)(player.time / player.length));
        currentTimeText.text = FormatTime(player.time);

        isDraggingSlider = false;
        UpdatePlayUI();
    }

    // kept for backward compatibility
    public void SeekEnd() { EndDrag(); }

    string FormatTime(double t)
    {
        if (double.IsNaN(t) || t < 0) return "00:00";
        int totalSec = Mathf.FloorToInt((float)t);
        int minutes = totalSec / 60;
        int seconds = totalSec % 60;
        return $"{minutes:00}:{seconds:00}";
    }
}
