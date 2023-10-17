using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.UX;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class ButtonTest : MonoBehaviour
{
    private PressableButton pb;
    private Vector3 startScale;
    public MeshRenderer texture;
    private Color startTextureColor;
    // Start is called before the first frame update

    void Start()
    {
        pb = this.GetComponent<PressableButton>();
        startScale = this.transform.localScale;
        startTextureColor = texture.materials[0].color;

        pb.hoverEntered.AddListener(OnHoverEntered);
        pb.OnClicked.AddListener(()=>{
            Debug.Log("Clicked");
        });
    }

    private void OnHoverEntered(HoverEnterEventArgs e){
        if(pb.IsPokeHovered){
            Debug.Log("Touch");
        }
    }

    // Update is called once per frame
    void Update()
    {
        OnHoverRay();
        OnClickRay();
    }

    private void OnHoverRay(){
        if(pb.IsRayHovered){
            this.transform.localScale = new Vector3(startScale.x + 0.1f, startScale.y + 0.1f, startScale.z);
        }else{
            this.transform.localScale = startScale;
        }
    }

    private void OnClickRay(){
        if(pb.IsRaySelected){
            texture.materials[0].color = Color.magenta;
        }else{
            texture.materials[0].color = startTextureColor;
        }
    }
}
