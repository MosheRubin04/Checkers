using System.IO;
using System.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{

    public string clientName;
    public bool isHost = false;
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

    private List<GameClient> players = new List<GameClient>();


    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (socketReady)
        {
            if (stream.DataAvailable)
            {
                string data = reader.ReadLine();
                if (data != null)
                    OnInComingData(data);
            }
        }
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnDisable()
    {
        CloseSocket();
    }

    public bool ConnectToServer(string host, int port)
    {
        if (socketReady)
        {
            return false;
        }
        Debug.Log("@ConnectToServer - Client " + clientName + " starting connecting");
        try
        {
            socket = new TcpClient(host, port);
            stream = socket.GetStream();
            writer = new StreamWriter(stream);
            reader = new StreamReader(stream);

            socketReady = true;
        }
        catch (Exception e)
        {

            Debug.Log("Socket Error " + e.Message);
        }
        return socketReady;
    }

    //? Send messages to  the server 
    public void Send(string data)
    {
        if (!socketReady)
            return;
        writer.WriteLine(data);
        writer.Flush();
    }

    //? Read messages from the server
    private void OnInComingData(string data)
    {
        Debug.Log("Client_" + clientName + " OnInComingData: " + data);
        string[] splitedData = data.Split('|');
        switch (splitedData[0])
        {
            case "S_WHO":
                for (int i = 1; i < splitedData.Length - 1; i++)
                {
                    UserConnected(splitedData[i], false);
                }
                Send("C_WHO|" + clientName + "|" + ((isHost) ? 1 : 0));
                break;
            case "S_CNN":
                UserConnected(splitedData[1], false);
                break;
            case "S_MOV":
                int x1 = int.Parse(splitedData[1]);
                int y1 = int.Parse(splitedData[2]);
                int x2 = int.Parse(splitedData[3]);
                int y2 = int.Parse(splitedData[4]);
                CheckersBoard.Instance.TryMove(x1, y1, x2, y2);
                break;
            default:
                Debug.Log("No condition for " + splitedData[0]);
                break;
        }

    }

    private void CloseSocket()
    {
        if (!socketReady)
            return;

        writer.Close();
        reader.Close();
        socket.Close();

        socketReady = false;
    }


    private void UserConnected(string name, bool host)
    {
        Debug.Log("@UserConnected " + name);
        if (name == "")
        {
            Debug.Log("@UserConnected - missing name data, return");
            return;

        }
        GameClient gameClint = new GameClient();
        gameClint.clientName = name;
        gameClint.isHost = host;
        players.Add(gameClint);

        Debug.Log("@UserConnected - players.Count" + players.Count);
        if (players.Count == 2)
            GameManager.Instance.StartGame();

    }
}

public class GameClient
{
    public string clientName;
    public bool isHost;
}
