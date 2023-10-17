using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class InventorySlot : MonoBehaviour{

    public int slotNumber; //identificativo dello slot assegnato in Inventory
    public GameObject prefabToSpawn; //prefab dell'oggetto che viene istanziato alla pressione del bottone
    //public Sprite icon;

    private PressableButton pb;

    private void Start() {
        pb = GetComponent<PressableButton>();

        pb.OnClicked.AddListener(SpawnItem); //Al click o al poke enter richiamo il metodo per lo spawn dell'oggetto assegnato allo slot
        pb.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if (pb.IsPokeHovered)
                SpawnItem();
        });
    }

    public void SpawnItem() {
        if(this.GetComponent<ButtonPressBehaviour>().CanBePressed()) {
            this.transform.root.GetComponent<Inventory>().itemToSpawnId = slotNumber;
            this.transform.root.GetComponent<Inventory>().wantToSpawn = true;
        }
    }

    public void SetSlot(int slotNumber, GameObject prefabToSpawn, Sprite prefabIcon) { //Metodo richiamato in Inventory per impostare lo slot
        this.slotNumber = slotNumber;
        this.prefabToSpawn = prefabToSpawn;
        transform.Find("Icon").GetComponent<SpriteRenderer>().sprite = prefabIcon;
    }
}
