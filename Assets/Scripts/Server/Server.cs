using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Server : MonoBehaviour
{
    public int port = 6321;

    private List<ServerClient> clients;
    private List<ServerClient> disconnectedList;
    private TcpListener server;
    private bool serverStarted;

    public void Init()
    {
        Debug.Log("Server Started on port " + port);

        DontDestroyOnLoad(gameObject);
        clients = new List<ServerClient>();
        disconnectedList = new List<ServerClient>();

        try
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();

            StartListening();
            serverStarted = true;
        }
        catch (Exception e)
        {
            Debug.Log("Socket Error: " + e.Message);
            throw;
        }

    }
    private void Update()
    {
        if (!serverStarted)
            return;
        foreach (ServerClient c in clients)
        {
            if (!IsConnected(c.tcp))
            {
                c.tcp.Close();
                disconnectedList.Add(c);
                continue;
            }
            else
            {
                NetworkStream s = c.tcp.GetStream();
                if (s.DataAvailable)
                {
                    StreamReader reader = new StreamReader(s, true);
                    string data = reader.ReadLine();
                    if (data != null)
                    {
                        OnComingData(c, data);
                    }
                }
            }
        }

        for (int i = 0; i < disconnectedList.Count - 1; i++)
        {
            //? Tell the player someone disconnected

            clients.Remove(disconnectedList[i]);
            disconnectedList.RemoveAt(i);
        }
    }
    //? Server Send
    private void Broadcast(string data, ServerClient c)
    {
        List<ServerClient> sl = new List<ServerClient> { c };
        Broadcast(data, sl);
    }
    private void Broadcast(string data, List<ServerClient> cl)
    {
        foreach (ServerClient sc in cl)
        {
            try
            {
                StreamWriter writer = new StreamWriter(sc.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.Log("Error @Broadcast - " + e.Message);
            }
        }
    }
    //? Server Read
    private void OnComingData(ServerClient c, string data)
    {
        Debug.Log("Server OnInComingData: " + data);
        string[] splitedData = data.Split('|');
        switch (splitedData[0])
        {
            case "C_WHO":
                c.clientName = splitedData[1];
                c.isHost = (splitedData[1] == "0") ? false : true;
                Broadcast("S_CNN|" + c.clientName, clients);
                break;
            case "C_MOV":
                Broadcast("S_MOV|" + splitedData[1] + "|" + splitedData[2] + "|" + splitedData[3] + "|" + splitedData[4], clients);
                break;
            default:
                Debug.Log("No condition for " + splitedData[0]);
                break;
        }
    }
    private void StartListening()
    {
        server.BeginAcceptTcpClient(AcceptTcpClient, server);
    }
    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        ServerClient sc = new ServerClient(listener.EndAcceptTcpClient(ar));
        clients.Add(sc);
        string allUsers = "";
        foreach (ServerClient client in clients)
        {
            allUsers += client.clientName + '|';
        }
        StartListening();
        Broadcast("S_WHO|" + allUsers, clients[clients.Count - 1]);
    }
    private bool IsConnected(TcpClient c)
    {
        try
        {
            if (c != null && c.Client != null && c.Client.Connected)
            {
                if (c.Client.Poll(0, SelectMode.SelectRead))
                    return !(c.Client.Receive(new byte[1], SocketFlags.Peek) == 0);
                return true;
            }
            else
                return false;

        }
        catch (Exception e)
        {
            Debug.Log("Error @IsConnected - " + e.Message);
            return false;
        }
    }
}

public class ServerClient
{
    public string clientName;
    public bool isHost = false;

    public TcpClient tcp;

    public ServerClient(TcpClient tcp)
    {
        this.tcp = tcp;
    }
}
