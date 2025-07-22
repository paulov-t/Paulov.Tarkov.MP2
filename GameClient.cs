using Comfort.Common;
using EFT;
using LiteNetLib;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Paulov.Tarkov.MP2;
public class GameClient : MonoBehaviour, INetEventListener
{
    public LiteNetLib.NetManager NetClient;

    public HashSet<EFT.Profile> LoadProfiles = new HashSet<EFT.Profile>();
    public HashSet<string> LoadingProfiles = new HashSet<string>();
    public ConcurrentBag<string> LoadedProfiles = new ConcurrentBag<string>();

    private void Start()
    {
        NetClient = new LiteNetLib.NetManager(this);
        NetClient.UnconnectedMessagesEnabled = true;
        NetClient.UpdateTime = 15;
    }

    private void Update()
    {
        NetClient.PollEvents();

        var peer = NetClient.FirstPeer;
        if (peer != null && peer.ConnectionState == ConnectionState.Connected)
        {
        }
        else
        {
            NetClient.SendBroadcast(new byte[] { 1 }, 17000);
        }


        if (LoadProfiles.Any())
        {
            foreach (var profile in LoadProfiles)
            {
                if (!LoadingProfiles.Contains(profile.Id) && !LoadedProfiles.Contains(profile.Id))
                {
                    LoadingProfiles.Add(profile.Id);
                    Plugin.Logger.LogDebug($"[CLIENT] Loading profile: {profile.Id}");
                    var taskSch = TaskScheduler.FromCurrentSynchronizationContext();
                    var taskLoadProfileBundles = new BundleLoader(taskSch);
                    taskLoadProfileBundles.LoadBundles(profile)
                        .ContinueWith((x) => { LoadedProfiles.Add(x.Result.Id); });
                }
            }
            LoadProfiles.Clear();
        }
    }

    private void OnDestroy()
    {
        if (NetClient != null)
            NetClient.Stop();
    }

    void INetEventListener.OnPeerConnected(NetPeer peer)
    {
        Plugin.Logger.LogDebug("[CLIENT] We connected to " + peer);
    }

    void INetEventListener.OnNetworkError(IPEndPoint endPoint, SocketError socketErrorCode)
    {
        Plugin.Logger.LogDebug("[CLIENT] We received error " + socketErrorCode);
    }

    void INetEventListener.OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
    }

    void INetEventListener.OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        if (messageType == UnconnectedMessageType.BasicMessage && NetClient.ConnectedPeersCount == 0 && reader.GetInt() == 1)
        {
            Plugin.Logger.LogDebug("[CLIENT] Received discovery response. Connecting to: " + remoteEndPoint);
            NetClient.Connect(remoteEndPoint, "sample_app");
        }
    }

    void INetEventListener.OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    void LiteNetLib.INetEventListener.OnConnectionRequest(LiteNetLib.ConnectionRequest request)
    {

    }

    void LiteNetLib.INetEventListener.OnPeerDisconnected(LiteNetLib.NetPeer peer, LiteNetLib.DisconnectInfo disconnectInfo)
    {
        Plugin.Logger.LogDebug("[CLIENT] We disconnected because " + disconnectInfo.Reason);
    }

    public void ConnectToIpAndPortAndStart(string ip, int port)
    {
        Plugin.Logger.LogDebug($"[CLIENT] [ConnectToIpAndPortAndStart]. Attempting to connect to {ip}:{port}");

        if (gameObject.GetComponent<GameServer>() != null)
        {
            gameObject.GetComponent<GameServer>().ServerStarted += (sender, args) =>
            {
                Plugin.Logger.LogDebug("[CLIENT] Server started, connecting to it.");
                NetClient.Start();
                NetClient.Connect(ip, port, "sample_app");
            };
        }
        else
        {
            NetClient.Start();
            NetClient.Connect(ip, port, "sample_app");
        }
    }
}


public struct BundleLoader
{
    private Profile _profile;
    TaskScheduler TaskScheduler { get; }

    public BundleLoader(TaskScheduler taskScheduler)
    {
        _profile = null;
        TaskScheduler = taskScheduler;
    }

    public Task<Profile> LoadBundles(Task<Profile> task)
    {
        _profile = task.Result;

        Plugin.Logger.LogDebug($"{nameof(BundleLoader)} Loading Bundles.");

        var loadTask = Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(
            PoolManagerClass.PoolsCategory.Raid,
            PoolManagerClass.AssemblyType.Local,
            _profile.GetAllPrefabPaths(true).Where(x => !x.IsNullOrEmpty()).ToArray(),
            JobPriority.General,
            null,
            default);

        return loadTask.ContinueWith(GetProfile, TaskScheduler);
    }

    public async Task<Profile> LoadBundles(Profile _profile)
    {
        Plugin.Logger.LogDebug($"{nameof(BundleLoader)} Loading Bundles.");

        await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(
            PoolManagerClass.PoolsCategory.Raid,
            PoolManagerClass.AssemblyType.Local,
            _profile.GetAllPrefabPaths(true).Where(x => !x.IsNullOrEmpty()).ToArray(),
            JobPriority.General,
            null,
            default);

        return _profile;

    }

    private Profile GetProfile(Task task)
    {
        return _profile;
    }
}