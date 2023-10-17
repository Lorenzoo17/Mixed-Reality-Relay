using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using System;

public class Inventory : NetworkBehaviour
{
    public List<Item> prefabToSpawn; //Lista dei prefab che possono essere spawnati (dovranno essere posti anche nella lista dei network prefabs)

    public List<Item> inventory; //Inventario (voglio tenerli separati, per eventuale aggiunta di mesh personalizzati)

    //UI elements
    [SerializeField] private GameObject inventorySlot; //slot che fa riferimento ai singoli oggetti che possono essere istanziati
    [SerializeField] private Transform inventoryContent; //GameObject contenente i vari slot

    //UI slots : dati su posizionamento
    [SerializeField] private Vector3 startPosition; //posizione iniziale nel quale porre il primo slot (relativa a inventoryContent)
    [SerializeField] private Vector3 slotDistance; //Distanza tra slot che permette di posizionare dinamicamente gli slot successivi al primo
    [SerializeField] private int slotInRow; //Numero massimo di slot che si vogliono porre per riga

    //Visto che le chiamate RPC possono essere effettuate solo da oggetti con NetworkObject, e dato che i child annidati non possono avere tale componente
    //Vado a istanziare i vari oggetti da questo script. Questi parametri vengono aggiornati dai singoli invetorySlot per questo scopo
    public bool wantToSpawn; //Se si vuole spawnare un oggetto
    public int itemToSpawnId; //identificativo dell'oggetto da spawnare

    private void Start() {
        InitializeInventory();
    }

    private void InitializeInventory() {
        inventory = new List<Item>();

        //Si caricano i prefab iniziali nell'inventario
        foreach(Item prefab in prefabToSpawn) {
            inventory.Add(prefab);
        }

        RefreshInventoryUI(); //Si generano gli slot sulla base dei prefab contenuti nell'inventario
    }

    private void Update() {
        if (!IsOwner) return;

        if(wantToSpawn) {
            SpawnItemServerRpc(itemToSpawnId); //Chiamata RPC per spawn dell'oggetto desiderato
            wantToSpawn = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnItemServerRpc(int index) {
        GameObject spawned = Instantiate(inventory[index].prefabToSpawn, new Vector3(transform.position.x, transform.position.y, transform.position.z + 1f), Quaternion.identity);
        spawned.GetComponent<NetworkObject>().Spawn();
        Debug.Log("Spawn new item");
    }

    private void RefreshInventoryUI() {
        foreach(Transform slot in inventoryContent) {
            Destroy(slot.gameObject); //Elimino gli slot precedentemente istanziati al refresh, per mantenere coerenza in caso in cui si volesse aggiungere elementi runtime
        }

        int inRowCount = 0;
        int columnCount = 0;

        //Genero i singoli slot all'interno di inventoryContent
        for(int i = 0; i < inventory.Count; i++) {
            GameObject slot = Instantiate(inventorySlot, inventoryContent.position, Quaternion.identity); //Istanzio lo slot
            slot.transform.SetParent(inventoryContent); //Imposto lo slot come child di content nell'inventario

            slot.transform.localScale = Vector3.one; //Imposto lo scale ad 1
            slot.transform.localPosition = startPosition + new Vector3(slotDistance.x * inRowCount, -slotDistance.y * columnCount, 0f); //Imposto la sua posizione, in base al numero di slot per riga e la distanza su y in base al numero di colonne

            slot.GetComponent<InventorySlot>().SetSlot(i, inventory[i].prefabToSpawn, inventory[i].icon); //Imposto l'identificativo dello slot e l'oggetto che esso deve spawnare

            //tengo traccia di oggetti nella riga attuale e colonna corrente      
            if (inRowCount < slotInRow-1)
                inRowCount++;
            else {
                inRowCount = 0;
                columnCount++;
            }
        }

    }

    public void Add(Item itemToAdd) { //Metodo pubblico per implementare eventuale aggiunta di oggetti da parte dell'utente
        inventory.Add(itemToAdd);
        RefreshInventoryUI(); //Effettuo il refresh all'aggiunta in modo da poter eventualmente aggiornare l'inventario runtime
    }
}
