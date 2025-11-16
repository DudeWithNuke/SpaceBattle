using GameBoard;

namespace InteractionMode
{
    public class InteractionModeManager : CustomMonoBehaviour<InteractionModeManager>
    {
        private CursorPlane _cursorPlane;

        private void Awake()
        {
            SubscribeOnInitialize<CursorPlane>(cursorPlane => _cursorPlane = cursorPlane);
        }
        
        protected override void SetUp()
        {
            throw new System.NotImplementedException();
        }
    }
}