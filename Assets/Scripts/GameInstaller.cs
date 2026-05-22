using System;
using GameBoard;
using InputController;
using Network;
using PlaceableObjectManipulation;
using PlayerCamera;
using Reflex.Core;
using UI.FleetPanel;
using UnityEngine;

public class GameInstaller : MonoBehaviour, IInstaller
{
    [SerializeField] private CellGrid cellGrid;
    [SerializeField] private CursorPlane cursorPlane;
    [SerializeField] private Selection selection;
    [SerializeField] private Moving moving;
    [SerializeField] private Spawning spawning;
    [SerializeField] private SpawnedObjectLifecycleTracker lifecycleTracker;
    [SerializeField] private FleetPanelController fleetPanelController;
    [SerializeField] private SpawnPositionResolver spawnPositionResolver = new();
    [SerializeField] private ShipRoster shipRoster;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private CameraInputController cameraInputController;
    [SerializeField] private BattlefieldInputController battlefieldInputController;
    [SerializeField] private CursorPlaneInputController cursorPlaneInputController;

    [SerializeField] private NetworkBootstrap networkBootstrap;
    [SerializeField] private NetworkRuntime networkRuntime;
    
    public void InstallBindings(ContainerBuilder builder)
    {
        RegisterRequired(builder, cellGrid, nameof(cellGrid));
        RegisterRequired(builder, cursorPlane, nameof(cursorPlane));
        RegisterRequired(builder, selection, nameof(selection));
        RegisterRequired(builder, moving, nameof(moving));
        RegisterRequired(builder, spawning, nameof(spawning));
        RegisterRequired(builder, lifecycleTracker, nameof(lifecycleTracker));
        RegisterRequired(builder, fleetPanelController, nameof(fleetPanelController));

        spawnPositionResolver ??= new SpawnPositionResolver();
        builder.RegisterValue(spawnPositionResolver);
        RegisterRequired(builder, shipRoster, nameof(shipRoster));
        RegisterRequired(builder, cameraMovement, nameof(cameraMovement));
        RegisterRequired(builder, cameraInputController, nameof(cameraInputController));
        RegisterRequired(builder, battlefieldInputController, nameof(battlefieldInputController));
        RegisterRequired(builder, cursorPlaneInputController, nameof(cursorPlaneInputController));

        if (networkBootstrap)
            builder.RegisterValue(networkBootstrap);
        if (networkRuntime)
            builder.RegisterValue(networkRuntime);
    }

    private static void RegisterRequired<T>(ContainerBuilder builder, T value, string fieldName)
        where T : UnityEngine.Object
    {
        if (!value)
            throw new InvalidOperationException($"GameInstaller dependency is not assigned: {fieldName} ({typeof(T).Name}).");

        builder.RegisterValue(value);
    }
}
