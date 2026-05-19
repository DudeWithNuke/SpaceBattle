using GameBoard;
using Reflex.Extensions;
using Reflex.Injectors;
using Reflex.Attributes;
using UnityEngine;

namespace PlaceableObjectManipulation
{
    public class Spawning : MonoBehaviour
    {
        [Inject] private SpawnPositionResolver _spawnPositionResolver;
        [Inject] private CursorPlane _cursorPlane;
        [Inject] private CellGrid _cellGrid;
        [SerializeField] private Camera targetCamera;

        public PlaceableObject.PlaceableObject Spawn(PlaceableObject.PlaceableObject prefab)
        {
            if (!prefab)
                return null;

            var spawnPosition = _spawnPositionResolver.ResolveSpawnPosition(_cursorPlane, targetCamera);
            return Spawn(prefab, spawnPosition);
        }

        public PlaceableObject.PlaceableObject SpawnAtCell(PlaceableObject.PlaceableObject prefab, Vector3Int cellPosition, bool isPlayerObject)
        {
            if (!prefab)
                return null;

            var spawnPosition = CoordinateUtility.CellToWorldPosition(_cellGrid, isPlayerObject, cellPosition, 0.5f);
            var instance = Spawn(prefab, spawnPosition);
            if (instance)
                instance.SetLocalPlayerObject(isPlayerObject);
            return instance;
        }

        private PlaceableObject.PlaceableObject Spawn(PlaceableObject.PlaceableObject prefab, Vector3 spawnPosition)
        {
            var instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (!instance)
                return null;

            var sceneContainer = gameObject.scene.GetSceneContainer();
            GameObjectInjector.InjectObject(instance.gameObject, sceneContainer);
            return instance;
        }
    }
}
