using System;
using Assets.Scripts.Core;
using Assets.Scripts.PlayerCamera;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.MainMenu
{
    public class SettingsMenu : MonoBehaviour
    {
        private static SettingsManager Settings => GameManager.SettingsManager;
        
        [SerializeField] private Toggle _useDOF;
        [SerializeField] private Slider _motionBlur;
        [SerializeField] private TMP_Dropdown _perspective;
        [SerializeField] private Slider _masterVolume;
        [SerializeField] private Slider _sfxVolume;
        [SerializeField] private Slider _musicVolume;
        
        private void Start() => UpdateUI();

        private void UpdateUI()
        {
            _useDOF.isOn = Settings.UseDOF;
            _motionBlur.value = Settings.MotionBlur;
            _perspective.value = Array.IndexOf(Enum.GetValues(typeof(Perspective)), Settings.Perspective);
            _masterVolume.value = Settings.MasterVolume;
            _sfxVolume.value = Settings.SFXVolume;
            _musicVolume.value = Settings.MusicVolume;
        }
    }
}