using GameBoard;
using InputController;
using PlaceableObjectManipulation;
using PlayerCamera;
using Reflex.Core;
using UI.FleetPanel;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private bool createNetworkBootstrapOnAwake = true;

    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private Selection selection;
    [SerializeField] private Moving moving;
    [SerializeField] private Spawning spawning;
    [SerializeField] private SpawnPositionResolver spawnPositionResolver;
    [SerializeField] private ShipRoster shipRoster;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraInputController cameraInputController;
    [SerializeField] private BattlefieldInputController battlefieldInputController;
    [SerializeField] private CursorPlaneInputController cursorPlaneInputController;

    private void Awake()
    {
        if (!createNetworkBootstrapOnAwake)
            return;
        if (FindFirstObjectByType<global::Network.NetworkBootstrap>())
            return;

        var networkBootstrapObject = new GameObject("NetworkBootstrap");
        networkBootstrapObject.AddComponent<global::Network.NetworkBootstrap>();
        DontDestroyOnLoad(networkBootstrapObject);
    }

    public void InstallBindings(ContainerBuilder builder)
    {
        builder.RegisterValue(cellGrid);
        builder.RegisterValue(cursorPlane);
        builder.RegisterValue(selection);
        builder.RegisterValue(moving);
        builder.RegisterValue(spawning);
        builder.RegisterValue(spawnPositionResolver);
        builder.RegisterValue(shipRoster);
        builder.RegisterValue(cameraMovement);
        builder.RegisterValue(cameraInputController);
        builder.RegisterValue(battlefieldInputController);
        builder.RegisterValue(cursorPlaneInputController);
    }
}
