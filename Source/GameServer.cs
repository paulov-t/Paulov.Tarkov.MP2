using BepInEx.Logging;
using LiteNetLib;
using LiteNetLib.Utils;
using Open.Nat;
using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

namespace Paulov.Tarkov.MP2;

public class GameServer : MonoBehaviour, LiteNetLib.INetEventListener, LiteNetLib.INetLogger
{
    private LiteNetLib.NetManager _netServer;
    private LiteNetLib.NetPeer _ourPeer;
    private LiteNetLib.Utils.NetDataWriter _dataWriter;
    private ManualLogSource _logger;

    public event EventHandler ServerStarted;


    private void Start()
    {
        _logger = BepInEx.Logging.Logger.CreateLogSource(nameof(GameServer));

        _logger.LogDebug("[SERVER] Start");
        NetDebug.Logger = this;
        _dataWriter = new NetDataWriter();
        _netServer = new LiteNetLib.NetManager(this);
        new NatDiscoverer().DiscoverDeviceAsync().ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                var nat = task.Result;
                _logger.LogDebug("[SERVER] NAT device discovered: " + nat);
                nat.GetExternalIPAsync().ContinueWith(ipTask =>
                {
                    if (ipTask.IsCompletedSuccessfully)
                    {
                        _logger.LogDebug("[SERVER] External IP: " + ipTask.Result);
                    }
                    else
                    {
                        _logger.LogError("[SERVER] Failed to get external IP: " + ipTask.Exception);
                    }
                });

                _logger.LogDebug("[SERVER] Port mapping created successfully.");
                _netServer.Start("127.0.0.1", "::", 17000);
                _netServer.BroadcastReceiveEnabled = true;
                _netServer.UpdateTime = 15;

                if (ServerStarted != null)
                    ServerStarted.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _logger.LogError("[SERVER] Failed to create port mapping: " + task.Exception);
            }
        });

    }

    private void Update()
    {
        _netServer.PollEvents();
    }

    private void FixedUpdate()
    {
        if (_ourPeer != null)
        {
            _dataWriter.Reset();
            _ourPeer.Send(_dataWriter, DeliveryMethod.Sequenced);
        }
    }

    private void OnDestroy()
    {
        NetDebug.Logger = null;
        if (_netServer != null)
            _netServer.Stop();
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
    {
        _logger.LogDebug("[SERVER] We have new peer " + peer);
        _ourPeer = peer;
    }

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        _logger.LogDebug("[SERVER] error " + socketErrorCode);
    }

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader,
        UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.Broadcast)
        {
            _logger.LogDebug("[SERVER] Received discovery request. Send discovery response");
            NetDataWriter resp = new NetDataWriter();
            resp.Put(1);
            _netServer.SendUnconnectedMessage(resp, remoteEndPoint);
        }
    }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
    }

    void LiteNetLib.INetEventListener.OnConnectionRequest(LiteNetLib.ConnectionRequest request)
    {
        request.AcceptIfKey("sample_app");
    }

    void LiteNetLib.INetEventListener.OnPeerDisconnected(LiteNetLib.NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
    {
        _logger.LogDebug("[SERVER] peer disconnected " + peer + ", info: " + disconnectInfo.Reason);
        if (peer == _ourPeer)
            _ourPeer = null;
    }

    void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
    }

    void INetLogger.WriteNet(NetLogLevel level, string str, params object[] args)
    {
        Debug.LogFormat(str, args);
    }

    public void OnPeerDisconnected(NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
    {
        throw new NotImplementedException();
    }

    public void OnConnectionRequest(LiteNetLib.ConnectionRequest request)
    {
        throw new NotImplementedException();
    }
}