using System;
using GameServices;
using Interfaces;
using TMPro;
using UnityEngine;

public class TimeDisplayWidget : MonoBehaviour
{
    [SerializeField] private TMP_Text timeDisplayText;
    private ClockService clockService;
    private void Start()
    {
        clockService = ServiceLocator.GetService<ClockService>();
    }
    
    private void OnEnable() { TickService.Instance.OnTick += ProgressTime; }
    private void OnDisable() { TickService.Instance.OnTick -= ProgressTime; }

    private void ProgressTime()
    {
        timeDisplayText.text = clockService.GetFormattedTime();
    }
}
