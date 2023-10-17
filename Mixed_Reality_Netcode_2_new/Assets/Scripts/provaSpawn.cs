using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class provaSpawn : NetworkBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        if(!IsOwner) return;
        provaServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void provaServerRpc() {
        GetComponent<NetworkObject>().Spawn();
        Debug.Log("Spawn");
    }
}
