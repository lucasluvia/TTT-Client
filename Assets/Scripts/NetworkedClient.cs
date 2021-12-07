using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class NetworkedClient : MonoBehaviour
{
    [SerializeField] ButtonBehaviour buttonTL;
    [SerializeField] ButtonBehaviour buttonTM;
    [SerializeField] ButtonBehaviour buttonTR;
    [SerializeField] ButtonBehaviour buttonCL;
    [SerializeField] ButtonBehaviour buttonCM;
    [SerializeField] ButtonBehaviour buttonCR;
    [SerializeField] ButtonBehaviour buttonBL;
    [SerializeField] ButtonBehaviour buttonBM;
    [SerializeField] ButtonBehaviour buttonBR;

    [SerializeField] TextMeshProUGUI MessageText;
    [SerializeField] TextMeshProUGUI TextText;
    [SerializeField] GameObject TextPanel;

    bool isMyTurn;
    bool isGameOver = false;
    bool canRestart = false;
    bool isSpectator = false;

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
            MessageText.text = "Waiting for other player...";
        }

        UpdateNetworkConnection();
        

    }

    public void SquareClicked(string SquareID)
    {
        if(isMyTurn && !isGameOver && !isSpectator)
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
        string location;
        switch (signifier)
        {
            case ServerToClientSignifiers.XValuePlaced:
                location = (csv[2]);
                inPositionPlaceX(location);
                location = "Z";
                break;
            case ServerToClientSignifiers.OValuePlaced:
                location = (csv[2]);
                inPositionPlaceO(location);
                location = "Z";
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
                if (isSpectator)
                    canRestart = false;
                break;
            case ServerToClientSignifiers.ReceiveText:
                TextText.text = csv[1].ToString();
                canRestart = true;
                break;
            case ServerToClientSignifiers.Spectating:
                MessageText.text = "Spectating the game...";
                TextPanel.SetActive(false);
                isSpectator = true;
                canRestart = false;
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
        buttonTL.WipePlacement();
        buttonTM.WipePlacement();
        buttonTR.WipePlacement();
        buttonCL.WipePlacement();
        buttonCM.WipePlacement();
        buttonCR.WipePlacement();
        buttonBL.WipePlacement();
        buttonBM.WipePlacement();
        buttonBR.WipePlacement();
    }


    private void inPositionPlaceX(string location)
    {
        if (location == "TL")
            buttonTL.PlaceX();
        else if (location == "TM")
            buttonTM.PlaceX();
        else if (location == "TR")
            buttonTR.PlaceX();
        else if (location == "CL")
            buttonCL.PlaceX();
        else if (location == "CM")
            buttonCM.PlaceX();
        else if (location == "CR")
            buttonCR.PlaceX();
        else if (location == "BL")
            buttonBL.PlaceX();
        else if (location == "BM")
            buttonBM.PlaceX();
        else if (location == "BR")
            buttonBR.PlaceX();
    }

    private void inPositionPlaceO(string clickedSquare)
    {
        if (clickedSquare == "TL")
            buttonTL.PlaceO();
        else if (clickedSquare == "TM")
            buttonTM.PlaceO();
        else if (clickedSquare == "TR")
            buttonTR.PlaceO();
        else if (clickedSquare == "CL")
            buttonCL.PlaceO();
        else if (clickedSquare == "CM")
            buttonCM.PlaceO();
        else if (clickedSquare == "CR")
            buttonCR.PlaceO();
        else if (clickedSquare == "BL")
            buttonBL.PlaceO();
        else if (clickedSquare == "BM")
            buttonBM.PlaceO();
        else if (clickedSquare == "BR")
            buttonBR.PlaceO();
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
    public const int Spectating = 11;
}