using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ProvaRPC : NetworkBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    private int[] prova;

    private void Start() {
        prova = new int[300000];
        for(int i = 0; i < 300000; i++) {
            prova[i] = i;
        }
    }
    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.M)) {
            //GameObject item = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            //item.GetComponent<MeshFilter>().mesh = CreateMesh();
            //SpawnModelServerRpc();
            GameObject item = Instantiate(objectToSpawn, transform.position, Quaternion.identity);
            item.GetComponent<MeshFilter>().mesh = CreateMesh();
            item.GetComponent<NetworkObject>().Spawn();

            MeshSerialization itemMesh;
            itemMesh.vertices = item.GetComponent<MeshFilter>().mesh.vertices;
            itemMesh.uvs = item.GetComponent<MeshFilter>().mesh.uv;
            itemMesh.triangles = item.GetComponent<MeshFilter>().mesh.triangles;

            for (int i = 0; i < 100; i++) {
                
                itemMesh.index = (i + 1) * 3000;
                int[] toSend = new int[3000];
                for(int j = 0; j < 3000; j++) {
                    toSend[j] = (i * 3000) + j;
                }
                itemMesh.prova = toSend;
                itemMesh.n_calls = i + 1;
                AssignMeshClientRpc(itemMesh);
            }
        }
    }

    [ClientRpc] //Viene invocata lato server ed eseguita lato client
    private void AssignMeshClientRpc(MeshSerialization meshSerialize) {

        Debug.Log(meshSerialize.index);
        Debug.Log(meshSerialize.n_calls);

        if(meshSerialize.n_calls == 100) {
            Debug.Log("Finished");
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh = new Mesh();
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.vertices = meshSerialize.vertices;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.uv = meshSerialize.uvs;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.triangles = meshSerialize.triangles;
        }
    }

    struct MeshSerialization : INetworkSerializable {
        public Vector3[] vertices;
        public Vector2[] uvs;
        public int[] triangles;

        public int[] prova;
        public int index;
        public int n_calls;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref vertices);
            serializer.SerializeValue(ref uvs);
            serializer.SerializeValue(ref triangles);
            serializer.SerializeValue(ref index);
            serializer.SerializeValue(ref n_calls);
            serializer.SerializeValue(ref prova);
        }
    }

    private Mesh CreateMesh() {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[3];
        Vector2[] uv = new Vector2[3];
        int[] triangles = new int[3];

        vertices[0] = new Vector3(0, 0);
        vertices[1] = new Vector3(0, 100);
        vertices[2] = new Vector3(100, 100);

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        return mesh;
    }
}
