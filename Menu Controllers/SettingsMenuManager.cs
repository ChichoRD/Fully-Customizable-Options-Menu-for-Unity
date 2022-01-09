/*
Copyright(c) 2021 Chicho Studio

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files
(the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge,
publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
subject to the following conditions:

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO
THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
DEALINGS IN THE SOFTWARE.
*/

using UnityEngine;
using SettingsSystem.DataStorage;
using UnityEngine.Rendering.Universal;
using ShadowQuality = UnityEngine.ShadowQuality;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;
using TMPro;
using UnityEngine.Audio;

namespace SettingsSystem.Settings
{
    public class SettingsMenuManager : MonoBehaviour
    {
        [SerializeField] private bool applyDataAfterLoading = true;

        //Video
        [SerializeField] private TMP_Text frameRateText;
        [SerializeField] private TMP_Text vSyncText;

        [SerializeField] private TMP_InputField xResolutionField;
        [SerializeField] private TMP_InputField yResolutionField;

        [SerializeField] private TMP_Text fullScreenModeText;
        [SerializeField] private TMP_Text renderScaleText;

        //Audio
        [SerializeField] private AudioMixer musicMixer;
        [SerializeField] private AudioMixer sfxMixer;

        //Graphics
        [SerializeField] private TMP_Text qualityLevelText;

        [SerializeField] private TMP_Text shadowQualityText;
        [SerializeField] private TMP_Text shadowDistanceText;
        [SerializeField] private TMP_Text shadowCascadesText;
        [SerializeField] private TMP_Text shadowResolutionText;
        [SerializeField] private TMP_Text MSAAText;

        private const string ENUM_FORMAT = "G";
        private const string FLOAT_DISPLAY_FORMAT = "0.00";

        private byte vSyncCount;
        public byte VSyncCount
        {
            get { return vSyncCount; }

            set
            {
                vSyncCount = value;

                if (vSyncText == null) return;
                vSyncText.text = value switch
                {
                    > 0 => $"Enabled ({value} V Sync)",
                    _ => "Disabled",
                };
            }
        }

        private int frameRate;
        public int FrameRate
        {
            get { return frameRate; }
            set
            {
                frameRate = value;

                if (frameRateText == null) return;
                frameRateText.text = value switch
                {
                    -1 => "Unlimited",
                    _ => $"{value} fps",
                };
            }
        }

        private (int, int) screenResolution;
        public (int, int) ScreenResolution
        {
            get { return screenResolution; }
            set
            {
                screenResolution = value;

                if (xResolutionField == null || yResolutionField == null) return;
                const string PIXELS = "px";

                (xResolutionField.text, yResolutionField.text) = value switch
                {
                    ( > 0, > 0) => (value.Item1.ToString() + PIXELS, value.Item2 + PIXELS),
                    _ => (Screen.currentResolution.width.ToString() + PIXELS, Screen.currentResolution.height.ToString() + PIXELS),
                };
            }
        }

        private FullScreenMode fullScreenMode;

        public FullScreenMode FullScreenMode
        {
            get { return fullScreenMode; }
            set
            {
                fullScreenMode = value;

                if (fullScreenModeText == null) return;

                string screenModeText = value.ToString(ENUM_FORMAT);
                fullScreenModeText.text = screenModeText;
                //Debug.Log(screenModeText);
            }
        }

        private byte qualityLevel;
        public byte QualityLevel
        {
            get { return qualityLevel; }
            set
            {
                qualityLevel = value;

                if (qualityLevelText == null) return;
                qualityLevelText.text = QualitySettings.names[qualityLevel].ToString();
            }
        }

        private float renderScale;

        public float RenderScale
        {
            get { return renderScale; }
            set
            {
                renderScale = value;

                if (renderScaleText == null) return;

                renderScaleText.text = $"x{value}";
            }
        }

        private ShadowQuality shadowQuality;
        private float shadowDistance;
        private int shadowCascades;
        private ShadowResolution shadowResolution;
        private MsaaQuality msaa;

        public ShadowQuality ShadowQuality
        {
            get => shadowQuality;
            set
            {
                shadowQuality = value;

                if (shadowQualityText == null) return;

                shadowQualityText.text = value.ToString(ENUM_FORMAT);
            }
        }

        public float ShadowDistance
        {
            get => shadowDistance;
            set
            {
                shadowDistance = value;

                if (shadowDistanceText == null) return;

                shadowDistanceText.text = value.ToString(FLOAT_DISPLAY_FORMAT);
            }
        }

        public int ShadowCascades
        {
            get => shadowCascades;
            set
            {
                shadowCascades = value;

                if (shadowCascadesText == null) return;

                shadowCascadesText.text = value.ToString();
            }
        }

        public ShadowResolution ShadowResolution
        {
            get => shadowResolution;
            set
            {
                shadowResolution = value;
                if (shadowResolutionText == null) return;

                shadowResolutionText.text = value.ToString(ENUM_FORMAT);
            }
        }

        public MsaaQuality Msaa
        {
            get => msaa;
            set
            {
                msaa = value;
                if (MSAAText == null) return;

                MSAAText.text = value.ToString(ENUM_FORMAT).Replace("_", string.Empty);
            }
        }

        private void OnEnable()
        {
            LoadAllData();

            if (applyDataAfterLoading)
                ApplyAllLoadedData();
        }

        private void OnDisable() => SaveAllData();

        private UniversalRenderPipelineAsset RenderPipelineAsset => QualitySettings.renderPipeline is UniversalRenderPipelineAsset universalRender ? universalRender : null;

        public void LoadAndApplyAllData()
        {
            LoadAllData();
            ApplyAllLoadedData();
        }

        public void ApplyAllLoadedData()
        {
            ApplyVideoChanges();
            ApplyGraphicChanges();
            ApplyAudioChanges();
        }

        public void ResetAndApplyAllData()
        {
            ApplyVideoChanges(reset: true);
            ApplyGraphicChanges(reset: true);
            ApplyAudioChanges(reset: true);
        }

        public void ApplyVideoChanges(bool reset = false)
        {
            var data = reset ? new() : SaveManager.CustomizationData;

            // Video
            SetVSyncCount(data.vSyncCount);
            SetFrameRate(data.frameRate);
            SetScreenResolution(data.screenResolution);
            SetFullScreenMode(data.screenMode);
            SetRenderScale(data.renderScale);
        }

        public void ApplyGraphicChanges(bool reset = false)
        {
            var data = reset ? new() : SaveManager.CustomizationData;

            // Graphics
            SetQualityLevel(data.qualityLevel);
            SetShadowQuality(data.shadowQuality);
            SetShadowDistance(data.shadowDistance);
            SetShadowCascadesCount(data.shadowCascadesCount);
        }

        public void ApplyAudioChanges(bool reset = false)
        {
            var data = reset ? new() : SaveManager.CustomizationData;

            //Audio
            SetMasterVolume(data.rawMasterVolume);
            SetMusicVolume(data.rawMusicVolume);
            SetSFXVolume(data.rawSFXVolume);
        }

        public void LoadAllData()
        {
            SaveManager.CustomizationData = SaveManager.Load<CustomizationData>(DocType.CustomizationData) ?? new();
            SaveManager.PLayerData = SaveManager.Load<PLayerData>(DocType.PlayerData) ?? new();
            SaveManager.GeneralData = SaveManager.Load<GeneralData>(DocType.GeneralData) ?? new();
        }

        public void SaveAllData()
        {
            SaveManager.Save(SaveManager.CustomizationData, DocType.CustomizationData);
            SaveManager.Save(SaveManager.PLayerData, DocType.PlayerData);
            SaveManager.Save(SaveManager.GeneralData, DocType.GeneralData);
        }

        public void SetVSyncCount(int count)
        {
            QualitySettings.vSyncCount = count;
            VSyncCount = (byte)count;
            SaveManager.CustomizationData.vSyncCount = (byte)count;
        }

        public void SetVSyncCount(float count) => SetVSyncCount((byte)count);

        public void SetFrameRate(int rate)
        {
            const int MAX_REFRESH_RATE = 360;

            if (rate > MAX_REFRESH_RATE) rate = -1;

            Application.targetFrameRate = rate;
            FrameRate = rate;
            SaveManager.CustomizationData.frameRate = rate;
        }

        public void SetFrameRate(float rate) => SetFrameRate((int)rate);

        public void SetScreenResolution(Vector2Int resolution)
        {
            if (resolution.x < 1 || resolution.y < 1) resolution = new(Screen.width, Screen.height);

            Screen.SetResolution(resolution.x, resolution.y, Screen.fullScreenMode);
            ScreenResolution = (resolution.x, resolution.y);
            SaveManager.CustomizationData.screenResolution = resolution;
        }

        public void SetScreenResolution()
        {
            if (xResolutionField == null || yResolutionField == null) return;

            if (!int.TryParse(xResolutionField.text, out int x) || !int.TryParse(yResolutionField.text, out int y)) return;

            SetScreenResolution(new Vector2Int(x, y));
        }

        public void SetAutoScreenResolution() => SetScreenResolution(new Vector2Int(-1, -1));

        public void SetFullScreenMode(FullScreenMode screenMode)
        {
            Screen.fullScreenMode = screenMode;
            FullScreenMode = screenMode;
            SaveManager.CustomizationData.screenMode = screenMode;
        }

        public void SetFullScreenMode(int screenMode) => SetFullScreenMode((FullScreenMode)screenMode);

        public void SetPostProcessing(bool allow)
        {
            //Asignment
            SaveManager.CustomizationData.postProcessing = allow;
        }

        public void SetQualityLevel(int quality)
        {
            QualitySettings.SetQualityLevel(quality, applyExpensiveChanges: true);
            QualityLevel = (byte)quality;
            SaveManager.CustomizationData.qualityLevel = (byte)quality;
        }

        public void SetQualityLevel(float quality) => SetQualityLevel((int)quality);

        public void SetFieldOfView(float fieldOfView)
        {
            //Asignment
            SaveManager.CustomizationData.fieldOfView = fieldOfView;
        }

        public void SetSensitivity(float rawSensitivity)
        {
            //Asignment

            const float BASE = 2f;
            float sensitivity = Mathf.Pow(BASE, rawSensitivity);

            SaveManager.CustomizationData.sensitivity = sensitivity;
        }

        public void SetCameraShake(float shakeValue)
        {
            //Asignment

            SaveManager.CustomizationData.cameraShake = shakeValue;
        }

        public void EnableVisualTips(bool enable)
        {
            //Asignment

            SaveManager.CustomizationData.visualTips = enable;
        }

        private const float AUDIO_FACTOR = 20f;
        private const string AUDIO_VOLUME_PARAMETER = "Volume";

        public void SetMasterVolume(float rawVolume)
        {
            // 0f - 1f
            AudioListener.volume = rawVolume;

            SaveManager.CustomizationData.rawMasterVolume = rawVolume;
        }

        public void SetSFXVolume(float rawVolume)
        {
            // -80f - 20f
            sfxMixer.SetFloat(AUDIO_VOLUME_PARAMETER, Mathf.Log10(rawVolume) * AUDIO_FACTOR);

            SaveManager.CustomizationData.rawSFXVolume = rawVolume;
        }

        public void SetMusicVolume(float rawVolume)
        {
            // -80f - 20f
            musicMixer.SetFloat(AUDIO_VOLUME_PARAMETER, Mathf.Log10(rawVolume) * AUDIO_FACTOR);

            SaveManager.CustomizationData.rawMusicVolume = rawVolume;
        }

        public void EnableTextSpeechSFX(bool enable)
        {
            //Asignment
            SaveManager.CustomizationData.enableTextSpeechSound = enable;
        }

        public void SetShadowQuality(ShadowQuality shadowQuality)
        {
            QualitySettings.shadows = shadowQuality;
            ShadowQuality = shadowQuality;
            SaveManager.CustomizationData.shadowQuality = shadowQuality;
        }

        public void SetShadowQuality(int shadowQuality) => SetShadowQuality((ShadowQuality)shadowQuality);
        public void SetShadowQuality(float shadowQuality) => SetShadowQuality((int)shadowQuality);

        public void SetShadowResolution(ShadowResolution shadowResolution)
        {
            //Asignment
            ShadowResolution = shadowResolution;
            SaveManager.CustomizationData.shadowResolution = shadowResolution;
        }

        public void SetShadowResolution(int shadowResolution) => SetShadowResolution((ShadowResolution)shadowResolution);
        public void SetShadowResolution(float shadowResolution) => SetShadowResolution((int)shadowResolution);

        public void SetShadowCascadesCount(int shadowCascadesCount)
        {
            RenderPipelineAsset.shadowCascadeCount = shadowCascadesCount;
            ShadowCascades = shadowCascadesCount;
            SaveManager.CustomizationData.shadowCascadesCount = (byte)shadowCascadesCount;
        }

        public void SetShadowCascadesCount(float shadowCascadesCount) => SetShadowCascadesCount((int)shadowCascadesCount);

        public void SetShadowDistance(float shadowDistance)
        {
            RenderPipelineAsset.shadowDistance = shadowDistance;
            ShadowDistance = shadowDistance;
            SaveManager.CustomizationData.shadowDistance = shadowDistance;
        }

        public void SetMSAAQuality(MsaaQuality rawMSAAQuality)
        {
            int msAAQuality = (int)Mathf.Pow(2, (int)rawMSAAQuality);

            RenderPipelineAsset.msaaSampleCount = msAAQuality;
            Msaa = (MsaaQuality)msAAQuality;
            SaveManager.CustomizationData.msaaQuality = rawMSAAQuality;
        }

        public void SetMSAAQuality(int msaaQuality) => SetMSAAQuality((MsaaQuality)msaaQuality);
        public void SetMSAAQuality(float msaaQuality) => SetMSAAQuality((int)msaaQuality);

        public void SetRenderScale(float renderScale)
        {
            renderScale = (float)System.Math.Round(renderScale, 2);
            RenderPipelineAsset.renderScale = renderScale;
            RenderScale = renderScale;
            SaveManager.CustomizationData.renderScale = renderScale;
        }
    }
}