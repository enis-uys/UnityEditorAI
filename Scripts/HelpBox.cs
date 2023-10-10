using UnityEditor;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class HelpBox
{
    private static HelpBox instance;

    private DateTime lastFunctionCallTime;
    private readonly int timerThresholdInMilliseconds = 600;
    private float pogressBarProgress = 0f;
    private float intendedProgress = 1f;
    private bool isProgressBarActive = false;
    private readonly int progressBarDelayInMilliseconds = 1500;

    public static HelpBox GetInstance()
    {
        instance ??= new HelpBox();
        return instance;
    }

    public MessageType HBMessageType { get; set; }

    public string HBMessage { get; set; }

    public float IntendedProgress { get; set; }

    public int ProgressBarDelayInMilliseconds => progressBarDelayInMilliseconds;

    private void UpdateLastFunctionCalltime()
    {
        lastFunctionCallTime = DateTime.Now;
    }

    private double TimeSinceLastFunctionInMilliseconds()
    {
        if (lastFunctionCallTime == null)
        {
            return timerThresholdInMilliseconds;
        }
        else
        {
            TimeSpan elapsed = DateTime.Now - lastFunctionCallTime;
            return elapsed.TotalMilliseconds;
        }
    }

    private int TimerDelayInMilliseconds()
    {
        return Mathf.RoundToInt(
            Mathf.Max(
                0f,
                timerThresholdInMilliseconds - (float)TimeSinceLastFunctionInMilliseconds()
            )
        );
    }

    public void UpdateMessage(
        string message,
        MessageType? UpdateMessageType,
        bool shouldAppend = false,
        bool debugMessage = false
    )
    {
        int timerDelayInMilliseconds = TimerDelayInMilliseconds();
        if (UpdateMessageType.HasValue && UpdateMessageType.Value == MessageType.Error)
        {
            timerDelayInMilliseconds = 0;
        }

        Task.Delay(timerDelayInMilliseconds)
            .ContinueWith(_ =>
            {
                if (UpdateMessageType.HasValue)
                {
                    HBMessageType = UpdateMessageType.Value;
                }
                DebugMessageIfShould(debugMessage, message, UpdateMessageType ?? HBMessageType);

                if (shouldAppend)
                {
                    HBMessage += "\n" + message;
                }
                else
                {
                    HBMessage = message;
                }

                UpdateLastFunctionCalltime();
            });
    }

    public void RemoveMessage(int milliSeconds)
    {
        Task.Delay(milliSeconds)
            .ContinueWith(_ =>
            {
                HBMessage = "";
                HBMessageType = MessageType.None;
            });
    }

    public void SetProgressBarProgress(float progress)
    {
        pogressBarProgress = progress;
        if (!isProgressBarActive)
        {
            SetProgressBarActive(true);
        }
        else if (progress >= 1.0f)
        {
            FinishProgressBarWithDelay(ProgressBarDelayInMilliseconds);
        }
    }

    public void SetProgressBarActive(bool isActive)
    {
        isProgressBarActive = isActive;
    }

    public void UpdateIntendedProgress(float progress)
    {
        intendedProgress = Mathf.Clamp01(progress);
    }

    public void FinishProgressBarWithDelay(int delayInMilliseconds)
    {
        Task.Delay(delayInMilliseconds / 2)
            .ContinueWith(_ =>
            {
                pogressBarProgress = 1.0f;
                Task.Delay(delayInMilliseconds)
                    .ContinueWith(_ =>
                    {
                        SetProgressBarActive(false);
                        pogressBarProgress = 0.0f;
                    });
            });
    }

    public void RenderProgressBar()
    {
        if (isProgressBarActive)
        {
            EditorGUILayout.Space();
            EditorGUI.ProgressBar(EditorGUILayout.GetControlRect(), pogressBarProgress, "Progress");
            EditorGUILayout.Space();
        }
    }

    public void UpdateProgressBar()
    {
        if (!isProgressBarActive)
        {
            SetProgressBarActive(true);
        }
        pogressBarProgress += 0.01f; // Simulate progress by incrementing the value.
        if (pogressBarProgress >= intendedProgress)
        {
            //Remove from update event
            EditorApplication.update -= UpdateProgressBar;
        }
        if (pogressBarProgress >= 1.0f)
        {
            FinishProgressBarWithDelay(ProgressBarDelayInMilliseconds);
            intendedProgress = 1.0f;
        }
    }

    public void DebugMessageIfShould(
        bool shouldDebugLog,
        string customMessage,
        MessageType messageTypeToLog
    )
    {
        if (shouldDebugLog)
        {
            if (messageTypeToLog == MessageType.Error)
            {
                Debug.LogError(customMessage);
            }
            else if (messageTypeToLog == MessageType.Warning)
            {
                Debug.LogWarning(customMessage);
            }
            else
            {
                Debug.Log(customMessage);
            }
        }
    }
}
