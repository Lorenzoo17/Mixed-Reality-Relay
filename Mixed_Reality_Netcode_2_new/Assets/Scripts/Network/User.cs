using Unity.Netcode;
using UnityEngine;

namespace Microsoft.MixedReality.Toolkit.MultiUser
{
    public class User : NetworkBehaviour
    {
        public NetworkVariable<Vector3> HeadPosition = new NetworkVariable<Vector3>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public NetworkVariable<Quaternion> HeadRotation = new NetworkVariable<Quaternion>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        private Camera userCamera;

        public Transform Avatar; // Riferimento all'avatar
        public Transform Menu; //Riferimento al menu

        void Awake()
        {
            userCamera = Camera.main;
        }

        void Update()
        {
            if (!IsSpawned){
                return;
            }

            if (IsOwner){ //Se sono l'owner dell'oggetto
                Avatar.gameObject.SetActive(false); //l'owner non vede il proprio avatar

                Menu.gameObject.SetActive(true); //Il menu viene visto solo localmente

                transform.SetPositionAndRotation(userCamera.transform.position, userCamera.transform.rotation); //si assegna la posizione e rotazione del player pari a quella della camera
                HeadPosition.Value = userCamera.transform.position; //per sincronizzare sulla rete si assegna a tale variabile la posizione ( e chi non è l'owner [le altre entità della connessione] vedrà quindi tale valore )
                HeadRotation.Value = userCamera.transform.rotation;
                HeadPosition.SetDirty(true);
                HeadRotation.SetDirty(true);             
            }
            else {
                transform.SetPositionAndRotation(HeadPosition.Value, HeadRotation.Value);
                Menu.gameObject.SetActive(false);
            }
        }
    }
}