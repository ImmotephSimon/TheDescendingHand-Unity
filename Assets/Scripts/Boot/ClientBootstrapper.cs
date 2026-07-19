using UnityEngine;
using FishNet;
using FishNet.Transporting;
using System.Linq;

public static class ClientBootstrapper
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
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
                InstanceFinder.ClientManager.StartConnection();
                break;

            case NetworkMode.Server:
                InstanceFinder.ServerManager.StartConnection();
                break;

            case NetworkMode.Host:
                InstanceFinder.ServerManager.StartConnection();
                InstanceFinder.ClientManager.StartConnection();
                break;
        }

        InstanceFinder.ClientManager.OnClientConnectionState += OnClientStateChanged;
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

    private static void OnClientStateChanged(ClientConnectionStateArgs args)
    {
        if (args.ConnectionState != LocalConnectionState.Started)
            return;

        var prefab = Resources.Load<GameObject>("ClientBridge");

        if (prefab != null)
        {
            var bridge = Object.Instantiate(prefab);
            Object.DontDestroyOnLoad(bridge);
        }
    }

    private enum NetworkMode
    {
        Client,
        Server,
        Host
    }
}