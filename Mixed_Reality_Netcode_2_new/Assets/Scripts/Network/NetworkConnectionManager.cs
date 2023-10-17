using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkConnectionManager : MonoBehaviour
{
    //[SerializeField] private GameObject startUpMenu; //Menu utilizzato per contenere i bottoni che implementano i metodi StartAsHost() e StartAsClient()
    [SerializeField] private string gameScene;
    [SerializeField] private string connectionScene;
    private void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            NetworkManager.Singleton.StartHost();
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            NetworkManager.Singleton.StartClient();
        }

        if (Input.GetKeyDown(KeyCode.C)) {
            ShutdownNetwork();
        }
    }

    //On clicked e poke entered gestiti direttamente dall'inspector Pressable Button dei bottoni
    public void StartAsHost() {
        NetworkManager.Singleton.StartHost();
        SceneManager.LoadScene(gameScene);
    }

    public void StartAsClient() {
        NetworkManager.Singleton.StartClient();
        SceneManager.LoadScene(gameScene);
    }

    public void ShutdownNetwork() {
        NetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(connectionScene);
    }
}
