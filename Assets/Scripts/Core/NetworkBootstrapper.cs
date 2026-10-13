using FishNet;
using FishNet.Transporting;
using UnityEngine;

public static class NetworkBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        var prefab = Resources.Load<GameObject>("ClientBridge");

        if (prefab != null)
        {
            var bridge = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(bridge);
        }

        if (InstanceFinder.NetworkManager == null)
        {
            Debug.LogError("[NetworkBootstrapper] FishNet NetworkManager not found in scene. Add a NetworkManager object to your startup scene.");
            return;
        }

        if (InstanceFinder.ServerManager == null || InstanceFinder.ClientManager == null)
        {
            Debug.LogError("[NetworkBootstrapper] FishNet managers are missing.");
            return;
        }

        var args = System.Environment.GetCommandLineArgs();
        var mode = GetNetworkMode(args);

        Debug.Log($"[NetworkBootstrapper] Starting as {mode}");

        switch (mode)
        {
            case NetworkMode.Client:
                string targetAddress = GetTargetAddress(args);
                InstanceFinder.TransportManager.Transport.SetClientAddress(targetAddress);
                Debug.Log($"[NetworkBootstrapper] Connecting client to: {targetAddress}");

                InstanceFinder.ClientManager.OnClientConnectionState += OnClientStateChanged;
                InstanceFinder.ClientManager.StartConnection();
                break;

            case NetworkMode.Server:
                InstanceFinder.ServerManager.OnServerConnectionState += OnServerStateChanged;
                InstanceFinder.ServerManager.StartConnection();
                break;

            case NetworkMode.Host:
                InstanceFinder.ServerManager.OnServerConnectionState += OnServerStateChanged;
                InstanceFinder.ServerManager.StartConnection();

                InstanceFinder.ClientManager.OnClientConnectionState += OnClientStateChanged;
                InstanceFinder.ClientManager.StartConnection();
                break;
        }
    }

    private static string GetTargetAddress(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i].ToLower() == "-serveraddress")
            {
                return args[i + 1];
            }
        }
        return "127.0.0.1";
    }

    private static NetworkMode GetNetworkMode(string[] args)
    {
        foreach (var arg in args)
        {
            switch (arg.ToLower())
            {
                case "-server":
                    return NetworkMode.Server;

                case "-host":
                    return NetworkMode.Host;

                case "-client":
                    return NetworkMode.Client;
            }
        }

#if UNITY_EDITOR
        return NetworkMode.Host;
#else
        Debug.LogError(
            "[NetworkBootstrapper] No network mode specified. Use -client, -server, or -host."
        );

        Application.Quit();
        return NetworkMode.Client;
#endif
    }

    private static void OnServerStateChanged(ServerConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started) return;

        var prefab = Resources.Load<GameObject>("NetworkManager");
        var go = Object.Instantiate(prefab);
        InstanceFinder.ServerManager.Spawn(go);
    }

    private static void OnClientStateChanged(ClientConnectionStateArgs args)
    {
    }

    private enum NetworkMode
    {
        Client,
        Server,
        Host
    }
}