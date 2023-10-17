using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using TMPro;

public class MenuOptions : MonoBehaviour
{
    //Enum utilizzato per capire a quale bottone si fa riferimento
    public enum BtnOption {
        scan,
        inventory,
        measure
    }

    public BtnOption btnOption; //Si assegna dall'inspector l'operazione che il bottone (al quale bisogna attaccare questo script deve eseguire)
    private PressableButton pb;

    [SerializeField] private GameObject inventory; //Contiene gameObject relativo all'inventario
    [SerializeField] private GameObject scanMenu; //Contiene gameObject relativo al menu di scansione
    [SerializeField] private Vector3 startInventoryPosition; //Posizione nella quale istanziare la finestra dell'inventario
    [SerializeField] private GameObject measureInfo; //Contiene gameObject relativo alla finestra per la misurazione
    [SerializeField] private Vector3 startMeasureInfoPosition; //Posizione nella quale istanziare la finestra per la misurazione
    [SerializeField] private Vector3 startScanMenuPosition; //Posizione nella quale istanziare la finestra per lo scan

    //private MeshRenderer textureRenderer;
    private GameObject pcModel; //Oggetto relativo a nuvola di punti

    void Start(){
        pb = GetComponent<PressableButton>();
        //textureRenderer = transform.Find("Texture").GetComponent<MeshRenderer>();

        //textureRenderer.material.color = new Color32(115, 166, 116, 255);

        pb.OnClicked.AddListener(MakeOperation);
        pb.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if(pb.IsPokeHovered)
                MakeOperation();
        });
    }

    private void Update() {
        if(btnOption == BtnOption.scan) {
            //Cambio colore bottone per scansione sulla base del fatto che il kinect è connesso o meno
            this.GetComponent<ButtonPressBehaviour>().SetColorByCondition(FindObjectOfType<KinectPointsCloud>() == null || !FindObjectOfType<KinectPointsCloud>().azureConnected || scanMenu.activeSelf, GetComponent<ButtonPressBehaviour>().negativeColor, GetComponent<ButtonPressBehaviour>().positiveColor);
            
            //Cambio anche testo
            if(FindObjectOfType<KinectPointsCloud>() == null || !FindObjectOfType<KinectPointsCloud>().azureConnected) {
                if(transform.Find("IconAndText").GetComponent<TextMeshPro>() != null) {
                    transform.Find("IconAndText").GetComponent<TextMeshPro>().text = "Kinect\nnot\navailable";
                    transform.Find("IconAndText").GetComponent<TextMeshPro>().fontSize = 0.3f;
                }
            }
            else {
                if (transform.Find("IconAndText").GetComponent<TextMeshPro>() != null) {
                    transform.Find("IconAndText").GetComponent<TextMeshPro>().fontSize = 0.3f;
                    transform.Find("IconAndText").GetComponent<TextMeshPro>().fontSize = 0.5f;
                }
            }
        }else if(btnOption == BtnOption.measure) {
            
            if(pcModel == null) {
                pcModel = GameObject.FindGameObjectWithTag("PCModel");
                if (transform.Find("IconAndText").GetComponent<TextMeshPro>() != null) {
                    transform.Find("IconAndText").GetComponent<TextMeshPro>().text = "Measure\nnot\navailable";
                }
            }
            else {
                if (transform.Find("IconAndText").GetComponent<TextMeshPro>() != null) {
                    transform.Find("IconAndText").GetComponent<TextMeshPro>().text = "Measure";
                }
            }

            //il colore del bottone per la misurazione cambia in base alla presenza o meno della nuvola di punti o se è stata abilitata o meno la misurazione
            if(pcModel != null) {
                this.GetComponent<ButtonPressBehaviour>().SetColorByCondition(!measureInfo.activeSelf, GetComponent<ButtonPressBehaviour>().positiveColor, GetComponent<ButtonPressBehaviour>().negativeColor);
            }
            else {
                this.GetComponent<ButtonPressBehaviour>().SetColorByCondition(pcModel != null && pcModel.GetComponent<MeshDistances>().canMeasure == false, GetComponent<ButtonPressBehaviour>().positiveColor, GetComponent<ButtonPressBehaviour>().negativeColor);
            }
        }
        else {
            this.GetComponent<ButtonPressBehaviour>().SetColorByCondition(inventory.activeSelf, GetComponent<ButtonPressBehaviour>().negativeColor, GetComponent<ButtonPressBehaviour>().positiveColor); //se l'inventario è attivo imposto colore rosso, altrimenti verde
        }
    }

    public void MakeOperation() { //Metodo che viene richiamato al click (in start()) o al poke enter (nell'inspector dei singoli bottoni)
        switch (btnOption) {
            case BtnOption.scan:
                ScanOperation();
                break;
            case BtnOption.inventory:
                InventoryOperation();
                break;
            case BtnOption.measure:
                MeasureOperation();
                break;
            default:
                InventoryOperation();
                break;
        }
    }

    private void MeasureOperation() {
        if (this.GetComponent<ButtonPressBehaviour>().CanBePressed()) {
            Debug.Log("Measure");
            if(pcModel != null) { //Se la nuvola di punti è presente
                pcModel.GetComponent<MeshDistances>().canMeasure = !pcModel.GetComponent<MeshDistances>().canMeasure; //abilito la misurazione
                if (!measureInfo.activeSelf) { //se lo devo istanziare
                    measureInfo.transform.localPosition = startMeasureInfoPosition; //imposto la posizione iniziale
                    if (measureInfo.GetComponent<WindowMove>() != null)
                        measureInfo.GetComponent<WindowMove>().fixedWindow = false;
                }
                measureInfo.SetActive(!measureInfo.activeSelf);
            }
            //transform.parent.parent.gameObject.SetActive(false);
        }
    }

    private void InventoryOperation() {
        if (this.GetComponent<ButtonPressBehaviour>().CanBePressed()) {
            if (!inventory.activeSelf) {
                inventory.transform.localPosition = startInventoryPosition;
                if (inventory.GetComponent<WindowMove>() != null)
                    inventory.GetComponent<WindowMove>().fixedWindow = false;
            }
            inventory.SetActive(!inventory.activeSelf);
            //transform.parent.parent.gameObject.SetActive(false);
        }
    }

    private void ScanOperation() {
        //Se trova l'oggetto contenente il component pointsCloud
        if (FindObjectOfType<KinectPointsCloud>() != null && this.GetComponent<ButtonPressBehaviour>().CanBePressed() && FindObjectOfType<KinectPointsCloud>().azureConnected) {
            //FindObjectOfType<KinectPointsCloud>().SharedAcquisitionOpt();
            if (!scanMenu.activeSelf) {
                scanMenu.transform.localPosition = startScanMenuPosition;
                if (scanMenu.GetComponent<WindowMove>() != null)
                    scanMenu.GetComponent<WindowMove>().fixedWindow = false;
            }
            scanMenu.SetActive(!scanMenu.activeSelf);
            //transform.parent.parent.gameObject.SetActive(false);
        }
    }
}
