using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkStartUp : MonoBehaviour
{
    [SerializeField] private GameObject startUpMenu; //Menu utilizzato per contenere i bottoni che implementano i metodi StartAsHost() e StartAsClient()
    [SerializeField] private GameObject clientJoinMenu;
    [SerializeField] private TextMeshProUGUI joinCodeText;
    private void Update() {
        if(Input.GetKeyDown(KeyCode.I)){
            //NetworkManager.Singleton.StartHost();
        }else if(Input.GetKeyDown(KeyCode.L)){
            //NetworkManager.Singleton.StartClient();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            //ShutdownNetwork();
        }
    }

    //On clicked e poke entered gestiti direttamente dall'inspector Pressable Button dei bottoni
    public void StartAsHost() {
        //NetworkManager.Singleton.StartHost(); //Con Unity Transport
        FindObjectOfType<TestRelay>().CreateRelay(); //Con Relay Unity Transport
        startUpMenu.SetActive(false);
    }

    public void OpenClientConnessionWindow() {
        startUpMenu.SetActive(false);
        clientJoinMenu.SetActive(true);
    }

    public void StartAsClient() {
        //NetworkManager.Singleton.StartClient(); //Con Unity Transport
        FindObjectOfType<TestRelay>().JoinRelay(joinCodeText.text); //Con Relay Unity Transport
        clientJoinMenu.SetActive(false);
        //startUpMenu.SetActive(false); //Con Unity Transport
    }

    public void ShutdownNetwork() {
        NetworkManager.Singleton.Shutdown();
        startUpMenu.SetActive(true);
    }
}
