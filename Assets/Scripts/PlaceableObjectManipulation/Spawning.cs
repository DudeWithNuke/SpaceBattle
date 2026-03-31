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
        [SerializeField] private Camera targetCamera;

        public PlaceableObject.PlaceableObject Spawn(PlaceableObject.PlaceableObject prefab)
        {
            if (!prefab)
                return null;

            var spawnPosition = _spawnPositionResolver != null
                ? _spawnPositionResolver.ResolveSpawnPosition(_cursorPlane, targetCamera)
                : Vector3.zero;

            var instance = Instantiate(prefab, spawnPosition, Quaternion.identity);
            if (!instance)
                return null;

            var sceneContainer = gameObject.scene.GetSceneContainer();
            GameObjectInjector.InjectObject(instance.gameObject, sceneContainer);
            return instance;
        }
    }
}
