namespace PlaceableObjectManipulation
{
    [System.Serializable]
    public class StateCoordinator
    {
        public bool TryTakeFromStorage(PlaceableObject.PlaceableObject placeableObject)
        {
            return placeableObject && placeableObject.TryTakeFromStorage();
        }

        public bool TryPick(PlaceableObject.PlaceableObject placeableObject)
        {
            return placeableObject && placeableObject.TryPick();
        }

        public bool TryPlace(PlaceableObject.PlaceableObject placeableObject)
        {
            return placeableObject && placeableObject.TryPlace();
        }
    }
}
