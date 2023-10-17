using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Microsoft.Azure.Kinect.Sensor;


public class provaNV : NetworkBehaviour {
    /*
    [Serializable]
    public class DataToSyncNetworkVariable : NetworkVariableBase{
        public List<int> vectorList;
        public override void WriteField(FastBufferWriter writer) {
            writer.WriteValueSafe(vectorList.Count);
            foreach (int vector in vectorList) {
                writer.WriteValueSafe(vector);
            }
        }
        public override void ReadField(FastBufferReader reader) {
            var itemToUpdate = (int)0;
            reader.ReadValueSafe(out itemToUpdate);
            vectorList.Clear();

            for (int i = 0; i < itemToUpdate; i++) {
                var newData = new int();
                reader.ReadValueSafe(out newData);
                vectorList.Add(newData);
            }
        }
        public override void ReadDelta(FastBufferReader reader, bool keepDirtyDelta) {
            throw new NotImplementedException();
        }

        public override void WriteDelta(FastBufferWriter writer) {
            throw new NotImplementedException();
        }
    }

    public DataToSyncNetworkVariable dataToSync = new DataToSyncNetworkVariable();

    public NetworkVariable<DataToSync> data = new NetworkVariable<DataToSync>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct DataToSync : INetworkSerializable{
        public Vector3[] vectorList;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            int lenght = 0;
            if (!serializer.IsReader) {
                lenght = vectorList.Length;
            }

            serializer.SerializeValue(ref lenght);

            if (serializer.IsReader) {
                vectorList = new Vector3[lenght];
            }

            for(int i = 0; i < lenght; i++) {
                serializer.SerializeValue(ref vectorList[i]);
            }
        }
    }
        */

    private Device device;
    private DeviceConfiguration config;

    private Mesh mesh; //Mesh dell'oggetto al quale voglio applicare la nuvola di punti
    private Vector3[] vertices; //Vertici del mesh della nuvola di punti --> Essendo il mesh una nuvola di punti, un vertice corrisponde ad un punto di quest'ultima
    private int[] indices; //Indici del mesh per la nuvola di punti
    private Color32[] colors; //Colore da applicare ai singoli punti della nuvola di punti

    private Transformation transformation; //Definisco la trasformazione che verrà utilizzata per il passaggio da un sistema di coordinate all'altro
    private int size; //dimensione totale (width * height) necessario lavorare con size per array di vertici ecc

    [SerializeField] private float viewDistance = 1000f; //Distanza oltre la quale i dati della depth camera non vengono presi in considerazione per lo sviluppo della nuvola di punti (default ad 1 metro : 1000f)

    [SerializeField] private GameObject modelToSpawn; //Modello da spawnare al quale applicare il mesh

    public bool azureConnected = true; //Variabile che richiamo da MenuOptions per permettere o meno la pressione del bottone in base allo stato di connessione del kinect

    public NetworkList<DataToSync> dataList;

    private List<Vector3> verticesList = new List<Vector3>();
    private List<Color32> colorList = new List<Color32>();
    private int[] optIndices;

    private float currTime;
    private float transTime = 0.2f;

    private void Awake() {
        dataList = new NetworkList<DataToSync>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    }

    private void Start() {
        try {
            InitializeKinect(); //inizializzo kinect
            InitializeMesh(); //inizializzo mesh definendo numero di vertici, colori e tipo di indici
        }
        catch (AzureKinectOpenDeviceException e) {
            Debug.Log("Device non disponibile per la connessione");
            azureConnected = false;
        }
    }

    private void Update() {

        if (!IsOwner) { return; }

        if (Input.GetKey(KeyCode.T) && azureConnected) {
            if (currTime <= 0) {
                using (Capture capture = device.GetCapture()) {
                    GeneratePointCloudOpt(capture);

                    if (GameObject.FindGameObjectWithTag("PCModel") == null) {
                        GameObject item = Instantiate(modelToSpawn, transform.position, Quaternion.Euler(0f, 0f, 180f));
                        item.GetComponent<NetworkObject>().Spawn(); //Non effettuo chiamata RPC in quanto l'oggetto può essere spawnato solo dall'host, quindi non c'è bisogno
                    }

                    dataList.Clear();

                    for (int i = 0; i < verticesList.Count; i++) {
                        dataList.Add(new DataToSync {
                            //per opt mettere le liste, altrimenti i vettori
                            index = optIndices[i],
                            color = colorList[i],
                            vertex = verticesList[i]
                        });
                    }
                    currTime = transTime;
                }
            }
            else {
                currTime -= Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.V)) {
            using (Capture capture = device.GetCapture()) {
                GeneratePointCloudOpt(capture);
            }
        }

    }

    private void GeneratePointCloud(Capture capture) {
        Image colorImage = transformation.ColorImageToDepthCamera(capture); //per ottenere un immagine a colori dalla prospettiva della depth camera

        BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray(); //ottengo l'array dei pixel in formato BGRA dell'immagine precedentemente acquisita

        Image xyzImage = transformation.DepthImageToPointCloud(capture.Depth); //ottengo un immagine di punti tridimensionali unendo le informazioni dell'immagine a colori con la depth camera
        Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray(); //qua semplicemente converto l'immagine in array di punti nello spazio 3D

        for (int i = 0; i < size; i++) {
            //i vertici corrisponderanno ai punti 3D
            vertices[i].x = xyzArray[i].X;
            vertices[i].y = xyzArray[i].Y;
            vertices[i].z = xyzArray[i].Z;

            //Aggiungo anche l'informazione del colore i-esimo che corrisponde al vertice i-esimo dall'immagine a colori dalla prospettiva della depth camera
            colors[i].r = colorArray[i].R;
            colors[i].g = colorArray[i].G;
            colors[i].b = colorArray[i].B;
            colors[i].a = 255;

            if (xyzArray[i].Z > viewDistance) { //se il punto è oltre 1 metro (1000 mm) non lo mappo all'interno del mesh (assegno coordinata [0,0,0])
                vertices[i] = Vector3.zero;
            }
        }

        mesh.vertices = vertices; //ora assegno i vertici alla mesh
        mesh.colors32 = colors; //lo stesso per i colori
        mesh.RecalculateBounds(); //e ricalcolo il volume avendo cambiato i vertici 
    }

    private void GeneratePointCloudOpt(Capture capture) {
        Image colorImage = transformation.ColorImageToDepthCamera(capture); //per ottenere un immagine a colori dalla prospettiva della depth camera

        BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray(); //ottengo l'array dei pixel in formato BGRA dell'immagine precedentemente acquisita

        Image xyzImage = transformation.DepthImageToPointCloud(capture.Depth); //ottengo un immagine di punti tridimensionali unendo le informazioni dell'immagine a colori con la depth camera
        Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray(); //qua semplicemente converto l'immagine in array di punti nello spazio 3D
        verticesList.Clear();
        colorList.Clear();
        for (int i = 0; i < size; i++) {
            //i vertici corrisponderanno ai punti 3D
            vertices[i].x = xyzArray[i].X;
            vertices[i].y = xyzArray[i].Y;
            vertices[i].z = xyzArray[i].Z;

            //Aggiungo anche l'informazione del colore i-esimo che corrisponde al vertice i-esimo dall'immagine a colori dalla prospettiva della depth camera
            colors[i].r = colorArray[i].R;
            colors[i].g = colorArray[i].G;
            colors[i].b = colorArray[i].B;
            colors[i].a = 255;

            if (xyzArray[i].Z > viewDistance) { //se il punto è oltre 1 metro (1000 mm) non lo mappo all'interno del mesh (assegno coordinata [0,0,0])
                vertices[i] = Vector3.zero;
            }

            if (vertices[i] != Vector3.zero) {
                verticesList.Add(vertices[i]);
                colorList.Add(colors[i]);
            }
        }
        optIndices = new int[verticesList.Count];
        for (int i = 0; i < optIndices.Length; i++) {
            optIndices[i] = i;
        }
        Debug.Log("Vertici old : " + vertices.Length + "\nVertices new : " + verticesList.Count);
    }

    private void InitializeKinect() {
        device = Device.Open(0);

        config = new DeviceConfiguration();
        config.CameraFPS = FPS.FPS30;
        config.ColorFormat = ImageFormat.ColorBGRA32;
        config.ColorResolution = ColorResolution.R720p;
        config.DepthMode = DepthMode.NFOV_2x2Binned; //immagine depth 320x288
        config.SynchronizedImagesOnly = true; //per acquisire sia immagini a colori che depth

        device.StartCameras(config);

        transformation = device.GetCalibration().CreateTransformation(); //creo automaticamente una trasformazione sulla base della configurazione precedente
    }

    private void InitializeMesh() {
        int depthWidth = device.GetCalibration().DepthCameraCalibration.ResolutionWidth; // width della depth camera (640 con Unbinned)
        int depthHeight = device.GetCalibration().DepthCameraCalibration.ResolutionHeight; //height (576 con Unbinned)
        size = (depthWidth * depthHeight);

        mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //formato a 32 bit che supporta fino a 4 miliardi di vertici

        vertices = new Vector3[size]; //array vuoto di vertici (tutti a vector3.zero)
        colors = new Color32[size];
        indices = new int[size];

        //assegno agli indici valori da 0 a size
        for (int i = 0; i < size; i++) {
            indices[i] = i; //inizio a definire gli indici, che avendo scelto MeshTopoly.Points sono in ordine
        }

        //assegno ai vertici del mesh quelli assegnati della nuvola di punti
        mesh.vertices = vertices; //inizializzo i vertici della mesh
        mesh.colors32 = colors; //inizializzo i colori dei vertici della mesh
        mesh.SetIndices(indices, MeshTopology.Points, 0); //imposto gli indici del mesh, in questo caso come topologia uso Points, quindi i vertici non si collegano triangolarmente per formare facce, ma si collegano a 2 a 2

        //this.GetComponent<MeshFilter>().mesh = mesh;
    }

    private void OnDestroy() {
        dataList?.Dispose();
        if (azureConnected)
            device.StopCameras();
    }

    public override void OnNetworkSpawn() {
        dataList.OnListChanged += IntList_OnListChanged;
    }

    private void IntList_OnListChanged(NetworkListEvent<DataToSync> changeEvent) {
        if (dataList.Count == 40000) {
            Debug.Log(OwnerClientId + " : Lenght = " + dataList.Count + " vertex 100 : " + dataList[100].vertex + " color 100 : " + dataList[100].color + " index 100 : " + dataList[100].index);
            if (GameObject.FindGameObjectWithTag("PCModel") != null) {
                Debug.Log("oK");
                Mesh modelMesh = new Mesh();
                modelMesh.vertices = new Vector3[dataList.Count];
                modelMesh.colors32 = new Color32[dataList.Count];
                modelMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                int[] indices = new int[dataList.Count];
                Vector3[] v = new Vector3[dataList.Count];
                Color32[] c = new Color32[dataList.Count];
                for (int i = 0; i < dataList.Count; i++) {
                    v[i] = dataList[i].vertex;
                    c[i] = dataList[i].color;
                    indices[i] = i;
                }
                modelMesh.vertices = v;
                modelMesh.colors32 = c;
                modelMesh.SetIndices(indices, MeshTopology.Points, 0);
                GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh = modelMesh;
            }
        }
    }

    public struct DataToSync : INetworkSerializable, System.IEquatable<DataToSync> {
        public int index;
        public Vector3 vertex;
        public Color32 color;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref index);
            serializer.SerializeValue(ref color);
            serializer.SerializeValue(ref vertex);
        }

        public bool Equals(DataToSync other) {
            throw new System.NotImplementedException();
        }
    }
}
