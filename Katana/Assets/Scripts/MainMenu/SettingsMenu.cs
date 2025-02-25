using System;
using System.Collections;
using Assets.Scripts.Core;
using Assets.Scripts.PlayerCamera;
using NnUtils.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MainMenu
{
    public class SettingsMenu : MonoBehaviour
    {
        private static SettingsManager Settings => GameManager.SettingsManager;
        
        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown _perspective;
        [SerializeField] private Slider _motionBlur;
        [SerializeField] private Toggle _useDOF;
        
        [Header("Audio")]
        [SerializeField] private Slider _masterVolume;
        [SerializeField] private Slider _sfxVolume;
        [SerializeField] private Slider _musicVolume;
        
        [Header("Tabs")]
        [SerializeField] private RectTransform _tabsPanel;
        [SerializeField] private float _animationTime;
        [SerializeField] private Easings.Type _animationEasing;
        [SerializeField] private AnimationCurve _animationCurve;
        private float _tabSize;
        
        private void Start()
        {
            _tabSize = _tabsPanel.rect.width;
            UpdateUI();
        }

        private void UpdateUI()
        {
            _useDOF.isOn = Settings.UseDOF;
            _motionBlur.value = Settings.MotionBlur;
            _perspective.value = Array.IndexOf(Enum.GetValues(typeof(Perspective)), Settings.Perspective);
            _masterVolume.value = Settings.MasterVolume;
            _sfxVolume.value = Settings.SFXVolume;
            _musicVolume.value = Settings.MusicVolume;
        }

        public void SwitchTab(int tabIndex) =>
            this.RestartRoutine(ref _switchTabRoutine, SwitchTabRoutine(-tabIndex * _tabSize));

        private Coroutine _switchTabRoutine;
        private IEnumerator SwitchTabRoutine(float targetX)
        {
            var startPos = _tabsPanel.anchoredPosition;
            var targetPos = startPos;
            targetPos.x = targetX;
            
            float lerpPos = 0;
            while (lerpPos < 1)
            {
                var t = _animationCurve.Evaluate(Misc.Tween(ref lerpPos, _animationTime, _animationEasing, true));
                _tabsPanel.anchoredPosition = Vector2.LerpUnclamped(startPos, targetPos, t);
                yield return null;
            }
            
            _switchTabRoutine = null;
        }
    }
}