using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using Unity.Netcode;

public class BtnNetTest : NetworkBehaviour
{
    private PressableButton pb;
    private Vector3 startScale;

    [SerializeField] private GameObject itemToSpawn;

    public NetworkVariable<Vector3> scaleNet = new NetworkVariable<Vector3>(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private void Start() {
        startScale = transform.localScale;
        scaleNet.Value = startScale;
        pb = this.GetComponent<PressableButton>();

        pb.IsRayHovered.OnEntered.AddListener((float call)=>{
            //transform.localScale = new Vector3(startScale.x * 1.5f, startScale.y * 1.5f, startScale.z);
            SetScaleServerRPC(new Vector3(startScale.x * 1.5f, startScale.y * 1.5f, startScale.z));
        });

        pb.IsRayHovered.OnExited.AddListener((float call) => {
            //transform.localScale = startScale;
            SetScaleServerRPC(startScale);
        });

        pb.OnClicked.AddListener(()=>{
            SpawnObjectServerRPC();
        });
    }

    private void Update() {
        if (IsOwner){
            scaleNet.Value = transform.localScale;
            scaleNet.SetDirty(true);
        }
        else{
            transform.localScale = scaleNet.Value;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetScaleServerRPC(Vector3 scale){
        transform.localScale = scale;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SpawnObjectServerRPC(ServerRpcParams serverRpcParams = default){
        if(IsOwner){
            GameObject newItem = Instantiate(itemToSpawn, new Vector3(transform.position.x, transform.position.y, transform.position.z + 1f), Quaternion.identity);
            newItem.GetComponent<NetworkObject>().Spawn();
            Debug.Log("Spawn by button");
        }
    }
}
