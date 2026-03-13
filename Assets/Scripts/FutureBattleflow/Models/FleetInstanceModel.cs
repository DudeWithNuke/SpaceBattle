using System;
using System.Collections.Generic;
using UnityEngine;

namespace FutureBattleflow.Models
{
    [Serializable]
    public class FleetShipInstance
    {
        [SerializeField] private string instanceId;
        [SerializeField] private LoadoutShipEntry sourceShip;
        [SerializeField] private bool isDeployed;
        [SerializeField] private Vector3Int deployedCell;

        public string InstanceId => instanceId;
        public LoadoutShipEntry SourceShip => sourceShip;
        public bool IsDeployed => isDeployed;
        public Vector3Int DeployedCell => deployedCell;

        public FleetShipInstance(string newInstanceId, LoadoutShipEntry newSourceShip)
        {
            instanceId = newInstanceId;
            sourceShip = newSourceShip;
            isDeployed = false;
            deployedCell = Vector3Int.zero;
        }

        public void MarkDeployed(Vector3Int cell)
        {
            isDeployed = true;
            deployedCell = cell;
        }
    }

    [Serializable]
    public class FleetInstanceModel
    {
        [SerializeField] private List<FleetShipInstance> ships = new();

        public IReadOnlyList<FleetShipInstance> Ships => ships;

        public void Clear()
        {
            ships.Clear();
        }

        public void Add(FleetShipInstance shipInstance)
        {
            if (shipInstance == null)
                return;

            ships.Add(shipInstance);
        }
    }
}
