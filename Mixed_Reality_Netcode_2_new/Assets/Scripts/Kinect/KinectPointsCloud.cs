using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Microsoft.Azure.Kinect.Sensor;
using System.Threading.Tasks;
using Unity.Netcode;

public class KinectPointsCloud : NetworkBehaviour {
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

    private Vector3[] temp_vertex; //array contenente i vertici temporanei da poi applicare al mesh lato client
    private Color32[] temp_colors; //array contenente i colrori temporanei da poi applicare al mesh lato client
    private int[] tempIndices; //array contenente gli indici temporanei da poi applicare al mesh lato client

    private int temp_call; //indica la chiamata attuale clientRPC

    public bool azureConnected = true; //Variabile che richiamo da MenuOptions per permettere o meno la pressione del bottone in base allo stato di connessione del kinect

    private float currTime;
    [SerializeField] private float transTime = 0.2f; //Tempo tra il trasferimento di un mesh e l'altro

    [Range(1, 2, order = 1)]
    [SerializeField] private int vertexToDiscard; //Decreta il numero di vertici da scartare (1 -> 0; 2 -> metà, non lo faccio booleano in caso voglia scartarne di più)
    [SerializeField] private Material[] meshMaterials; //Materiali da assegnare alla nuvola di punti
    [SerializeField] private int materialNumber; //indice per decidere che materiale assegnare

    [SerializeField] private float maxVerticesToAcquire = 35000f; //Numero massimo di vertici che possono essere inviati durante un acquisizione real time
    [SerializeField] private float firstLimitVertices = 14000f; //Numero massimo di vertici entro i quali è possibile trasmettere a 0.1f
    [SerializeField] private float secondLimitVertices = 24000f; //Numero massimo di vertici entro i quali è possibile trasmettere a 0.2f

    public bool captureVideo; //Richiamo da scanOptions.cs, per decidere quando eseguire scansione real time

    [SerializeField]private ChoosekinectSettings.KinectSetting currentSettings;
    //Richiamo dallo script ChooseKinectSettings
    public void SetRealTimeSetting(int m, int v, float fps, ChoosekinectSettings.KinectSetting newSettings) {
        transTime = fps;
        vertexToDiscard = v;
        materialNumber = m;
        currentSettings = newSettings;
    }

    private void Start() {
        try {
            InitializeKinect(); //inizializzo kinect
            InitializeMesh(); //inizializzo mesh definendo numero di vertici, colori e tipo di indici

            materialNumber = 0;
            vertexToDiscard = 2;
            transTime = 0.1f;
            currentSettings = ChoosekinectSettings.KinectSetting.performance; //All'avvio è in performance
        }
        catch (AzureKinectOpenDeviceException e) {
            Debug.Log("Device non disponibile per la connessione");
            azureConnected = false;
        }

        //Task t = GeneratePointCloudRealTime(); //eseguo thread per generare parallelamente il mesh dalla nuvola di punti
    }

    private void Update() {
        if (!IsServer) return;

        if ((Input.GetKey(KeyCode.K) || captureVideo) && azureConnected) {
            if (currTime <= 0) {
                SharedAcquisitionOpt();
                currTime = transTime;
            }
            else {
                currTime -= Time.deltaTime;
            }
        }

        /*
        if (Input.GetKeyDown(KeyCode.P)) {
            RestartDevice();
        }
        */
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

    //Funzione utilizzata per riavviare il kinect e cambiare impostazioni
    public void RestartDevice(DeviceConfiguration newConfiguration = null) {
        if(newConfiguration != null) { //Se è null non eseguo il codice
            bool needRestart = false;
            if (azureConnected && newConfiguration.DepthMode != config.DepthMode) { //Se devo effettuare un cambiamento nella depth mode è necessario riavviare il kinect, effettuo il cambiamento solo quando necessario per migliorare le prestazioni
                device.StopCameras();
                needRestart = true;
            }

            config = newConfiguration; //Assegno la nuova configurazione

            if (needRestart)
                device.StartCameras(config); //Faccio ripartire la camera se necessario

            transformation = device.GetCalibration().CreateTransformation(); //creo automaticamente una trasformazione sulla base della configurazione precedente
            InitializeMesh(); //inizializzo mesh definendo numero di vertici, colori e tipo di indici
        }
    }

    private void InitializeMesh() {
        int depthWidth = device.GetCalibration().DepthCameraCalibration.ResolutionWidth; // width della depth camera (640 con Unbinned)
        int depthHeight = device.GetCalibration().DepthCameraCalibration.ResolutionHeight; //height (576 con Unbinned)
        size = (depthWidth * depthHeight);

        mesh = new Mesh();

        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; //formato a 32 bit che supporta fino a 4 miliardi di vertici
    }

    private async Task GeneratePointCloudRealTime() {
        while (true) { //Sempre in esecuzione
            using (Capture capture = await Task.Run(() => device.GetCapture()).ConfigureAwait(true)) {
                GeneratePointCloudOpt(capture);
            }
        }
    }

    public void SharedAcquisitionOpt() {
        using (Capture capture = device.GetCapture()) {
            Debug.Log("Configurazione : " + config.DepthMode.ToString());
            GeneratePointCloudOpt(capture);
            //this.GetComponent<MeshDistances>().modelMesh = mesh;

            //Istanzio sia per server che per client l'oggetto, il quale lato host avrà il mesh corretto, ma lato client esso deve essere assegnato mediante ClientRPC

            if (GameObject.FindGameObjectWithTag("PCModel") == null) {
                GameObject item = Instantiate(modelToSpawn, transform.position, Quaternion.Euler(0f, 0f, 180f));
                item.GetComponent<MeshFilter>().mesh = mesh;
                item.GetComponent<NetworkObject>().Spawn(); //Non effettuo chiamata RPC in quanto l'oggetto può essere spawnato solo dall'host, quindi non c'è bisogno
            }
            else {
                GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh = mesh;
            }

            SendCalibrationInfoClientRpc(verticesList.Count, materialNumber); //In modo che a temp_vertex e temp_colors venga assegnata la dimensione pari a size per i vari client della rete

            //Ora devo effettuare diverse chiamate clientRPC in modo da poter aggiornare, senza andare in overflow, i vettori contenenti le informazioni riguardanti il mesh
            //Il problema è che il Mesh non è un tipo che può essere trasmesso mediante parametro di una chiamata RPC, e non può nemmeno essere serializzato in quanto può assumere valore null
            //Per questo devo innanzitutto inviare i dati lato host (dove ho effettuato l'acquisizione) dividendo il mesh nei vertici, colori e indici che lo compongono.
            //Per trasmettere i dati ho usato una struttura di appoggio "MeshSerialization" dove pongo i vari dati necessari serializzati
            //Per l'invio uso una chiamata ClientRPC, nella quale lato host invio i dati e poi il codice viene eseguito lato client (ricomposizione del mesh)
            //Qui però sorge un altro problema, ovvero il fatto che c'è un limite alla dimensione dei dati che possono essere inviati tramite chiamata RPC.
            //Per questo devo aggiungere parametri alla struct, in modo tale da ricomporre correttamente i dati lato client
            //Decido di divere i vari dati in blocchi di vettori di 3000 elementi

            int size_t = (verticesList.Count % 2 == 0) ? verticesList.Count : verticesList.Count - 1;
            int packetSize = 3000;
            Debug.Log("dimensione pacchetto : " + packetSize);
            int total_calls = size_t / packetSize; //numero totale di chiamate (Mi serviranno per capire quando fermarmi)

            MeshSerialization itemMesh; //Utilizzo una struct in cui applico serializzazione per trasmettere i vari dati necessari insieme
            itemMesh.total_calls = total_calls;
            itemMesh.size = size_t;

            for (int i = 0; i < total_calls; i++) {
                Vector3[] to_send_vertex = new Vector3[packetSize]; //vettori di appoggio che contengono i dati i-esimi da inviare
                Color32[] to_send_color = new Color32[packetSize];

                itemMesh.precIndex = i * packetSize; //indice di inizio i-esimo
                itemMesh.currIndex = (i + 1) * packetSize; //indice di fine i-esimo

                for (int j = 0; j < packetSize; j++) {
                    to_send_vertex[j] = verticesList[(i * packetSize) + j]; //riempio i dati da inviare lato server (host), sulla base dell'indice corrente
                    to_send_color[j] = colorList[(i * packetSize) + j];
                }
                //assegno i valori alle strutture presenti nella struct
                itemMesh.vertices = to_send_vertex;
                itemMesh.colors = to_send_color;
                ChangeMeshClientRPC(itemMesh); //invoco la chiamata lato client (per un numero di volte pari a total_calls)
            }
        }
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

            //int rand = Random.Range(0, vertexToDiscard);
            bool notDiscard = vertexToDiscard == 1 || (vertexToDiscard == 2 && i % 2 == 0); //Utilizzo in performance mode

            bool canAcquire = true;
            if(currentSettings != ChoosekinectSettings.KinectSetting.staticScan) { //Se non sto acquisendo una "foto" (in tal caso non ho limitazione nella trasmissione
                canAcquire = verticesList.Count < maxVerticesToAcquire; //se il numero di vertici è minore di 35000, posso acquisirne ancora di nuovi, altrimenti mi fermo
                if(currentSettings == ChoosekinectSettings.KinectSetting.performance && canAcquire) { //Se sono in performance
                    if(verticesList.Count > firstLimitVertices && verticesList.Count < secondLimitVertices) { //All'interno di questo range di vertici diminuisco il frame rate a 0.2
                        transTime = 0.2f;
                    }else if(verticesList.Count > secondLimitVertices) { //Oltre questo lo diminuisco ulteriormente
                        transTime = 0.3f;
                    }
                    else { //altrimenti vuol dire che sono in un range nel quale mi è garantito stare a 0.1
                        transTime = 0.1f;
                    }
                }
                else { //Se sono in quality mode
                    if (canAcquire) { //E non ho superato i 35k vertici
                        if(verticesList.Count > secondLimitVertices) {
                            transTime = 0.3f;
                        }
                        else {
                            transTime = 0.2f;
                        }
                    }
                }
            }
            
            if (points_i != Vector3.zero /*&& rand == 0*/ && notDiscard && canAcquire) {
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

    [ClientRpc]
    private void ChangeMeshClientRPC(MeshSerialization modelMesh) {
        temp_call++; //variabile che viene utilizzata per capire se ho finito di ricevere i dati dalla chiamata RPC invocata lato server

        Debug.Log("Temp_call : " + temp_call);

        //Ora qui uso due vettori temporanei, che riempio man mano con i dati che ricevo lato server (host)
        for (int i = modelMesh.precIndex; i < modelMesh.currIndex; i++) { //dall'indice a cui mi sono fermato prima fino a quello successivo
            temp_vertex[i] = modelMesh.vertices[i - modelMesh.precIndex];
            temp_colors[i] = modelMesh.colors[i - modelMesh.precIndex];
            tempIndices[i] = i;
        }

        if (modelMesh.total_calls == temp_call) { //Una volta acquisiti tutti i dati
            Debug.Log("Trasmissione mesh terminata");

            //Qui cerco l'oggetto nella scene e cambio il suo mesh lato client
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh = new Mesh();
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.vertices = new Vector3[modelMesh.size];
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.colors32 = new Color32[modelMesh.size];
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.vertices = temp_vertex;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.colors32 = temp_colors;
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.SetIndices(tempIndices, MeshTopology.Points, 0);
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshFilter>().mesh.RecalculateBounds();

            //IN BASE AL MATERIAL NUMBER SI CAMBIA IL MATERIALE DEL MESH RENDERER
            Debug.Log("Material : " + materialNumber);
            GameObject.FindGameObjectWithTag("PCModel").GetComponent<MeshRenderer>().material = meshMaterials[materialNumber];
        }
    }


    [ClientRpc]
    private void SendCalibrationInfoClientRpc(int size, int matNumber) { //Questo deve essere fatto una sola volta, posso fare una chiamata a parte
        this.temp_vertex = new Vector3[size];
        this.temp_colors = new Color32[size];
        this.tempIndices = new int[size];
        temp_call = 0; //Resetto temp_call lato client, in modo da poter eseguire nuovamente il controllo di fine trasmissione mesh
        this.materialNumber = matNumber;
        Debug.Log(temp_vertex.Length);
    }

    struct MeshSerialization : INetworkSerializable { //Struct che implementa interfaccia di serializzazione per la trasmissione dei dati da server a client
        public Vector3[] vertices;
        public Color32[] colors;
        //public int[] indices;

        public int precIndex;
        public int currIndex;
        public int total_calls;
        public int size;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter {
            serializer.SerializeValue(ref vertices);
            serializer.SerializeValue(ref colors);
            //serializer.SerializeValue(ref indices);
            serializer.SerializeValue(ref currIndex);
            serializer.SerializeValue(ref precIndex);
            serializer.SerializeValue(ref total_calls);
            serializer.SerializeValue(ref size);
        }
    }
}