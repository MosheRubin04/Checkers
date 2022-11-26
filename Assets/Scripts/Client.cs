using System.IO;
using System.Net.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Client : MonoBehaviour
{
    private bool socketReady;
    private TcpClient socket;
    private NetworkStream stream;
    private StreamWriter writer;
    private StreamReader reader;

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
        Debug.Log("OnInComingData: " + data);
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

}

public class GameClient
{
    public string name;
    public bool isHost;
}
