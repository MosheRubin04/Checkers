using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour
{

    public static GameManager Instance { set; get; }

    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject connectMenu;
    [SerializeField] private GameObject serverMenu;

    [SerializeField] private TMP_InputField serverAddressInput;
    [SerializeField] private TMP_InputField playerNameInput;


    public GameObject serverPrefab;
    public GameObject clientPrefab;



    void Start()
    {
        Instance = this;
        serverMenu.SetActive(false);
        connectMenu.SetActive(false);

        DontDestroyOnLoad(gameObject);
    }

    public void ConnectButton()
    {
        Debug.Log("Connect");
        mainMenu.SetActive(false);
        connectMenu.SetActive(true);
    }
    public void HostButton()
    {
        Debug.Log("Host");

        try
        {
            Server s = Instantiate(serverPrefab).GetComponent<Server>();
            s.Init();

            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            if (c.clientName == null)
                c.clientName = "Host";
            c.ConnectToServer("127.0.0.1", 6321);
        }
        catch (Exception e)
        {
            Debug.Log("Server Init Error: " + e.Message);
        }

        mainMenu.SetActive(false);
        serverMenu.SetActive(true);
    }

    public void ConnectToServerButton()
    {
        Debug.Log("Connect to server");
        string hostAddress = serverAddressInput.text;
        if (hostAddress == "")
            hostAddress = "127.0.0.1";

        try
        {
            Client c = Instantiate(clientPrefab).GetComponent<Client>();
            c.clientName = playerNameInput.text;
            if (c.clientName == null)
                c.clientName = "Client";
            c.ConnectToServer(hostAddress, 6321);
            connectMenu.SetActive(false);
        }
        catch (Exception e)
        {
            Debug.Log("Error @ConnectToServerButton: " + e.Message);
        }

    }

    public void ReturnButton()
    {
        connectMenu.SetActive(false);
        serverMenu.SetActive(false);

        mainMenu.SetActive(true);
    }

}
