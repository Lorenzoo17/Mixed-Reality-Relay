using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TMPro;
using UnityEngine.UI;

using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.SpatialManipulation;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.InputSystem;
using Microsoft.MixedReality.Toolkit.UX;

public class MeshDistances : MonoBehaviour {

    public Mesh modelMesh; //mesh dell'oggetto di cui si vuole effettuare la misura tra due punti
    private Vector3[] vertices; //array dei vertici del mesh

    private Transform index;

    private Vector3 point1; //coordinate del primo punto
    private Vector3 point2; //coordinate del secondo punto

    private bool collected1; //booleano che indica se il primo punto è stato acquisito o meno
    private bool collected2; //booleano che indica se il secondo punto è stato acquisito

    public float timeToWait;
    public float timeBtwPointCollection; //tempo tra l'acquisizione di un punto e il successivo
    private bool canCollect; //in base al tempo passato abilita o meno l'acquisizione del punto successivo

    private Transform measureInfo; //Finestra nel client locale nel quale verranno poste le informazioni sull'acquisizione
    private GameObject point1Verified, point2Verified; //Fanno riferimento ai gameObject della finestra measureInfo che indicano se il punto i-esimo è stato o meno acquisito
    private TextMeshPro distanceText; //Testo della finestra nel quale va posto il valore della distanza

    [SerializeField] private GameObject pointIndicatorPrefab; //prefab per segnare la posizione del punto acquisito

    private GameObject point1Object, point2Object; //punti da istanziare

    private ObjectManipulator om;
    public bool canMeasure; //booleano attivato dal bottone Measure del menu

    // Start is called before the first frame update
    void Start() {
        canCollect = true;
        timeBtwPointCollection = timeToWait;
        om = GetComponent<ObjectManipulator>();
    }

    // Update is called once per frame
    void Update() {
        CalculateDistances();
    }

    private void CalculateDistances() {
        if (modelMesh != null) { //Se si trova il mesh nell'oggetto
            vertices = modelMesh.vertices; //si assegnano i vertici, vanno assegnati sempre in quanto può cambiare

            //Inizializzo le variabili per acquisire le informazioni della finestra di misurazione
            if(GameObject.FindGameObjectWithTag("MeasureInfo") != null) {
                if(measureInfo == null) {
                    measureInfo = GameObject.FindGameObjectWithTag("MeasureInfo").transform;
                    point1Verified = measureInfo.Find("Content").Find("Point_1").Find("Verified").gameObject;
                    point2Verified = measureInfo.Find("Content").Find("Point_2").Find("Verified").gameObject;
                    distanceText = measureInfo.Find("Content").Find("Distance").Find("IconAndText").GetComponent<TextMeshPro>();
                }
            }
            else {
                measureInfo = null;
            }

            if (GameObject.FindGameObjectWithTag("Index") != null && om.IsGrabHovered && canMeasure) { //Se è presente nella scene l'oggetto index (posto come child della mano destra del MRTK, che serve per selezionare il vertice), se sono abbastanza vicino al mesh e se canMeasure è true (definita in MenuOptions)
                index = GameObject.FindGameObjectWithTag("Index").transform; //Ottengo l'indice

                if (vertices.Length > 0) { //Se i vertici sono stati assegnati correttamente
                    for (int i = 0; i < vertices.Length; i++) { //Ciclo su tutti i vertici
                        if (vertices[i] != null) {
                            Vector3 indexPos = index.position; //ottengo la posizione dell'indice globale
                            Vector3 vertexWorldPos = gameObject.transform.TransformPoint(vertices[i]);//Ottengo la posizione del singolo vertice e la converto nello spazio globale

                            if (Vector3.Distance(indexPos, vertexWorldPos) < 0.1f) { //se sono vicino al vertice che sto analizzando
                                if (!collected1 && canCollect) { //se non ho ancora selezionato il primo vertice
                                    point1 = vertices[i]; //imposto le sue coordinate
                                    collected1 = true;
                                    canCollect = false;
                                    point1Object = Instantiate(pointIndicatorPrefab, vertexWorldPos, Quaternion.identity); //istanzio il punto 
                                }
                                else if (!collected2 && canCollect && vertices[i] != point1) { //se ho già selezionato il primo ma non ancora il secondo
                                    point2 = vertices[i];
                                    collected2 = true;
                                    canCollect = false;
                                    point2Object = Instantiate(pointIndicatorPrefab, vertexWorldPos, Quaternion.identity);
                                }
                                else if (collected1 && collected2 && canCollect) { //se li ho già selezionati entrambi, resetto
                                    collected1 = false;
                                    collected2 = false;
                                    point1 = Vector3.zero;
                                    point2 = Vector3.zero;
                                    Destroy(point1Object);
                                    Destroy(point2Object);
                                }
                                break;
                            }

                        }
                    }

                    WaitForSecondPoint(); //Dopo l'acquisizione del primo punto aspetto a collezionare il secondo
                    AssignText();
                }
            }

        }
        else {
            modelMesh = this.GetComponent<MeshFilter>().mesh;
        }
    }

    private void AssignText() {
        point1Verified.SetActive(collected1);
        point2Verified.SetActive(collected2);
        
        if (point1 != Vector3.zero && point2 != Vector3.zero) {
            distanceText.text = "Distance : " + ((point1 - point2).magnitude / 1000f).ToString("F") + " m";
        }
    }

    private void WaitForSecondPoint() { //Attesa tra l'acquisizione di un punto e l'altro
        if (!canCollect) {
            if (timeBtwPointCollection <= 0) {
                timeBtwPointCollection = timeToWait;
                canCollect = true;
            }
            else {
                timeBtwPointCollection -= Time.deltaTime;
            }
        }
    }
}
