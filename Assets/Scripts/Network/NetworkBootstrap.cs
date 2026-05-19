using System.Threading;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Network
{
    public sealed class NetworkBootstrap : MonoBehaviour
    {
        [SerializeField] private string defaultAddress = "127.0.0.1";
        [SerializeField] private ushort port = 7777;
        [SerializeField] private bool autoConnectOnStart = true;

        private NetworkManager _networkManager;
        private Mutex _hostRoleMutex;
        private bool _ownsHostRole;

        private void Awake()
        {
            _networkManager = EnsureNetworkManager();
        }

        private void Start()
        {
            if (!autoConnectOnStart)
                return;
            if (_networkManager && _networkManager.IsListening)
                return;

            AutoConnectLocal();
        }

        private void OnEnable()
        {
            if (!_networkManager)
                return;

            _networkManager.OnClientConnectedCallback += HandleClientConnected;
            _networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
        }

        private void OnDisable()
        {
            if (!_networkManager)
                return;

            _networkManager.OnClientConnectedCallback -= HandleClientConnected;
            _networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
        }

        private void OnDestroy()
        {
            ReleaseHostRole();
        }

        private void OnApplicationQuit()
        {
            ReleaseHostRole();
        }

        public bool StartHost()
        {
            if (!PrepareNetwork(defaultAddress))
                return false;

            var started = _networkManager.StartHost();
            if (started)
                InitializeMatchController();

            Debug.Log(started
                ? "[NetworkBootstrap] Host started."
                : "[NetworkBootstrap] Failed to start host.");
            return started;
        }

        public bool StartClient(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                address = defaultAddress;

            if (!PrepareNetwork(address))
                return false;

            var started = _networkManager.StartClient();
            if (started)
                InitializeMatchController();

            Debug.Log(started
                ? $"[NetworkBootstrap] Client connecting to {address}:{port}."
                : "[NetworkBootstrap] Failed to start client.");
            return started;
        }

        public void Disconnect()
        {
            if (!_networkManager || !_networkManager.IsListening)
                return;

            _networkManager.Shutdown();
            Debug.Log("[NetworkBootstrap] Disconnected.");
        }

        private void AutoConnectLocal()
        {
            if (TryAcquireHostRole())
            {
                Debug.Log("[NetworkBootstrap] Auto-connect selected Host mode.");
                if (StartHost())
                    return;

                ReleaseHostRole();
            }

            Debug.Log("[NetworkBootstrap] Auto-connect selected Client mode.");
            StartClient(defaultAddress);
        }

        private bool TryAcquireHostRole()
        {
            if (_ownsHostRole)
                return true;

            _hostRoleMutex = new Mutex(false, $"SpaceBattle_NetworkHost_{port}");

            try
            {
                _ownsHostRole = _hostRoleMutex.WaitOne(0);
            }
            catch (AbandonedMutexException)
            {
                _ownsHostRole = true;
            }

            if (!_ownsHostRole)
            {
                _hostRoleMutex.Dispose();
                _hostRoleMutex = null;
            }

            return _ownsHostRole;
        }

        private void ReleaseHostRole()
        {
            if (_hostRoleMutex == null)
                return;

            if (_ownsHostRole)
                _hostRoleMutex.ReleaseMutex();

            _ownsHostRole = false;
            _hostRoleMutex.Dispose();
            _hostRoleMutex = null;
        }

        private bool PrepareNetwork(string address)
        {
            _networkManager = EnsureNetworkManager();
            if (!_networkManager)
                return false;

            var transport = _networkManager.GetComponent<UnityTransport>();
            if (!transport)
            {
                Debug.LogError("[NetworkBootstrap] UnityTransport is missing.");
                return false;
            }

            transport.SetConnectionData(address, port);
            return true;
        }

        private void InitializeMatchController()
        {
            var matchController = _networkManager.GetComponent<NetworkMatchController>();
            matchController?.InitializeAfterNetworkStart();
        }

        private NetworkManager EnsureNetworkManager()
        {
            if (NetworkManager.Singleton)
            {
                EnsureMatchController(NetworkManager.Singleton.gameObject);
                return NetworkManager.Singleton;
            }

            var networkObject = new GameObject("NetworkManager");
            var transport = networkObject.AddComponent<UnityTransport>();
            var manager = networkObject.AddComponent<NetworkManager>();
            manager.NetworkConfig = new NetworkConfig
            {
                NetworkTransport = transport,
                ProtocolVersion = 1,
                ConnectionApproval = false
            };

            EnsureMatchController(networkObject);
            DontDestroyOnLoad(networkObject);
            return manager;
        }

        private static void EnsureMatchController(GameObject networkObject)
        {
            if (!networkObject.GetComponent<NetworkPlayerContext>())
                networkObject.AddComponent<NetworkPlayerContext>();

            if (!networkObject.GetComponent<NetworkPrefabRegistry>())
                networkObject.AddComponent<NetworkPrefabRegistry>();

            if (!networkObject.GetComponent<NetworkMatchController>())
                networkObject.AddComponent<NetworkMatchController>();

            if (!networkObject.GetComponent<NetworkPlacementController>())
                networkObject.AddComponent<NetworkPlacementController>();

            if (!networkObject.GetComponent<NetworkTurnInputGate>())
                networkObject.AddComponent<NetworkTurnInputGate>();

            if (!networkObject.GetComponent<NetworkTurnSubmitController>())
                networkObject.AddComponent<NetworkTurnSubmitController>();
        }

        private static void HandleClientConnected(ulong clientId)
        {
            Debug.Log($"[NetworkBootstrap] Client connected: {clientId}.");
        }

        private static void HandleClientDisconnected(ulong clientId)
        {
            Debug.Log($"[NetworkBootstrap] Client disconnected: {clientId}.");
        }
    }
}
