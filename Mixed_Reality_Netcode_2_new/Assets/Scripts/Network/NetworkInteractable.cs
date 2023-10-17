using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.XR.Interaction.Toolkit;

namespace Microsoft.MixedReality.Toolkit.MultiUser
{
    public class NetworkInteractable : NetworkBehaviour
    {
        public XRBaseInteractable interactable; //variabile che fa riferimento al componente Interactable dell'object manipulator

        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>( //per sincronizzare posizione dell'oggetto nella rete
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public NetworkVariable<Quaternion> Rotation = new NetworkVariable<Quaternion>( //per sincronizzare rotazione dell'oggetto nella rete
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );
        public override void OnNetworkSpawn() //allo spawn
        {
            base.OnNetworkSpawn();
            if (interactable == null){
                interactable = GetComponentInChildren<XRBaseInteractable>(); //si assegna il componente alla variabile
            }
            
            if (interactable != null){
                interactable.firstSelectEntered.AddListener(OnFirstSelectEntered); //si applica all'evento di selezione dell'oggetto (del MRTK) il metodo definito
                interactable.lastSelectExited.AddListener(OnLastSelectExited); //si applica all'evento di fine selezione dell'oggetto (del MRTK) il metodo definito
            }
        }

        public override void OnNetworkDespawn() //Al despawn si rimuovono i metodi dai listener
        {
            base.OnNetworkDespawn();
            if (interactable != null)
            {
                interactable.firstSelectEntered.RemoveListener(OnFirstSelectEntered);
                interactable.lastSelectExited.RemoveListener(OnLastSelectExited);
            }
        }
        public void Update(){
            if (!IsSpawned)
            {
                return;
            }

            SyncPose();
        }

        private void SyncPose()
        {
            if (IsOwner){ //meccanismo come quello dello user, se sono owner assegno alle variabili di rete la posizione
                Position.Value = transform.localPosition;
                Rotation.Value = transform.localRotation;
                Position.SetDirty(true);
                Rotation.SetDirty(true);
            }
            else //Se non sono l'owner usufruisco delle variabili di rete per capire la posizione dell'oggetto
            {
                transform.localPosition = Position.Value;
                transform.localRotation = Rotation.Value;
            }
        }

        private void OnFirstSelectEntered(SelectEnterEventArgs args)
        {
            if (!IsOwner) //se non sono l'owner
            {
                ChangeOwnershipServerRpc(true); //cambio ownership per rendere il player il possessore dell'oggetto, quando entro in contatto con esso
            }
        }

        private void OnLastSelectExited(SelectExitEventArgs args)
        {
            if (IsOwner)
            {
                ChangeOwnershipServerRpc(false); //Lascio l'ownership quando lascio l'oggetto
            }
        }

        [ServerRpc(RequireOwnership = false)] 
        public void ChangeOwnershipServerRpc(bool wantOwnership, ServerRpcParams serverRpcParams = default) //per far si che un solo client alla volta possa cambiare le proprietà dell'oggetto è necessario assegnarli l'ownership. Solo chi ha l'ownership di un oggetto può cambiarne le proprietà
        {
            Debug.Log($"{serverRpcParams.Receive.SenderClientId} {(wantOwnership ? "vuole" : "non vuole")} avere {gameObject.name}!");
            if (wantOwnership)
            {
                NetworkObject.ChangeOwnership(serverRpcParams.Receive.SenderClientId); //setto l'ownership al client che "ne ha fatto richiesta"
            }
            else
            {
                NetworkObject.RemoveOwnership(); //rimuovo l'ownership
            }
        }
    }
}
