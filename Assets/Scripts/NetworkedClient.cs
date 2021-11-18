using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkedClient : MonoBehaviour
{
    [SerializeField] ButtonBehaviour buttonA;
    [SerializeField] ButtonBehaviour buttonB;
    [SerializeField] ButtonBehaviour buttonC;
    [SerializeField] ButtonBehaviour buttonD;
    [SerializeField] ButtonBehaviour buttonE;
    [SerializeField] ButtonBehaviour buttonF;
    [SerializeField] ButtonBehaviour buttonG;
    [SerializeField] ButtonBehaviour buttonH;
    [SerializeField] ButtonBehaviour buttonI;

    bool isMyTurn;

    int connectionID;
    int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 5491;
    byte error;
    bool isConnected = false;
    int ourClientID;

    // Start is called before the first frame update
    void Start()
    {
        Connect();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateNetworkConnection();
    }

    public void SquareClicked(char SquareID)
    {
        if(isMyTurn)
        {
            SendMessageToHost(ClientToServerSignifiers.ClickedSquare + "," + SquareID);
            isMyTurn = false;
        }

    }

    private void UpdateNetworkConnection()
    {
        if (isConnected)
        {
            int recHostID;
            int recConnectionID;
            int recChannelID;
            byte[] recBuffer = new byte[1024];
            int bufferSize = 1024;
            int dataSize;
            NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

            switch (recNetworkEvent)
            {
                case NetworkEventType.ConnectEvent:
                    Debug.Log("connected.  " + recConnectionID);
                    ourClientID = recConnectionID;
                    break;
                case NetworkEventType.DataEvent:
                    string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                    ProcessRecievedMsg(msg, recConnectionID);
                    //Debug.Log("got msg = " + msg);
                    break;
                case NetworkEventType.DisconnectEvent:
                    isConnected = false;
                    Debug.Log("disconnected.  " + recConnectionID);
                    break;
            }
        }
    }

    private void Connect()
    {

        if (!isConnected)
        {
            Debug.Log("Attempting to create connection");

            NetworkTransport.Init();

            ConnectionConfig config = new ConnectionConfig();
            reliableChannelID = config.AddChannel(QosType.Reliable);
            unreliableChannelID = config.AddChannel(QosType.Unreliable);
            HostTopology topology = new HostTopology(config, maxConnections);
            hostID = NetworkTransport.AddHost(topology, 0);
            Debug.Log("Socket open.  Host ID = " + hostID);

            connectionID = NetworkTransport.Connect(hostID, "192.168.2.25", socketPort, 0, out error); // server is local on network

            if (error == 0)
            {
                isConnected = true;

                Debug.Log("Connected, id = " + connectionID);

            }
        }
    }

    public void Disconnect()
    {
        NetworkTransport.Disconnect(hostID, connectionID, out error);
    }

    public void SendMessageToHost(string msg)
    {
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, connectionID, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }

    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

        string[] csv = msg.Split(',');

        int signifier = int.Parse(csv[0]);
        char location;
        switch (signifier)
        {
            case ServerToClientSignifiers.XValuePlaced:
                Debug.Log("XValuePlaced");
                location = char.Parse(csv[2]);
                Debug.Log("location = " + location);
                if (location == 'A')
                    buttonA.PlaceX();
                else if (location == 'B')
                    buttonB.PlaceX();
                else if (location == 'C')
                    buttonC.PlaceX();
                else if (location == 'D')
                    buttonD.PlaceX();
                else if (location == 'E')
                    buttonE.PlaceX();
                else if (location == 'F')
                    buttonF.PlaceX();
                else if (location == 'G')
                    buttonG.PlaceX();
                else if (location == 'H')
                    buttonH.PlaceX();
                else if (location == 'I')
                    buttonI.PlaceX();
                location = 'J';
                break;
            case ServerToClientSignifiers.OValuePlaced:
                Debug.Log("OValuePlaced");
                location = char.Parse(csv[2]);
                if (location == 'A')
                    buttonA.PlaceO();
                else if (location == 'B')
                    buttonB.PlaceO();
                else if (location == 'C')
                    buttonC.PlaceO();
                else if (location == 'D')
                    buttonD.PlaceO();
                else if (location == 'E')
                    buttonE.PlaceO();
                else if (location == 'F')
                    buttonF.PlaceO();
                else if (location == 'G')
                    buttonG.PlaceO();
                else if (location == 'H')
                    buttonH.PlaceO();
                else if (location == 'I')
                    buttonI.PlaceO();
                location = 'J';
                break;
            case ServerToClientSignifiers.ValueNotPlaced:
                Debug.Log("ValueNotPlaced");
                break;
            case ServerToClientSignifiers.ItsYourTurn:
                Debug.Log("ItsYourTurn");
                isMyTurn = true;
                break;

        }

    }

    public bool IsConnected()
    {
        return isConnected;
    }


}

static public class ClientToServerSignifiers
{
    public const int ClickedSquare = 1;
}

static public class ServerToClientSignifiers
{
    public const int XValuePlaced = 1;
    public const int OValuePlaced = 2;
    public const int ValueNotPlaced = 3;
    public const int ItsYourTurn = 4;
}