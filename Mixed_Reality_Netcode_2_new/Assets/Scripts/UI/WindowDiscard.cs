using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using UnityEngine.XR.Interaction.Toolkit;

public class WindowDiscard : MonoBehaviour {
    [SerializeField] private float discardDistance;
    private Transform menuDiscard;
    [SerializeField] private MeshRenderer textureMaterial;
    [SerializeField] private Color32 discardColor;
    [SerializeField] private float discardScalingInterpolationValue;
    [SerializeField] private Color32 grabColor;
    [SerializeField] private float dragScaleMultiplier;
    private Transform dragVisual;
    private Color32 dragVisualStartColor;
    private Vector3 dragVisualStartScale;
    private Color32 startColor;
    private Vector3 startScale;

    private bool windowGrabbed;
    private void Start() {
        menuDiscard = GameObject.FindGameObjectWithTag("MenuToggle").transform;
        startColor = textureMaterial.material.color;
        startScale = transform.localScale;

        dragVisual = transform.Find("DragVisual");
        dragVisualStartColor = dragVisual.GetComponent<MeshRenderer>().material.color;
        dragVisualStartScale = dragVisual.localScale;

        this.GetComponent<ObjectManipulator>().selectEntered.AddListener((SelectEnterEventArgs e) => {
            windowGrabbed = false;
        });
        this.GetComponent<ObjectManipulator>().selectExited.AddListener((SelectExitEventArgs e) => {
            windowGrabbed = false;
        });
    }

    private void Update() {
        if (Vector3.Distance(menuDiscard.position, transform.position) < discardDistance) {
            if (this.GetComponent<ObjectManipulator>().IsGrabSelected || this.GetComponent<ObjectManipulator>().IsRaySelected) {
                textureMaterial.material.color = discardColor;
                DiscardScaleBehaviour(startScale / 2);
            }
            else {
                this.gameObject.SetActive(false);
                if (this.GetComponent<WindowMove>() != null) {
                    this.GetComponent<WindowMove>().fixedWindow = false; //Quando chiudo la finestra imposto a false fixedWindow
                    this.GetComponent<WindowMove>().pinButton.GetComponent<ButtonPressBehaviour>().SetColorByCondition(true, this.GetComponent<WindowMove>().pinButton.GetComponent<ButtonPressBehaviour>().positiveColor, this.GetComponent<WindowMove>().pinButton.GetComponent<ButtonPressBehaviour>().negativeColor);
                }
                //DisableMenuToggle();
            }
        }
        else {
            if (transform.localScale != startScale)
                DiscardScaleBehaviour(startScale);

            if (textureMaterial.material.color != startColor) {
                textureMaterial.material.color = startColor;
            }

            if (this.GetComponent<ObjectManipulator>().IsGrabSelected || this.GetComponent<ObjectManipulator>().IsRaySelected) {
                dragVisual.GetComponent<MeshRenderer>().material.color = grabColor;
                dragVisual.transform.localScale = Vector3.Lerp(dragVisual.localScale, dragVisualStartScale * dragScaleMultiplier, discardScalingInterpolationValue);
            }
            else {
                dragVisual.GetComponent<MeshRenderer>().material.color = dragVisualStartColor;
                dragVisual.transform.localScale = Vector3.Lerp(dragVisual.localScale, dragVisualStartScale, discardScalingInterpolationValue);
            }
        }

        if (!windowGrabbed) {
            foreach (Transform menu in transform.parent) {
                if (menu.GetComponent<WindowDiscard>() != null && (menu.GetComponent<ObjectManipulator>().IsGrabSelected || menu.GetComponent<ObjectManipulator>().IsRaySelected)) {
                    windowGrabbed = true;
                }
            }

            menuDiscard.GetComponent<MenuToggle>().ChangeIcon(windowGrabbed);
        }
    }

    private void DiscardScaleBehaviour(Vector3 desideredScale) {
        transform.localScale = Vector3.Lerp(transform.localScale, desideredScale, discardScalingInterpolationValue);
    }

    private void DisableMenuToggle() {
        bool allClosed = true;
        foreach (Transform menu in transform.parent) {
            if (menu.GetComponent<WindowDiscard>() != null && menu != transform && menu.gameObject.activeSelf) {
                allClosed = false;
            }
        }

        menuDiscard.GetComponent<MenuToggle>().ChangeIcon(!allClosed);
    }
}