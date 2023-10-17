using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine.XR.Interaction.Toolkit;

public class ScanOptions : MonoBehaviour
{
    //Per identificare l'effetto del bottone
    public enum ScanTypeButton {
        staticScanBtn,
        realTimeScanBtn
    }

    private PressableButton pb;
    [SerializeField] private ChoosekinectSettings kinectSettings;
    [SerializeField] private ScanTypeButton btnType;

    private void Start() {
        pb = GetComponent<PressableButton>();

        //Al click del bottone eseguo il metodo
        pb.OnClicked.AddListener(() => {
            MakeScanOperation();
        });

        //Al poke hover eseguo il metodo
        pb.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if (pb.IsPokeHovered) {
                MakeScanOperation();
            }
        });
    }

    private void Update() {
        if(btnType == ScanTypeButton.realTimeScanBtn) {
            if(FindObjectOfType<KinectPointsCloud>() != null) {
                this.GetComponent<ButtonPressBehaviour>().SetColorByCondition(FindObjectOfType<KinectPointsCloud>().captureVideo, GetComponent<ButtonPressBehaviour>().negativeColor, GetComponent<ButtonPressBehaviour>().positiveColor);
            }
        }
    }

    private void MakeScanOperation() {
        if (btnType == ScanTypeButton.staticScanBtn) { //Se il bottone è per la scansione singola
            if(kinectSettings.GetCurrentSetting() != ChoosekinectSettings.KinectSetting.staticScan) //Se le impostazioni di acquisizione non sono per la staticScan
                kinectSettings.ChangeKinectSettings(ChoosekinectSettings.KinectSetting.staticScan); //Cambio in static scan
            
            //Debug.Log("Start static scan");

            KinectPointsCloud kinect = FindObjectOfType<KinectPointsCloud>();
            if (kinect != null) { //Se trovo nella scene l'oggetto relativo al kinect
                kinect.captureVideo = false; //Interrompo eventuale acquisizione "video"
                kinect.SharedAcquisitionOpt(); //Effettuo scansione singola
            }
        }
        else if(btnType == ScanTypeButton.realTimeScanBtn) { //Se il bottone è per acquisizione real time
            if (kinectSettings.GetCurrentSetting() == ChoosekinectSettings.KinectSetting.staticScan) //Se le impostazioni di scansione sono per la staticScan
                kinectSettings.ChangeKinectSettings(kinectSettings.GetCurrentRealTimeSettings()); //Le imposto per l'acquisizione real time
            
            //Debug.Log("Start video");

            KinectPointsCloud kinect = FindObjectOfType<KinectPointsCloud>();
            if (kinect != null) {
                kinect.captureVideo = !kinect.captureVideo; //Abilito / disabilito acquisizione
            }
        }
    }


    //Appunto:
    //In questo modo ogni volta che si fa un video bisogna comunque fare change settings
    //Vedere se mettere la possibilità di fare foto a qualità minore, non permettendo di fare video con qualità massima

    //Cambiare negli altri script il pokeHovered e farlo come in questo script e come in ChooseKinectSettings
}
