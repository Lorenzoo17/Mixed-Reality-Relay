using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using UnityEngine;
using System.Threading.Tasks;
using System.Text;

public class UDPClient : MonoBehaviour
{
    public const int PORT = 5000;
    public bool isClient;

    private Socket socket;
    private EndPoint endPoint;

    private byte[] buffer_recv;
    private ArraySegment<byte> buffer_recv_segment;

    private async void Start() {
        InitializeClient(IPAddress.Loopback, UDPServer.PORT);
        StartMessageLoop();

        await Send(Encoding.UTF8.GetBytes("Hello"));
    }

    private void InitializeClient(IPAddress address, int port) {
        buffer_recv = new byte[4096];

        buffer_recv_segment = new ArraySegment<byte>(buffer_recv);

        endPoint = new IPEndPoint(address, port);

        socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.PacketInformation, true);
    }

    private void StartMessageLoop() {
        _ = Task.Run(async () => {
            SocketReceiveMessageFromResult res;

            while (true) {
                res = await socket.ReceiveMessageFromAsync(buffer_recv_segment, SocketFlags.None, endPoint);

                Debug.Log("Received Message : " + Encoding.UTF8.GetString(buffer_recv, 0, res.ReceivedBytes));
            }
        });
    }

    private async Task Send(byte[] data) {
        var s = new ArraySegment<byte>(data);
        await socket.SendToAsync(s, SocketFlags.None, endPoint);
    }
}
