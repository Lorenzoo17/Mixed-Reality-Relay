using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;
using Microsoft.MixedReality.Toolkit.UX;

public class WindowMove : MonoBehaviour
{
    public bool fixedWindow;
    public PressableButton pinButton;

    public Vector3 lastPosition; //modificare questo valore anche quando si riattiva il gameobject
    public Vector3 startPosition;
    private void Start() {
        startPosition = transform.position;
        lastPosition = transform.position;

        GetComponent<ObjectManipulator>().selectExited.AddListener((SelectExitEventArgs e) => {
            lastPosition = transform.position;
        });

        pinButton.OnClicked.AddListener(() => {
            fixedWindow = !fixedWindow;
            lastPosition = transform.position;
            pinButton.GetComponent<ButtonPressBehaviour>().SetColorByCondition(!fixedWindow, pinButton.GetComponent<ButtonPressBehaviour>().positiveColor, pinButton.GetComponent<ButtonPressBehaviour>().negativeColor);
        });
        pinButton.hoverEntered.AddListener((HoverEnterEventArgs e) => {
            if (pinButton.IsPokeHovered) {
                fixedWindow = !fixedWindow;
                lastPosition = transform.position;
                pinButton.GetComponent<ButtonPressBehaviour>().SetColorByCondition(!fixedWindow, pinButton.GetComponent<ButtonPressBehaviour>().positiveColor, pinButton.GetComponent<ButtonPressBehaviour>().negativeColor);
            }
        });
    }
    // Update is called once per frame
    void Update()
    {
        if (!(GetComponent<ObjectManipulator>().IsGrabSelected || GetComponent<ObjectManipulator>().IsRaySelected) && fixedWindow)
            transform.position = lastPosition;

        //Debug.Log(Camera.main.WorldToScreenPoint(transform.position));
    }
}
