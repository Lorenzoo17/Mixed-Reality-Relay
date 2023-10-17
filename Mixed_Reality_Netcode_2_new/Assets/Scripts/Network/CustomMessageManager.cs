using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Netcode;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Microsoft.Azure.Kinect.Sensor;
using TMPro;

public class CustomMessageManager : NetworkBehaviour {
    [Tooltip("The name identifier used for this custom message handler.")]
    public string MessageData = "MessageData";
    public string MessageSize = "MessageSize";



    /// <summary>
    /// For most cases, you want to register once your NetworkBehaviour's
    /// NetworkObject (typically in-scene placed) is spawned.
    /// </summary>
    public override void OnNetworkSpawn() {
        // Both the server-host and client(s) register the custom named message.
        NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageData, ReceiveData);
        NetworkManager.CustomMessagingManager.RegisterNamedMessageHandler(MessageSize, ReceiveSize);

        if (IsServer) {
            // Server broadcasts to all clients when a new client connects (just for example purposes)
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
        else {
            // Clients send a unique Guid to the server
            //SendMessage(Guid.NewGuid());
        }
    }

    private void OnClientConnectedCallback(ulong obj) {
        //SendMessage(Guid.NewGuid());
    }

    public override void OnNetworkDespawn() {
        // De-register when the associated NetworkObject is despawned.
        NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageData);
        NetworkManager.CustomMessagingManager.UnregisterNamedMessageHandler(MessageSize);

        // Whether server or not, unregister this.
        NetworkManager.OnClientDisconnectCallback -= OnClientConnectedCallback;
    }

    /// <summary>
    /// Invoked when a custom message of type <see cref="MessageName"/>
    /// </summary>
    private void ReceiveData(ulong senderId, FastBufferReader messagePayload) {
        //var receivedMessageContent = new ForceNetworkSerializeByMemcpy<Vector3>(new Vector3());
        byte[] receivedDataSerialized;
        messagePayload.ReadValueSafe(out receivedDataSerialized);

        DataToSend receivedData = (DataToSend)ToObject(receivedDataSerialized);
        //Debug.Log("Index : " + receivedData.index + ", next : " + receivedData.nextIndex);
        for (int i = receivedData.index; i < receivedData.nextIndex; i++) {
            int indexFetch = i - receivedData.index;

            storedVertices[i] = new Vector3(receivedData.xValues[indexFetch], receivedData.yValues[indexFetch], receivedData.zValues[indexFetch]);
            storedColors[i] = new Color32(receivedData.rValues[indexFetch], receivedData.gValues[indexFetch], receivedData.bValues[indexFetch], receivedData.aValues[indexFetch]);
            storedIndices[i] = receivedData.meshIndices[indexFetch];
        }

        //Debug.Log(storedVertices.Length);
        //Debug.Log(receivedData.nextIndex);
        //MeshBuilding
        if (GameObject.FindGameObjectWithTag("PCModel") != null && (receivedData.nextIndex + 40 > storedVertices.Length)) {
            Debug.Log("Start mesh");
            Debug.Log("Stored : " + storedVertices.Length + ", received : " + receivedData.nextIndex);
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.Clear();
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh = new Mesh();
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.vertices = new Vector3[storedVertices.Length];
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.colors32 = new Color32[storedColors.Length];
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.vertices = storedVertices;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.colors32 = storedColors;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.SetIndices(storedIndices, MeshTopology.Points, 0);
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.RecalculateBounds();
        }

        if (IsServer) {
            if (receivedData.nextIndex == 20000)
                Debug.Log($"Sever received ( Vertices : {receivedData.xValues.Length}, Colors : {receivedData.rValues.Length}, index : {receivedData.index}) from client ({senderId})");
        }
        else {
            if (receivedData.nextIndex == 20000)
                Debug.Log($"Client received ( Vertices : {receivedData.xValues.Length}, Colors : {receivedData.rValues.Length}, index : {receivedData.index}) from the server.");
        }
    }

    /// <summary>
    /// Invoke this with a Guid by a client or server-host to send a
    /// custom named message.
    /// </summary>
    public void SendData(DataToSend data) {
        byte[] serializedData = ToBytes(data);

        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(serializedData), Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        using (writer) {
            writer.WriteValueSafe(serializedData);
            if (IsServer) {
                // This is a server-only method that will broadcast the named message.
                // Caution: Invoking this method on a client will throw an exception!
                customMessagingManager.SendNamedMessageToAll(MessageData, writer);
            }
            else {
                // This is a client or server method that sends a named message to one target destination
                // (client to server or server to client)
                customMessagingManager.SendNamedMessage(MessageData, NetworkManager.ServerClientId, writer);
            }
        }
    }

    private void ReceiveSize(ulong senderId, FastBufferReader messagePayload) {
        //var receivedMessageContent = new ForceNetworkSerializeByMemcpy<Vector3>(new Vector3());
        int receivedSize;
        messagePayload.ReadValueSafe(out receivedSize);

        storedColors = new Color32[receivedSize];
        storedVertices = new Vector3[receivedSize];
        storedIndices = new int[receivedSize];
        if (IsServer) {
            //Debug.Log($"Sever received ( Vertices : {receivedData.xValues.Length}, Colors : {receivedData.rValues.Length}, index : {receivedData.index}) from client ({senderId})");
        }
        else {
            //Debug.Log($"Client received ( Vertices : {receivedData.xValues.Length}, Colors : {receivedData.rValues.Length}, index : {receivedData.index}) from the server.");
        }
    }

    public void SendSize(int meshSize) {

        var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize(meshSize), Allocator.Temp);
        var customMessagingManager = NetworkManager.CustomMessagingManager;
        using (writer) {
            writer.WriteValueSafe(meshSize);
            if (IsServer) {
                // This is a server-only method that will broadcast the named message.
                // Caution: Invoking this method on a client will throw an exception!
                customMessagingManager.SendNamedMessageToAll(MessageSize, writer);
            }
            else {
                // This is a client or server method that sends a named message to one target destination
                // (client to server or server to client)
                customMessagingManager.SendNamedMessage(MessageSize, NetworkManager.ServerClientId, writer);
            }
        }
    }

    private Vector3[] storedVertices;
    private Color32[] storedColors;
    private int[] storedIndices;

    private Device device;
    private DeviceConfiguration config;

    private Mesh mesh; //Mesh dell'oggetto al quale voglio applicare la nuvola di punti
    private List<Vector3> verticesList = new List<Vector3>();//Vertici del mesh della nuvola di punti --> Essendo il mesh una nuvola di punti, un vertice corrisponde ad un punto di quest'ultima
    private List<Color32> colorList = new List<Color32>();//Colore da applicare ai singoli punti della nuvola di punti
    private int[] optIndices;//Indici del mesh per la nuvola di punti

    private Transformation transformation; //Definisco la trasformazione che verrà utilizzata per il passaggio da un sistema di coordinate all'altro
    private int size; //dimensione totale (width * height) necessario lavorare con size per array di vertici ecc

    [SerializeField] private float viewDistance = 1000f; //Distanza oltre la quale i dati della depth camera non vengono presi in considerazione per lo sviluppo della nuvola di punti (default ad 1 metro : 1000f)

    [SerializeField] private GameObject modelToSpawn; //Modello da spawnare al quale applicare il mesh

    public bool azureConnected = true; //Variabile che richiamo da MenuOptions per permettere o meno la pressione del bottone in base allo stato di connessione del kinect

    [System.Serializable]
    public class DataToSend {

        public int index;
        public int nextIndex;
        public int[] meshIndices;
        public float[] xValues;
        public float[] yValues;
        public float[] zValues;

        public byte[] rValues;
        public byte[] gValues;
        public byte[] bValues;
        public byte[] aValues;

        public void SetValues(Vector3[] vertices, Color32[] colors, int[] meshIndices, int index, int nextIndex) {
            this.index = index;
            this.nextIndex = nextIndex;
            this.meshIndices = new int[meshIndices.Length];
            this.meshIndices = meshIndices;
            xValues = new float[vertices.Length];
            yValues = new float[vertices.Length];
            zValues = new float[vertices.Length];

            rValues = new byte[colors.Length];
            gValues = new byte[colors.Length];
            bValues = new byte[colors.Length];
            aValues = new byte[colors.Length];
            for (int i = 0; i < vertices.Length; i++) {
                xValues[i] = vertices[i].x;
                yValues[i] = vertices[i].x;
                zValues[i] = vertices[i].x;

                rValues[i] = colors[i].r;
                gValues[i] = colors[i].g;
                bValues[i] = colors[i].b;
                aValues[i] = colors[i].a;
            }
        }
    }

    private DataToSend myData;
    private void Start() {
        /*
        for (int i = 0; i < vertices.Length; i++) {
            vertices[i] = new Vector3(i, i, i);
            colors[i] = new Color32(255, 255, 255, 255);
        }
        */

        try {
            InitializeKinect(); //inizializzo kinect
            InitializeMesh(); //inizializzo mesh definendo numero di vertici, colori e tipo di indici
        }
        catch (AzureKinectOpenDeviceException e) {
            Debug.Log("Device non disponibile per la connessione");
            azureConnected = false;
        }

        myData = new DataToSend();
        //myData.SetValues(vertices, colors, 1);
    }

    private void Update() {
        if (Input.GetKey(KeyCode.P) && azureConnected) {
            using (Capture capture = device.GetCapture()) {
                GeneratePointCloudOpt(capture);

                SendSize(verticesList.Count);

                if (GameObject.FindGameObjectWithTag("PCModel") == null) {
                    GameObject item = Instantiate(modelToSpawn, transform.position, Quaternion.Euler(0f, 0f, 180f));
                    item.GetComponent<MeshFilter>().mesh = mesh;
                    item.GetComponent<NetworkObject>().Spawn(); //Non effettuo chiamata RPC in quanto l'oggetto può essere spawnato solo dall'host, quindi non c'è bisogno
                }
                else {
                    GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh = mesh;
                }

                for (int i = 0; i < (int)verticesList.Count / 40; i++) {
                    DataToSend newData = new DataToSend();
                    Vector3[] verticesToSend = new Vector3[40];
                    Color32[] colorsToSend = new Color32[40];
                    int[] indicesToSend = new int[40];
                    for (int j = i * 40; j < (i * 40) + 40; j++) {
                        verticesToSend[j - (i * 40)] = verticesList[j];
                        colorsToSend[j - (i * 40)] = colorList[j];
                        indicesToSend[j - (i * 40)] = optIndices[j];
                    }
                    int index = (i * 40);
                    int nextIndex = (i * 40) + 40;
                    newData.SetValues(verticesToSend, colorsToSend, indicesToSend, index, nextIndex);
                    SendData(newData);
                }
                Debug.Log("Stored vertices :" + storedVertices[storedVertices.Length - 1]);
            }
        }
    }

    private void InitializeKinect() {
        device = Device.Open(0);

        config = new DeviceConfiguration();
        config.CameraFPS = FPS.FPS30;
        config.ColorFormat = ImageFormat.ColorBGRA32;
        config.ColorResolution = ColorResolution.R720p;
        config.DepthMode = DepthMode.NFOV_2x2Binned; //immagine depth 640x576
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
    }

    private void GeneratePointCloudOpt(Capture capture) {
        Image colorImage = transformation.ColorImageToDepthCamera(capture); //per ottenere un immagine a colori dalla prospettiva della depth camera

        BGRA[] colorArray = colorImage.GetPixels<BGRA>().ToArray(); //ottengo l'array dei pixel in formato BGRA dell'immagine precedentemente acquisita

        Image xyzImage = transformation.DepthImageToPointCloud(capture.Depth); //ottengo un immagine di punti tridimensionali unendo le informazioni dell'immagine a colori con la depth camera
        Short3[] xyzArray = xyzImage.GetPixels<Short3>().ToArray(); //qua semplicemente converto l'immagine in array di punti nello spazio 3D

        verticesList.Clear();
        colorList.Clear();
        for (int i = 0; i < size; i++) {
            Vector3 points_i = new Vector3();
            points_i.x = xyzArray[i].X;
            points_i.y = xyzArray[i].Y;
            points_i.z = xyzArray[i].Z;

            Color32 color_i = new Color32();
            color_i.r = colorArray[i].R;
            color_i.g = colorArray[i].G;
            color_i.b = colorArray[i].B;
            color_i.a = 255;

            if (xyzArray[i].Z > viewDistance || xyzArray[i].Z < 500f) { //se il punto è oltre 1 metro (1000 mm) non lo mappo all'interno del mesh (assegno coordinata [0,0,0])
                points_i = Vector3.zero;
            }

            if (points_i != Vector3.zero) {
                verticesList.Add(points_i);
                colorList.Add(color_i);
            }
        }
        optIndices = new int[verticesList.Count];
        for (int i = 0; i < optIndices.Length; i++) {
            optIndices[i] = i;
        }
        mesh.Clear();
        mesh.vertices = verticesList.ToArray(); //ora assegno i vertici alla mesh
        mesh.colors32 = colorList.ToArray(); //lo stesso per i colori
        mesh.SetIndices(optIndices, MeshTopology.Points, 0);
        mesh.RecalculateBounds(); //e ricalcolo il volume avendo cambiato i vertici 
    }

    private void OnDestroy() { //alla chiusura della scene / app
        if (azureConnected)
            device.StopCameras(); //chiudo la camera
    }

    private byte[] ToBytes(System.Object obj) {
        if (obj == null)
            return null;

        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        bf.Serialize(ms, obj);

        return ms.ToArray();
    }

    private System.Object ToObject(byte[] arrBytes) {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream ms = new MemoryStream();
        ms.Write(arrBytes, 0, arrBytes.Length);
        ms.Seek(0, SeekOrigin.Begin);
        System.Object obj = (System.Object)bf.Deserialize(ms);

        return obj;
    }

    [Serializable]
    public struct SVector3 {
        public float x;
        public float y;
        public float z;

        public SVector3(float x, float y, float z) {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
            => $"[x, y, z]";

        public static implicit operator Vector3(SVector3 s)
            => new Vector3(s.x, s.y, s.z);

        public static implicit operator SVector3(Vector3 v)
            => new SVector3(v.x, v.y, v.z);


        public static SVector3 operator +(SVector3 a, SVector3 b)
            => new SVector3(a.x + b.x, a.y + b.y, a.z + b.z);

        public static SVector3 operator -(SVector3 a, SVector3 b)
            => new SVector3(a.x - b.x, a.y - b.y, a.z - b.z);

        public static SVector3 operator -(SVector3 a)
            => new SVector3(-a.x, -a.y, -a.z);

        public static SVector3 operator *(SVector3 a, float m)
            => new SVector3(a.x * m, a.y * m, a.z * m);

        public static SVector3 operator *(float m, SVector3 a)
            => new SVector3(a.x * m, a.y * m, a.z * m);

        public static SVector3 operator /(SVector3 a, float d)
            => new SVector3(a.x / d, a.y / d, a.z / d);
    }

    [Serializable]
    public struct SColor32 {
        public byte r;
        public byte g;
        public byte b;

        public SColor32(byte r, byte g, byte b) {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public SColor32(Color32 c) {
            r = c.r;
            g = c.g;
            b = c.b;
        }

        public override string ToString()
            => $"[{r}, {g}, {b}]";

        public static implicit operator Color32(SColor32 rValue)
            => new Color32(rValue.r, rValue.g, rValue.b, a: byte.MaxValue);

        public static implicit operator SColor32(Color32 rValue)
            => new SColor32(rValue.r, rValue.g, rValue.b);
    }
}