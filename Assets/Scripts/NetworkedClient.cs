using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

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

    [SerializeField] TextMeshProUGUI MessageText;
    [SerializeField] TextMeshProUGUI TextText;

    bool isMyTurn;
    bool isGameOver = false;
    bool canRestart = false;

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
        
        if(Input.GetKeyDown(KeyCode.R) && canRestart)
        {
            SendMessageToHost(ClientToServerSignifiers.Restart + "");
        }

        UpdateNetworkConnection();
        

    }

    public void SquareClicked(char SquareID)
    {
        if(isMyTurn && !isGameOver)
        {
            SendMessageToHost(ClientToServerSignifiers.ClickedSquare + "," + SquareID);
            isMyTurn = false;
            MessageText.text = "Waiting for other player...";
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
                    MessageText.text = "Waiting for other player...";
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
                    MessageText.text = "Disconnected from room";
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
            case ServerToClientSignifiers.ItsYourTurn:
                Debug.Log("ItsYourTurn");
                MessageText.text = "It's your turn!";
                isMyTurn = true;
                break;
            case ServerToClientSignifiers.YouWon:
                Debug.Log("You Won! Press R to Restart");
                MessageText.text = "You Won! Watching replay...";
                isGameOver = true;
                break;
            case ServerToClientSignifiers.YouLost:
                MessageText.text = "You Lost! Watching replay...";
                Debug.Log(csv[1].ToString());
                isGameOver = true;
                break;
            case ServerToClientSignifiers.Tie:
                MessageText.text = "Game Tied. Watching replay...";
                Debug.Log(csv[1].ToString());
                isGameOver = true;
                break;
            case ServerToClientSignifiers.WipeBoard:
                MessageText.text = "Waiting for other player...";
                WipeBoard();
                break;
            case ServerToClientSignifiers.WatchReplay:
                WipeButtons();
                break;
            case ServerToClientSignifiers.WantToRestart:
                MessageText.text = "Press R if you want to restart.";
                canRestart = true;
                break;
            case ServerToClientSignifiers.ReceiveText:
                TextText.text = csv[1].ToString();
                canRestart = true;
                break;

        }

    }

    public void SendMessage(int messageID)
    {
        string message = "";
        if (messageID == 1)
            message = "Sorry!";
        if (messageID == 2)
            message = "One More!";
        if (messageID == 3)
            message = "Good One!";
        if (messageID == 4)
            message = "Nice Try!";

        SendMessageToHost(ClientToServerSignifiers.SendText + "," + message);
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    private void WipeBoard()
    {
        WipeButtons();
        isGameOver = false;
        canRestart = false;
    }
    private void WipeButtons()
    {
        buttonA.WipePlacement();
        buttonB.WipePlacement();
        buttonC.WipePlacement();
        buttonD.WipePlacement();
        buttonE.WipePlacement();
        buttonF.WipePlacement();
        buttonG.WipePlacement();
        buttonH.WipePlacement();
        buttonI.WipePlacement();
    }


}

static public class ClientToServerSignifiers
{
    public const int ClickedSquare = 1;
    public const int Replay = 2;
    public const int Restart = 3;
    public const int SendText = 4;

}

static public class ServerToClientSignifiers
{
    public const int XValuePlaced = 1;
    public const int OValuePlaced = 2;
    public const int ItsYourTurn = 3;
    public const int YouWon = 4;
    public const int YouLost = 5;
    public const int Tie = 6;
    public const int WipeBoard = 7;
    public const int WatchReplay = 8;
    public const int WantToRestart = 9;
    public const int ReceiveText = 10;
}