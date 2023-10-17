using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Globalization;

public class ObjReader : MonoBehaviour
{
    //public string fileName; // Nome del file OBJ da leggere
    [HideInInspector] public string basePath = "C:/Users/utente/Documents/Progetti Unity/Files/"; //potrà ad esempio essere inserito da utente
    public Material meshMaterial; // Materiale da assegnare al mesh

    private void Update() {
        if(Input.GetKeyDown(KeyCode.I)){
            try{
                foreach(string filePath in GetFiles(basePath)){
                //Debug.Log(filePath);
                SpawnModel(filePath);
            }
            }catch{
                Debug.Log("File non presente");
            }
        }
    }
    
    private string[] GetFiles(string path) //dato il path ritorno i vari file nella directory specificata dal path
    { //prendo i vari file presenti nella directory
        if (Directory.Exists(path))
        {
            return Directory.GetFiles(path);
        }
        return null;
    }

    private void SpawnModel(string path)
    {
        // Leggi il file OBJ
        string filePath = path;
        List<Vector3> vertices = new List<Vector3>(); //lista dei vertici del mesh
        List<int> triangles = new List<int>(); //per le faccie

        StreamReader sr = new StreamReader(filePath);
        string line;
        char[] delimiters = new char[] { ' '}; //delimiter che salto per lettura

        while ((line = sr.ReadLine()) != null) //fino alla fine del file
        {
            string[] parts = line.Trim().Split(delimiters); //le differenti parti del testo, che erano separate tra loro dai delimiter

            if (parts.Length == 0)
                continue;

            if (parts[0] == "v") // Vertici (i file obj indicano i vertici iniziando con la lettera v), qui sotto converto il valore dei vertici da stringa a float e creo  il vettore che identifica le coordinate del vertice
            {
                float x = float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat); //parto da 2 visto che ha i doppi spazi questo formato
                float y = float.Parse(parts[3], CultureInfo.InvariantCulture.NumberFormat);
                float z = float.Parse(parts[4], CultureInfo.InvariantCulture.NumberFormat);
                vertices.Add(new Vector3(x, y, z));
                //Debug.Log(x + ", " + y + ", " + z);
            }
            else if (parts[0] == "f") // Facce
            {
                for (int i = 1; i < parts.Length; i++)
                {
                    int vertexIndex = int.Parse(parts[i].Split('/')[0]) - 1; // -1 perché far corrispondere con l'indice dell'array
                    //Debug.Log(vertexIndex);
                    triangles.Add(vertexIndex);
                }
            }
        }

        Mesh mesh = new Mesh(); //creo il mesh assegnando vertici e triangoli
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();

        GameObject newObject = new GameObject("NewObject"); //creo il gameobject, con la creazione c'è anche automaticamente lo spawn e lo istanzio in posizione casuale e assegno ad esso i vari componenti creati
        newObject.transform.position = new Vector3(Random.Range(-10f, 10f), Random.Range(-10f, 10f), Random.Range(-10f, 10f));
        newObject.AddComponent<MeshFilter>();
        newObject.AddComponent<MeshRenderer>();
        newObject.GetComponent<MeshFilter>().mesh = mesh;
        newObject.GetComponent<MeshRenderer>().material = meshMaterial;
    }
}
