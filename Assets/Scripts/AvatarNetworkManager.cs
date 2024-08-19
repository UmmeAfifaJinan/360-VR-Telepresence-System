using System;
using System.Collections;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Linq;
using SysDiag = System.Diagnostics;
using System.IO;

public class AvatarNetworkManager : MonoBehaviour
{
    [Header("Network Debug, Read Only")]
    public static connectionStatuses connectionStatus = connectionStatuses.Disconnected;
    public enum connectionStatuses { Disconnected, Connecting, Connected, ConnectedHost, ConnectedClient, Error }
    [SerializeField] bool TX; // transmit
    [SerializeField] bool RX; // receive
    public dataTypes dataTypeSent = dataTypes.NA;
    public dataTypes dataTypeReceived = dataTypes.NA;
    public enum dataTypes { NA, Trans, generic, voice, Mesh, BlendshapeWeight}
    [SerializeField] string lastReceived;
    [SerializeField] string lastSent;
    public string thisIP;

    [Header("Input Target")]
    public string targetIP;
    public int port;
   
    int hostSeed;
    UdpState s;
    [SerializeField] [Range(0, 1f)] float sendRate = 0.2f;
    float sendRateStamp;

    public static AvatarNetworkManager instance;

    dataTypes clientType = dataTypes.NA;
    string[] dataSplitSet, MposData, MrotData;

    // Avatar transmission
    [Header("Avatar Model")]
    public GameObject player;
    Transform[] playerJoints;
    Mesh playerMesh;
    public SkinnedMeshRenderer skinnedMeshRenderer;

    private static int HeaderSize = sizeof(int) * 2;

    // "connection" things
    IPEndPoint remoteEndPoint;
    UdpClient client;
    byte[] dataIn;
    bool messageReceived;
    AsyncCallback ascb;

    //microphoneData
    static byte[] micData;
    public static bool testMic;

    private float[] blendshapesWeights;

    private void Awake()
    {
        instance = this;
        //reduces errors when running and clicking out of window
        Application.runInBackground = true;
        //start time seed to determine which instance is Host. Fist starting instance is host.
        DateTime epochStart = new DateTime(2018, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        hostSeed = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        micData = new byte[0];
        testMic = false;
    }

    void Start()
    {
        // match the relevant player
        InitAvatarsData();
        //Begin connection routine
        StartCoroutine(initConnectAction());
    }

    private void Update()
    {
        updateDataIn();
        
        sendRateStamp += Time.deltaTime * sendRate;
        if (sendRateStamp >= 0.01f)
        {
            sendRateStamp = 0;
            updateDataOut();
        }
    }

    private void OnGUI()
    {
        //Monitor Sending and Receiving
        GUI.Label(new Rect(10, 0, 500, 20), String.Concat(connectionStatus.ToString(), (TX == true ? " TX" : "")));
        GUI.Label(new Rect(10, 25, 500, 20), String.Concat(connectionStatus.ToString(), (RX == true ? " RX" : "")));
        GUI.Label(new Rect(10, 50, 500, 20), lastReceived);
        GUI.Label(new Rect(10, 75, 500, 20), clientType.ToString());
        //GUI.Label(new Rect(10,100,500,20),micData.Length > 0 == true ? " VOIP IN" : "" );
    }

    private void InitAvatarsData()
    {
        if(player == null )
        {
            Debug.LogError("Please reference the player");
            return;
        }
        else
        {
            playerJoints = player.GetComponentsInChildren<Transform>();
            playerMesh = skinnedMeshRenderer.sharedMesh;
            blendshapesWeights = new float[playerMesh.blendShapeCount];

        }     
    }

    //recurring data out
    void updateDataOut()
    {
        if (client == null) { TX = false; return; } // abandon if no client

        else if (connectionStatus == connectionStatuses.ConnectedHost)
        {
            SendTrans();
            //SendMesh();
            SendWeights();
        }
    }

    void SendTrans()
    {
        dataTypeSent = dataTypes.Trans;
        Debug.Log("Data type send: " + dataTypeSent);
        lastSent = WriteTransforms(dataTypeSent);
        sendData(lastSent, dataTypeSent);
    }

    void SendWeights()
    {
        dataTypeSent = dataTypes.BlendshapeWeight;
        lastSent = WriteBlendshapesWeights(dataTypeSent);
        sendData(lastSent, dataTypeSent);
        Debug.Log("Sending blendshapes");
    }

    void SendMesh()
    {
        dataTypeSent = dataTypes.Mesh;
        byte[] m = WriteMeshes(dataTypeSent);
        sendData(m, dataTypeSent);
        Debug.Log("Sending mesh1");
    }

    //send unique command. must have identifiable characters for receiving end.
    public void sendGenericMessage(string m)
    {
        Debug.Log("[Network Manager] Sending Generic Message: " + m);
        lastSent = m;
        sendData(lastSent, dataTypes.generic);
    }

    public static void sendMicData(byte[] micOut)
    {
        if (testMic) { micData = micOut; return; }
        if (micOut == null || micOut.Length == 0) { return; }
        if (instance)
        {
            instance.sendData(addByteToArray(micOut, (byte)dataTypes.voice));
        }
    }

    public static byte[] getMicData()
    {
        if (micData != null) { return micData; }
        else { return new byte[0]; }
    }

    public static void clearMicData()
    {
        micData = new byte[0];
    }

    public static byte[] addByteToArray(byte[] bArray, byte newByte)
    {
        if (bArray == null || bArray.Length < 1) { return new byte[0]; }
        byte[] newArray = new byte[bArray.Length + 1];
        bArray.CopyTo(newArray, 1);
        newArray[0] = newByte;
        return newArray;
    }

    public static byte[] removeByteFromArray(byte[] bArray)
    {
        return bArray.Skip(1).ToArray();
    }

    //bytes then send
    public void sendData(string m, dataTypes type)
    {
        byte[] data = addByteToArray(Encoding.UTF8.GetBytes(m), (byte)type);
        try
        {
            TX = !string.IsNullOrEmpty(m);
            client.Send(data, data.Length, remoteEndPoint);
        }
        catch (Exception e)
        {
            TX = false;
            Debug.LogError(e);
            return;
        }
    }

    void sendData(byte[] m)
    {
        if (remoteEndPoint == null) { return; }

        client.Send(m, m.Length, remoteEndPoint);
    }

    private void sendData(byte[] m, dataTypes type)
    {
        byte[] data = addByteToArray(m, (byte)type);

        if (remoteEndPoint == null) { return; }
        client.Send(data, data.Length, remoteEndPoint);
    }

    //receive data must wait for "messageReceived" which is controlled by the Callback: ReceiveCallback()
    void updateDataIn()
    {
        RX = false;
        if (client == null) { return; }
        //if not initialized, initialize

        if (s.stateE == null || port == 0)
        {
            IPEndPoint e = new IPEndPoint(IPAddress.Any, port);
            s = new UdpState
            {
                stateE = e,
                stateU = client
            };
            ascb = new AsyncCallback(ReceiveCallback);
            client.BeginReceive(ascb, s);
        }
        //get data, switch recieved back to false, wait for listener;
        if (messageReceived)
        {
            processInData(dataIn);
            client.BeginReceive(ascb, s);
            messageReceived = false;
        }
        
    }

    void processInData(byte[] d)
    {
        lastReceived = string.Empty;
        if (d.Length > 0)
        {
            RX = true;
            byte byte1 = d[0];

            dataTypeReceived = (dataTypes)byte1;
            //microphone data
            if ((dataTypes)byte1 == dataTypes.voice)
            {
                micData = removeByteFromArray(d);
            }
            //generic data types are one-off commands, usually headed with a letter
            else if ((dataTypes)byte1 == dataTypes.generic)
            {
                lastReceived = Encoding.UTF8.GetString(removeByteFromArray(d));
                dataSplitSet = lastReceived.Split(':');
                if (dataSplitSet[0] == "H")
                { //Host request
                    if (dataSplitSet.Length == 2)
                    {
                        RX_establishHost(int.Parse(dataSplitSet[1]));
                    }
                }
                else if (dataSplitSet[0] == "I")
                { //Start Session Command
                    if (dataSplitSet.Length == 2)
                    {
                        RX_StartSession(int.Parse(dataSplitSet[1]));
                    }
                }
                else if (dataSplitSet[0] == "C")
                { //Match IP
                    if (dataSplitSet.Length == 2)
                    {
                        RX_establishConnection(dataSplitSet[1]);
                    }
                }
                else if (dataSplitSet[0] == "B")
                {
                    //if (dataSplitSet[1] == "start") { GameControl.instance.StartGame(); }
                    //else if (dataSplitSet[1] == "pass") { GameControl.instance.Pass_Button_Click(); }
                    //else if (dataSplitSet[1] == "next") { GameControl.instance.Next_Button_Click(); }
                    //else if (dataSplitSet[1] == "over") { GameControl.instance.GameOver(); }
                }
            }
            // read mesh
            else if ((dataTypes)byte1 == dataTypes.Mesh && connectionStatus == connectionStatuses.ConnectedClient)
            {
                clientType = dataTypes.Mesh;
                playerMesh = MeshDeserialize(removeByteFromArray(d));
                Debug.Log("Reading mesh1");
            }
            // read blendshapes
            else if ((dataTypes)byte1 == dataTypes.BlendshapeWeight && connectionStatus == connectionStatuses.ConnectedClient)
            {
                lastReceived = Encoding.UTF8.GetString(removeByteFromArray(d));
                String[] weightStr = lastReceived.Split('|');
                for(int i=0; i<weightStr.Length; i++)
                {
                    blendshapesWeights[i] = (float)Convert.ToDouble(weightStr[i]);
                }
                clientType = dataTypes.BlendshapeWeight;
                ReadBlendshapesWeights(blendshapesWeights, clientType);
                Debug.Log("Reading blendshapes");

            }

            // limb inputs fall into multiple categories for this project. Should be switched to a common data type as above.
            else
            {
                if(connectionStatus == connectionStatuses.ConnectedClient)
                {
                    lastReceived = Encoding.UTF8.GetString(removeByteFromArray(d));
                    dataSplitSet = lastReceived.Split(':');
                    if (dataSplitSet.Length >= 2)
                    {
                        MposData = dataSplitSet[0].Split('|');
                        MrotData = dataSplitSet[1].Split('|');
                        if ((dataTypes)byte1 == dataTypes.Trans) { clientType = dataTypes.Trans; }
                        ReadTransforms(MposData, MrotData, clientType);
                    }
                }
            }
        }
    }

    //generic test to see if connection accepted as host or client
    public bool networkReady()
    {
        return connectionStatus == connectionStatuses.ConnectedClient || connectionStatus == connectionStatuses.ConnectedHost;
    }

    //receive callback.  This was created specifically for Unity to remove multithreading. Callback uses asynchronous receives.

    public void ReceiveCallback(IAsyncResult ar)
    {
        UdpClient u = ((UdpState)(ar.AsyncState)).stateU;
        IPEndPoint e = ((UdpState)(ar.AsyncState)).stateE;
        dataIn = u.EndReceive(ar, ref e);
        messageReceived = true;
    }

    //are you the host in this connection?
    public bool isHost()
    {
        return connectionStatus == connectionStatuses.ConnectedHost;
    }

    public void TX_StartSession(int s)
    {
        if (connectionStatus == connectionStatuses.ConnectedHost)
        {
            sendGenericMessage(string.Concat("I:", s));
        }
    }
    void TX_establishConnection()
    {

        sendGenericMessage("C:" + thisIP);
    }
    void TX_establishHost()
    {
        sendGenericMessage("H:" + hostSeed);
    }

    void RX_StartSession(int s)
    {
        Debug.Log("[NetworkManager] Receiving RX_StartSession");
    }
    void RX_establishConnection(string ipIn)
    {
        Debug.Log("[NetworkManager] Receiving RX_establishConnection");
        if (connectionStatus == connectionStatuses.Connecting)
        {
            if (targetIP == ipIn) { connectionStatus = connectionStatuses.Connected; }
        }
    }
    void RX_establishHost(float seed)
    {
        Debug.Log("[NetworkManager] Receiving RX_establishHost");
        if (connectionStatus == connectionStatuses.Connected)
        {
            if (seed < hostSeed)
            {
                connectionStatus = connectionStatuses.ConnectedClient;
            }
            else
            {
                connectionStatus = connectionStatuses.ConnectedHost;
            }
        }
    }

    float[] GetBlendshapesWeights(SkinnedMeshRenderer skinnedMeshRenderer)
    {
        var index = skinnedMeshRenderer.sharedMesh.blendShapeCount;
        float[] Weights = new float[index];
        for(int i=0; i<index; i++)
        {
            Weights[i] = skinnedMeshRenderer.GetBlendShapeWeight(i);
        }
        return Weights;
    }
    string WriteBlendshapesWeights(dataTypes sentType)
    {
        string data = null;
        if(sentType == dataTypes.BlendshapeWeight)
        {
            for(int i=0; i<playerMesh.blendShapeCount; i++)
            {
                data += (GetBlendshapesWeights(skinnedMeshRenderer)[i].ToString("F3") + "|");
            }
            data = data.Remove(data.Length - 1);
        }
        return data;
    }

    void ReadBlendshapesWeights(float[] weights, dataTypes dt)
    {
        if(dt == dataTypes.BlendshapeWeight)
        {
            for(int i=0; i<weights.Length; i++)
            {
                skinnedMeshRenderer.SetBlendShapeWeight(i, weights[i]);
            }
        }
    }

    string WriteTransforms(dataTypes sentType)
    {
        string data = null;
        if (sentType == dataTypes.Trans)
        {           
            for (int i=0; i<playerJoints.Length; i++)
            {
                data += (playerJoints[i].position.ToString("F3") + "|" );
            }
            data = data.Remove(data.Length - 1);
            data += ":";
            for (int i = 0; i < playerJoints.Length; i++)
            {
                data += (playerJoints[i].rotation.ToString("F3") + "|");
            }
            data = data.Remove(data.Length - 1);
        }
        return data;
    }

    void ReadTransforms(string[] posData, string[] RotData, dataTypes dt)
    {
        if (dt == dataTypes.Trans)
        {
            for (int i = 0; i < posData.Length; i++)
            {
                playerJoints[i].position = StringToVector3(posData[i]);
                playerJoints[i].rotation = StringToQuaternion(RotData[i]);
            }
        }
    }

    private byte[] WriteMeshes(dataTypes sentType)
    {
        byte[] data = null;
        if (sentType == dataTypes.Mesh) { data = MeshSerialize(playerMesh); }
        return data;
    }

    private byte[] WriteMeshes(Mesh mesh)
    {
        byte[] data = null;
        data = MeshSerialize(mesh);
        return data;
    }

    public static Vector3 StringToVector3(string sVector)
    {
        // Remove the parentheses
        if (sVector.StartsWith("(") && sVector.EndsWith(")"))
        {
            sVector = sVector.Substring(1, sVector.Length - 2);
        }

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

    public static Quaternion StringToQuaternion(string sQuaternion)
    {
        // Remove the parentheses
        if (sQuaternion.StartsWith("(") && sQuaternion.EndsWith(")"))
        {
            sQuaternion = sQuaternion.Substring(1, sQuaternion.Length - 2);
        }

        // split the items
        string[] sArray = sQuaternion.Split(',');

        // store as a Vector3
        Quaternion result = new Quaternion(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]),
            float.Parse(sArray[3]));

        return result;
    }

    //start automatic connection process. You will need a UI to manually enter an IP and port.
    IEnumerator initConnectAction()
    {
        connectionStatus = connectionStatuses.Disconnected;
        yield return null;
        thisIP = GetIP(ADDRESSFAM.IPv4);
        //RDM_DemoCanvas.instance.setThisIpLabel(thisIP);

        //overall wait interval for scanning. Can add a counter for a timeout if needed.
        WaitForSeconds wait = new WaitForSeconds(0.5f);
        yield return new WaitForSeconds(5);
        //wait if IP or port are empty
        while (string.IsNullOrEmpty(targetIP) || port == 0)
        {
            yield return null;
        }
        //begin connection process
        client = new UdpClient(port);

        connectionStatus = connectionStatuses.Connecting;
        while (connectionStatus == connectionStatuses.Connecting)
        {
            remoteEndPoint = new IPEndPoint(IPAddress.Parse(targetIP), port);
            PlayerPrefs.SetString("IP", string.Concat(targetIP, ":", port));
            Debug.Log("[NetworkManager] Waiting on connection to targetIP");
            TX_establishConnection();
            yield return wait;
        }
        while (connectionStatus == connectionStatuses.Connected)
        {
            Debug.Log("[NetworkManager] Waiting on Host/Client establishment...");
            TX_establishConnection();
            TX_establishHost();
            yield return wait;
        }
        while (connectionStatus != connectionStatuses.Disconnected)
        {
            TX_establishHost();
            if (connectionStatus == connectionStatuses.ConnectedHost)
            {
                //Debug.Log("[NetworkManager] Host Methods");
                hostMethods();
                yield return wait;
            }
            else if (connectionStatus == connectionStatuses.ConnectedClient)
            {
                //Debug.Log("[NetworkManager] Client Methods");
                clientMethods();
                yield return wait;
            }
        }
    }

    void hostMethods()
    {
        
    }

    void clientMethods()
    {
        
    }

    // sendData
    private bool sendString(string message)
    {
        try
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            client.Send(data, data.Length, remoteEndPoint);
            return true;
        }
        catch (Exception err)
        {
            print(err.ToString());
            return false;
        }
    }

    private void OnDestroy()
    {
        if (client != null) { client.Dispose(); }
    }

    public static string GetIP(ADDRESSFAM Addfam)
    {
        //Return null if ADDRESSFAM is Ipv6 but Os does not support it
        if (Addfam == ADDRESSFAM.IPv6 && !Socket.OSSupportsIPv6)
        {
            return null;
        }

        string output = "";

        foreach (NetworkInterface item in NetworkInterface.GetAllNetworkInterfaces())
        {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

            if ((item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || item.NetworkInterfaceType == NetworkInterfaceType.Ethernet) && item.OperationalStatus == OperationalStatus.Up)
#endif 
            {
                foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                {
                    //IPv4
                    if (Addfam == ADDRESSFAM.IPv4)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            output = ip.Address.ToString();
                        }
                    }

                    //IPv6
                    else if (Addfam == ADDRESSFAM.IPv6)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetworkV6)
                        {
                            output = ip.Address.ToString();
                        }
                    }
                }
            }
        }
        return output;
    }

    //Serialize the mesh object to byte array
    public static byte[] MeshSerialize(Mesh mesh)
    {
        byte[] data = null;

        using (MemoryStream stream = new MemoryStream())
        {
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                WriteMesh(writer, mesh);

                stream.Position = 0;
                data = new byte[stream.Length];
                stream.Read(data, 0, data.Length);
            }
        }

        return data;
    }

    //Deserializes the mesh object from the provided byte array.
    public static Mesh MeshDeserialize(byte[] data)
    {
        Mesh mesh = new Mesh();

        using (MemoryStream stream = new MemoryStream(data))
        {
            using (BinaryReader reader = new BinaryReader(stream))
            {
                while (reader.BaseStream.Length - reader.BaseStream.Position >= HeaderSize)
                {
                    ReadMesh(reader);
                }
            }
        }

        return mesh;
    }

    //Writes a Mesh object to the data stream.
    private static void WriteMesh(BinaryWriter writer, Mesh mesh)
    {
        SysDiag.Debug.Assert(writer != null);

        // Write the mesh data.
        WriteMeshHeader(writer, mesh.vertexCount, mesh.triangles.Length);
        WriteVertices(writer, mesh.vertices);
        WriteTriangleIndicies(writer, mesh.triangles);
    }

    //Reads a single Mesh object from the data stream.
    private static Mesh ReadMesh(BinaryReader reader)
    {
        SysDiag.Debug.Assert(reader != null);

        int vertexCount = 0;
        int triangleIndexCount = 0;

        // Read the mesh data.
        ReadMeshHeader(reader, out vertexCount, out triangleIndexCount);
        Vector3[] vertices = ReadVertices(reader, vertexCount);
        int[] triangleIndices = ReadTriangleIndicies(reader, triangleIndexCount);

        // Create the mesh.
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangleIndices;
        // Reconstruct the normals from the vertices and triangles.
        mesh.RecalculateNormals();

        return mesh;
    }

    // Writes a mesh header to the data stream.
    private static void WriteMeshHeader(BinaryWriter writer, int vertexCount, int triangleIndexCount)
    {
        SysDiag.Debug.Assert(writer != null);

        writer.Write(vertexCount);
        writer.Write(triangleIndexCount);

    }

    // Reads a mesh header from the data stream.
    private static void ReadMeshHeader(BinaryReader reader, out int vertexCount, out int triangleIndexCount)
    {
        SysDiag.Debug.Assert(reader != null);

        vertexCount = reader.ReadInt32();
        triangleIndexCount = reader.ReadInt32();
    }

    // Writes a mesh's vertices to the data stream.
    private static void WriteVertices(BinaryWriter writer, Vector3[] vertices)
    {
        SysDiag.Debug.Assert(writer != null);

        foreach (Vector3 vertex in vertices)
        {
            writer.Write(vertex.x);
            writer.Write(vertex.y);
            writer.Write(vertex.z);
        }
    }

    // Reads a mesh's vertices from the data stream.
    private static Vector3[] ReadVertices(BinaryReader reader, int vertexCount)
    {
        SysDiag.Debug.Assert(reader != null);

        Vector3[] vertices = new Vector3[vertexCount];

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new Vector3(reader.ReadSingle(),
                                    reader.ReadSingle(),
                                    reader.ReadSingle());
        }

        return vertices;
    }

    // Writes the vertex indices that represent a mesh's triangles to the data stream
    private static void WriteTriangleIndicies(BinaryWriter writer, int[] triangleIndices)
    {
        SysDiag.Debug.Assert(writer != null);

        foreach (int index in triangleIndices)
        {
            writer.Write(index);
        }
    }

    // Reads the vertex indices that represent a mesh's triangles from the data stream
    private static int[] ReadTriangleIndicies(BinaryReader reader, int triangleIndexCount)
    {
        SysDiag.Debug.Assert(reader != null);

        int[] triangleIndices = new int[triangleIndexCount];

        for (int i = 0; i < triangleIndices.Length; i++)
        {
            triangleIndices[i] = reader.ReadInt32();
        }

        return triangleIndices;
    }
}

public enum ADDRESSFAM
{
    IPv4, IPv6
}

public struct UdpState
{
    public UdpClient stateU;
    public IPEndPoint stateE;
}

