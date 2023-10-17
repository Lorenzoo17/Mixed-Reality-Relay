using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonPressBehaviour : MonoBehaviour
{
    private PressableButton pb;

    //Abilitazione pressione bottone
    [SerializeField] private float pressRate; //Ogni quanto può essere premuto
    private float timeBtwPress;
    private bool canPress = true;

    //Animazioni post pressione tasto
    private Vector3 startScale;
    [SerializeField] private float scaleAnimationDuration = 0.1f; //durata passaggio da scale attuale a iniziale dopo cambiamento di quest'ultimo
    [SerializeField] private float scaleModifier; //Di quanto deve essere cambiato lo scale

    [SerializeField] private Color32 startColor;
    [SerializeField] public Color32 positiveColor = new Color32(219, 219, 219, 255);
    [SerializeField] public Color32 negativeColor = new Color32(241, 95, 95, 255);
    private MeshRenderer textureRenderer;
    private SpriteRenderer iconSlot, icon, iconUnderline;

    [SerializeField] private Transform objectToRescale;

    // Start is called before the first frame update
    void Start(){
        pb = GetComponent<PressableButton>();
        SetPressRate();

        if(textureRenderer == null)
            textureRenderer = transform.Find("Texture").GetComponent<MeshRenderer>();

        textureRenderer.material.color = startColor;

        if(transform.Find("IconSlot") != null) {
            iconSlot = transform.Find("IconSlot").GetComponent<SpriteRenderer>();
            iconSlot.color = startColor;

            icon = transform.Find("IconSlot").Find("Icon").GetComponent<SpriteRenderer>();

            if (iconSlot.transform.Find("Icon_Underline") != null) {
                iconUnderline = iconSlot.transform.Find("Icon_Underline").GetComponent<SpriteRenderer>();
                iconUnderline.color = startColor;
            }
        }

        if(objectToRescale == null) {
            objectToRescale = this.gameObject.transform;
        }

        startScale = objectToRescale.localScale;

        pb.OnClicked.AddListener(PressEffect); //Dopo il click bisogna aspettare pressRate prima che possa essere premuto di nuovo
        pb.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if(pb.IsPokeHovered)
                PressEffect();
        });
    }

    private void Update() {
        if(FindObjectOfType<ArticulatedHandController>() != null) { //Qualche effetto all'avvicinamento della mano
            Transform hand = FindObjectOfType<ArticulatedHandController>().transform;
            Vector3 handWorldPosition = hand.parent.TransformPoint(hand.localPosition);
            Vector3 buttonWorldPosition = transform.parent.TransformPoint(transform.localPosition);

            //Some effect
        }

        ResetPress();
        ResetScale();

        IconAnimation(pb.IsRayHovered);
    }
    private void ResetPress() { //Aspetto pressRate secondi prima che il bottone possa nuovamente essere premuto
        if (!canPress) {
            if (timeBtwPress <= 0) {
                SetPressRate();
                canPress = true;
            }
            else {
                timeBtwPress -= Time.deltaTime;
            }
        }
    }

    private void SetPressRate() {
        timeBtwPress = pressRate;
    }

    public bool CanBePressed() { return canPress; }

    private void PressEffect() { //Operazioni da svolgere alla pressione del bottone
        if (canPress) {
            canPress = false;

            objectToRescale.localScale = startScale * scaleModifier;
        }
    }

    private void ResetScale() {
        if(objectToRescale.localScale != startScale)
            objectToRescale.localScale = Vector3.Lerp(objectToRescale.localScale, startScale, scaleAnimationDuration); //effettuo interpolazione tra scale attuale e quello iniziale quando esso è diverso da quello inziale
    }

    private void IconAnimation(bool condition) {
        try {
            this.GetComponent<Animator>().SetBool("Hovered", condition);
        }
        catch {

        }
    }

    public void SetColorByCondition(bool condition, Color32 trueColor, Color32 falseColor) { //Metodo utilizzato per assegnare al bottone un colore piuttosto che un altro in base ad una specifica condizione
        if (condition) {
            textureRenderer.material.color = trueColor;
            /*
            if (iconSlot != null)
                iconSlot.color = trueColor;
            if(iconUnderline != null)
                iconUnderline.color = trueColor;
            */
            
        }
        else {
            textureRenderer.material.color = falseColor;
            /*
            if (iconSlot != null)
                iconSlot.color = falseColor;
            if(iconUnderline != null)
                iconUnderline.color = falseColor;
            */
            
        }
    }
}
