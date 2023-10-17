using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using Dummiesman;

public class ObjImportManager : MonoBehaviour
{
    [HideInInspector] public string basePath = "C:/Users/utente/Documents/Progetti Unity/Files/"; //potrà ad esempio essere inserito da utente
    private string loadingPath;
    private OBJLoader objModel;

    public List<MeshFilter> meshes = new List<MeshFilter>();
    private void Start() {
        objModel = new OBJLoader();
    }

    private void Update() {

        if(Input.GetKeyDown(KeyCode.I)){
            try{
                string[] fileNames = GetFiles(basePath);

                for(int i = 0; i < fileNames.Length; i++){
                    GameObject model = objModel.Load(fileNames[i]);
                }
            }catch{
                Debug.Log("Non è ancora stato caricato il file");
            }
        }
    }

    private string[] GetFiles(string path){ //prendo i vari file presenti nella directory
        if(Directory.Exists(path)){
            return Directory.GetFiles(path);
        }
        return null;
    }
}
