using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;

public class SelectAvatarManager : MonoBehaviour
{
    [Header("Buttons")]
    public PressableButton nextBtn;
    public PressableButton prevBtn;
    public float pressRate; //ogni quanto posso premere un bottone
    private float currentPressTime;

    [Header("Avatar")]
    public Transform avatarVisualizer; //gameObject avente come child gli avatar
    public PressableButton avatarSelector; //"bottone" per selezionare avatar
    public float rotationSpeed;

    public int avatar_selector; //avatar corrente
    private int numberOfAvatars; //numero totale di avatar


    // Start is called before the first frame update
    void Start()
    {   
        numberOfAvatars = avatarVisualizer.childCount;
        avatar_selector = 0;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAvatarVisualizer();
        AvatarRotationAnim();

        if(currentPressTime <= 0){ //per evitare di premere in continuazione, ogni volta che si preme o next o prev, per ripremerli nuovamente è necessario aspettare pressRate secondi
            Next();
            Prev();
        }else{
            currentPressTime -= Time.deltaTime;
        }

        if(avatarSelector.IsRaySelected){
            Debug.Log("Selected avatar : " + avatarVisualizer.GetChild(avatar_selector).gameObject.name);
            this.gameObject.SetActive(false);
            //Caricamento nuova scene...
        }
    }

    private void Next(){
        if(nextBtn.IsRaySelected){ //se premo il bottone
            avatar_selector = (avatar_selector + 1) % numberOfAvatars; //aumento avatar_selector sulla base del numero di avatar disponibili (child di avatarVisualizer)
            currentPressTime = pressRate; //resetto currentPressTime
        }
    }

    private void Prev(){
        if(prevBtn.IsRaySelected){ //se premo il bottone
            if(avatar_selector > 0){
                avatar_selector -= 1;
            }else{
                avatar_selector = numberOfAvatars - 1;
            }
            currentPressTime = pressRate;
        }
    }

    private void UpdateAvatarVisualizer(){
        avatarVisualizer.GetChild(avatar_selector).gameObject.SetActive(true); //attivo l'avatar attuale

        //disattivo gli altri gameObject
        for(int i = 0; i < numberOfAvatars; i++){
            if(i != avatar_selector)
                avatarVisualizer.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void AvatarRotationAnim(){
        avatarVisualizer.Rotate(0f, rotationSpeed, 0f); //rotazione di rotationSpeed attorno ad asse y
    }
}
