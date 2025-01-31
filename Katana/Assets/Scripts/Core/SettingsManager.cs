using System;
using System.Threading.Tasks;
using Assets.Scripts.PlayerCamera;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Assets.Scripts.Core
{
    public class SettingsManager : MonoBehaviour
    {
        [SerializeField] private VolumeProfile _dofProfile;
        [SerializeField] private VolumeProfile _menuDOFProfile;
        [SerializeField] private VolumeProfile _motionBlurProfile;
        [SerializeField] private AudioMixerGroup _master;
        [SerializeField] private AudioMixerGroup _sfx;
        [SerializeField] private AudioMixerGroup _music;
        
        private bool _useDOF;
        public bool UseDOF
        {
            get => _useDOF;
            private set
            {
                if (_useDOF == value) return;
                _useDOF = value;
                if (_menuDOFProfile != null && _dofProfile.TryGet(out DepthOfField dof)) dof.active = _useDOF;
                if (_menuDOFProfile != null && _menuDOFProfile.TryGet(out DepthOfField menuDOF)) menuDOF.active = _useDOF;
                PlayerPrefs.SetInt("UseDOF", UseDOF ? 1 : 0);
            }
        }

        private float _motionBlur;
        public float MotionBlur
        {
            get => _motionBlur;
            private set
            {
                if (Mathf.Approximately(_motionBlur, value)) return;
                _motionBlur = value;
                if (_motionBlurProfile != null && _motionBlurProfile.TryGet(out MotionBlur mb)) mb.intensity.value = _motionBlur;
                PlayerPrefs.SetFloat("MotionBlur", value);
            }
        }

        private Perspective _perspective;
        public Perspective Perspective 
        {
            get => _perspective;
            set
            {
                if (_perspective == value) return;
                _perspective = value;
                PlayerPrefs.SetString("Perspective", _perspective.ToString());
            }
        }

        private float _masterVolume;
        public float MasterVolume
        {
            get => _masterVolume;
            private set
            {
                if (Mathf.Approximately(_masterVolume, value)) return;
                _masterVolume = value;
                _master.audioMixer.SetFloat("MasterVolume", GetVolume(value));
                PlayerPrefs.SetFloat("MasterVolume", value);
            }
        }

        private float _sfxVolume;
        public float SFXVolume
        {
            get => _sfxVolume;
            private set
            {
                if (Mathf.Approximately(_sfxVolume, value)) return;
                _sfxVolume = value;
                _sfx.audioMixer.SetFloat("SFXVolume", GetVolume(value));
                PlayerPrefs.SetFloat("SFXVolume", value);
            }
        }

        private float _musicVolume;
        public float MusicVolume
        {
            get => _musicVolume;
            private set
            {
                if (Mathf.Approximately(_musicVolume, value)) return;
                _musicVolume = value;
                _music.audioMixer.SetFloat("MusicVolume", GetVolume(value));
                PlayerPrefs.SetFloat("MusicVolume", value);
            }
        }

        private void Awake() => LoadSettings();

        private async void LoadSettings()
        {
            UseDOF = PlayerPrefs.GetInt("UseDOF", 1) == 1;
            MotionBlur = PlayerPrefs.GetFloat("MotionBlur", 1);
            Perspective = (Perspective)Enum.Parse(typeof(Perspective), PlayerPrefs.GetString("Perspective", "First"));

            // Takes care of updating the UI
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
            
            // Wait a bit before updating the audio mixer cuz unity devs are competent
            await Task.Delay(TimeSpan.FromSeconds(0.01f));
            
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        }
        
        public void ChangeDOF(bool useDOF) => UseDOF = useDOF;
        public void ChangeMotionBlur(float mb) => MotionBlur = mb;
        public void ChangePerspective(int p) => Perspective = (Perspective)p;
        public void ChangeMasterVolume(float vol) => MasterVolume = vol;
        public void ChangeSFXVolume(float vol) => SFXVolume = vol;
        public void ChangeMusicVolume(float vol) => MusicVolume = vol;

        private float GetVolume(float t) => Mathf.Lerp(-80, 0, t);
    }
}