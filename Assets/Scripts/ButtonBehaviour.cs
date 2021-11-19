using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonBehaviour : MonoBehaviour
{
    [SerializeField]
    private char buttonID;
    [SerializeField]
    private NetworkedClient client;

    [SerializeField]
    private TextMeshProUGUI textX;
    [SerializeField]
    private TextMeshProUGUI textO;

    [SerializeField]
    private bool isMessageButton = false;
    [SerializeField]
    private int messageID = 0;

    private bool isOccupied = false;

    public void OnClicked()
    {
        if (!isMessageButton)
        {
            if (!isOccupied)
            {
                client.SquareClicked(buttonID);
            }
        }
        else
        {
            client.SendMessage(messageID);
        }
    }

    public void PlaceX()
    {
        textX.gameObject.SetActive(true); 
        isOccupied = true;
    }

    public void PlaceO()
    {
        textO.gameObject.SetActive(true);
        isOccupied = true;
    }

    public void WipePlacement()
    {
        textX.gameObject.SetActive(false);
        textO.gameObject.SetActive(false);
        isOccupied = false;
    }
}
