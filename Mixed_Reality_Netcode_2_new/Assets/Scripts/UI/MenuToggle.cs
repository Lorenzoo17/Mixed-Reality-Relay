using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine.XR.Interaction.Toolkit;

using TMPro;

public class MenuToggle : MonoBehaviour
{
    private GameObject mainMenu; //gameObject che rappresenta il menu
    private PressableButton pb;

    [SerializeField] private Vector3 menuStartPosition; //posizione iniziale nella quale il menu deve essere istanziato

    public TextMeshPro btnText;

    // Start is called before the first frame update
    void Start(){
        mainMenu = transform.parent.Find("MainMenu").gameObject;
        btnText = transform.Find("IconAndText").GetComponent<TextMeshPro>();

        pb = GetComponent<PressableButton>(); 

        pb.OnClicked.AddListener(ToggleMainMenu);
        pb.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if (pb.IsPokeHovered)
                ToggleMainMenu();
        });
    }

    public void ToggleMainMenu() {
        if(mainMenu != null && this.GetComponent<ButtonPressBehaviour>().CanBePressed()) { //Se il menu è presente e il bottone può essere premuto
            if (!mainMenu.activeSelf) { //Se non è attivo
                mainMenu.SetActive(true); //attivo
                mainMenu.transform.localPosition = menuStartPosition; //Resetto la posizione a quella iniziale 
                //change button icon
                ChangeIcon(true);
            }
            else {

                //mainMenu.SetActive(false); //disattivo
                //ChangeIcon(false);
            }
        }
    }

    public void ChangeIcon(bool open) {
        if (open) {
            btnText.text = "X";
            btnText.fontSize = 1.5f;
            btnText.margin = Vector4.zero;
        }
        else {
            btnText.text = "-\n-\n-";
            btnText.fontSize = 3;
            btnText.margin = new Vector4(0, -0.05f, 0f, 0f);
        }
    }
}
