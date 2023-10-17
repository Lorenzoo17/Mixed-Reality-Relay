using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit.SpatialManipulation; //per accedere a componenti quali ObjectManipulator via script

public class EventsTest : MonoBehaviour
{
    public Color changedMaterialColor;
    private Color startMaterialColor;
    private ObjectManipulator om;
    
    public float colorInterpolator;
    public Transform target;
    public float minDistance;
    // Start is called before the first frame update
    void Start()
    {
        startMaterialColor = this.GetComponent<MeshRenderer>().materials[0].color; //assegno il material che ha inizialmente
        om = this.GetComponent<ObjectManipulator>(); //mi riferisco al componente ObjectManipulator assegnato all'oggetto
    }

    // Update is called once per frame
    void Update()
    {
        if(om.IsRaySelected){ //se ho selezionato l'oggetto (grabbato)
            this.GetComponent<MeshRenderer>().materials[0].color = Color.Lerp(this.GetComponent<MeshRenderer>().materials[0].color, changedMaterialColor, colorInterpolator);
        }else if (this.GetComponent<Rigidbody>().velocity.y == 0){
            if(Vector3.Distance(this.transform.position, target.position) < minDistance)
                this.GetComponent<MeshRenderer>().materials[0].color = startMaterialColor;
            else{
                this.GetComponent<MeshRenderer>().materials[0].color = Color.green;
            }
        }
    }
}
