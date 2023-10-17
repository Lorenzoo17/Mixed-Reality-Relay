using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

public class UDPServer : MonoBehaviour
{
    public bool isServer;
    public const int PORT = 5000;

    private Socket socket;
    private EndPoint endPoint;

    private byte[] buffer_recv;
    private ArraySegment<byte> buffer_recv_segment;

    private void Start() {
        InitializeServer();
        StartMessageListening();
    }

    private void InitializeServer() {
        buffer_recv = new byte[4096];

        buffer_recv_segment = new ArraySegment<byte>(buffer_recv);

        endPoint = new IPEndPoint(IPAddress.Any, PORT);

        socket = new(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        socket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, true);
        socket.Bind(endPoint);
    }

    private void StartMessageListening() {
        _ = Task.Run(async () => {
            SocketReceiveMessageFromResult res;

            while (true) {
                res = await socket.ReceiveMessageFromAsync(buffer_recv_segment, SocketFlags.None, endPoint);

                Debug.Log("Received Message : " + Encoding.UTF8.GetString(buffer_recv, 0, res.ReceivedBytes));
                await SendTo(res.RemoteEndPoint, Encoding.UTF8.GetBytes("Hello back"));
            }
        });
    }

    private async Task SendTo(EndPoint recipient, byte[] data) {
        var s = new ArraySegment<byte>(data);
        await socket.SendToAsync(s, SocketFlags.None, recipient);
    }
}
