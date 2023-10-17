using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEditor;
using UnityEngine;

public class TestRelay : MonoBehaviour
{
    //[SerializeField] private TextMeshProUGUI joinCodeText;

    private async void Start() {
        await UnityServices.InitializeAsync(); //Si inizializzano i servizi di Unity (per potersi autenticare e accedere a funzioni come Relay server appunto)

        AuthenticationService.Instance.SignedIn += () => {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId); //All'accesso si legge un messaggio con l'id del player
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync(); //Si entra nel contesto relay come anonimo
    }


    public async void CreateRelay() {
        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3); //Si crea una sessione gestita da un Relay Server con al massimo 4 persone connesse (compreso host [3 + 1]), la regione viene scelta automaticamente 

            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); //Si prende il joinCode per accedere alla sessione del Relay creato

            Debug.Log("Join code to share : " + joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);
            NetworkManager.Singleton.StartHost();
        }catch(RelayServiceException e) {
            Debug.Log("Relay : " + e);
        }
    }

    public async void JoinRelay(string joinCodeString) {
        try {
            Debug.Log("Joining relay with : " + joinCodeString);
            string joinCode = joinCodeString.Substring(0, 6);
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );
            NetworkManager.Singleton.StartClient();
        }
        catch(RelayServiceException e) {
            Debug.Log("Relay join : " + e);
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.U)) {
            //CreateRelay();
        }

        if (Input.GetKeyDown(KeyCode.M)) {
            //JoinRelay();
        }
    }
}
