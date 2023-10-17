using Microsoft.MixedReality.Toolkit.UX;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class DisconnectButton : MonoBehaviour
{
    private PressableButton pb;

    private void Start() {
        pb = GetComponent<PressableButton>();

        pb.OnClicked.AddListener(() => {
            Disconnect();
        });
        pb.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if(pb.IsPokeSelected)
                Disconnect();
        });
    }

    private void Disconnect() {
        if(FindObjectOfType<NetworkStartUp>() != null) {
            FindObjectOfType<NetworkStartUp>().ShutdownNetwork();
        }
    }
}
