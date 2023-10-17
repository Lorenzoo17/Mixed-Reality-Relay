using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class ChoosekinectSettings : MonoBehaviour {
    
    //Impostazioni di acquisizione, vengono cambiate da questo script ed agiscono sul tipo di acquisizione (singola o real time) e dalla complessità del modello scansionato (numero vertici e frame rate)
    public enum KinectSetting {
        performance,
        quality,
        staticScan
    }

    [SerializeField] private PressableButton btnRealTimeSettings; //Bottone che viene utilizzato per passare da una modalità all'altra (performance <-> quality) nel caso dell'acquisizione real time
    public KinectSetting currentRealTimeSettings; //impostazioni correnti real time
    private KinectSetting currentSettings; //impostazioni correnti in generale
    [SerializeField] private TextMeshPro settingsInfo;

    public KinectSetting GetCurrentRealTimeSettings() {
        return currentRealTimeSettings;
    }
    public KinectSetting GetCurrentSetting() {
        return currentSettings;
    }
    private void Start() {

        currentRealTimeSettings = KinectSetting.performance; //Inizializzo a performance mode
        settingsInfo.text = currentRealTimeSettings.ToString();

        btnRealTimeSettings.OnClicked.AddListener(() => { //Al click del bottone passo a performance o quality
            if(currentRealTimeSettings == KinectSetting.performance) {
                ChangeKinectSettings(KinectSetting.quality);
            }
            else {
                ChangeKinectSettings(KinectSetting.performance);
            }
        });
        btnRealTimeSettings.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if (btnRealTimeSettings.IsPokeHovered) {
                if (currentRealTimeSettings == KinectSetting.performance) {
                    ChangeKinectSettings(KinectSetting.quality);
                }
                else {
                    ChangeKinectSettings(KinectSetting.performance);
                }
            }
        });
    }

    public void ChangeKinectSettings(KinectSetting setting) {
        if(setting != KinectSetting.staticScan)
            currentRealTimeSettings = setting; //Real time settings li lascio invariati se non sono andato a modificare questi ultimi
        currentSettings = setting;
        settingsInfo.text = currentRealTimeSettings.ToString();
        KinectPointsCloud kinectManager = FindObjectOfType<KinectPointsCloud>();
        if (kinectManager != null && kinectManager.azureConnected) {
            int materialNumber;
            int vertexToDiscard;
            float fps;
            DeviceConfiguration config;
            (materialNumber, vertexToDiscard, fps, config) = GetDeviceSetting(setting); //in base ai setting passati come parametro, definisco differenti valori di materialNumber, vertexToDiscard, fps e config

            kinectManager.SetRealTimeSetting(materialNumber, vertexToDiscard, fps, currentSettings);//Che poi assegno ai valori nel kinect manager
            kinectManager.RestartDevice(config);//Effettuo il restart del device in base alle impostazioni definite
        }
    }

    //Funzione che in base alle impostazioni selezionate ritorno : materiale da assegnare al mesh della nuvola di punti, numero di vertici da scartare (1 -> nessuno ; 2 -> la meta'), framerate del video, configurazione)
    private (int, int, float, DeviceConfiguration) GetDeviceSetting(KinectSetting setting) {
        DeviceConfiguration config = new DeviceConfiguration();
        int materialNumber;
        //Sulla base dei settings imposto differenti valori di configurazione, frameRate, vertici da scartare e materiale della nuvola di punti
        if (setting == KinectSetting.performance) {
            config.CameraFPS = FPS.FPS30;
            config.ColorFormat = ImageFormat.ColorBGRA32;
            config.ColorResolution = ColorResolution.R720p;
            config.DepthMode = DepthMode.NFOV_2x2Binned; //immagine depth 320x288
            config.SynchronizedImagesOnly = true; 

            materialNumber = 0;
            return (materialNumber, 2, 0.1f, config);
        }
        else if(setting == KinectSetting.quality) {
            config.CameraFPS = FPS.FPS30;
            config.ColorFormat = ImageFormat.ColorBGRA32;
            config.ColorResolution = ColorResolution.R720p;
            config.DepthMode = DepthMode.NFOV_2x2Binned; //immagine depth 320x288
            config.SynchronizedImagesOnly = true; 

            materialNumber = 1;
            return (materialNumber, 1, 0.2f, config);
        }
        else {
            config.CameraFPS = FPS.FPS15;
            config.ColorFormat = ImageFormat.ColorBGRA32;
            config.ColorResolution = ColorResolution.R720p;
            config.DepthMode = DepthMode.NFOV_Unbinned; //immagine depth 1024x1024
            config.SynchronizedImagesOnly = true; 

            materialNumber = 1;
            return (materialNumber, 1, 0.2f, config);
        }
    }
}