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

    private bool isOccupied = false;

    public void OnClicked()
    {
        if (!isOccupied)
        {
            client.SquareClicked(buttonID);
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
}
