using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Netcode;
using Microsoft.MixedReality.Toolkit.UX;

public class DestroySpawnedItem : NetworkBehaviour
{
    private void Start() {
        //on clicked gestito nell'inspector > On Clicked
        //Poke enter gestito nell'inspector > MRTK Events > Is Poke Hovered
    }

    //Solo il server può despawnare oggetti nella rete, quindi per eseguire il metodo lato server devo effettuare una chiamata ServerRpc
    [ServerRpc(RequireOwnership = false)]
    public void DespawnAllObjectsServerRpc() {
        foreach(GameObject item in GameObject.FindGameObjectsWithTag("Item")) { //Cerco tutti gli oggetti che hanno tag item nella scene
            item.GetComponent<NetworkObject>().Despawn(); //Despawno
            Destroy(item); //Distruggo l'item i-esimo
            Debug.Log("Despawned item : " + item.name);
        }
    }
}
