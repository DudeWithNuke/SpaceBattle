using UnityEngine;
namespace GameBoard
{
    public class CursorPlane : CustomMonoBehaviour<CursorPlane>
    {
        private const KeyCode UpButton = KeyCode.UpArrow; 
        private const KeyCode DownButton = KeyCode.DownArrow;

        public Plane Plane { get; private set; }

        public int layersCount;
        public int currentLayer;

        private void Awake()
        {
            Subscribe<CellGrid>(OnCellGridInitialized);
        }

        private void OnCellGridInitialized(CellGrid cellGrid)
        {
            layersCount = cellGrid.gridSize.y;
            currentLayer = layersCount / 2;
        }
        
        protected override void SetUp()
        {
            gameObject.transform.position = GetActualPosition();
            Plane = new Plane(Vector3.up, GetActualPosition());
            Refresh();
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(UpButton))
                Up();
            else if (Input.GetKeyDown(DownButton))
                Down();
        }

        private void Up()
        {
            if (currentLayer >= layersCount - 1)
                return;

            currentLayer++;
            Refresh();
        }

        private void Down()
        {
            if (currentLayer <= 0)
                return;

            currentLayer--;
            Refresh();
        }

        private void Refresh()
        {
            gameObject.transform.position = GetActualPosition();
            
            var newPlane = Plane;
            newPlane.SetNormalAndPosition(Vector3.up, GetActualPosition());
            Plane = newPlane;
        }

        private Vector3 GetActualPosition()
        {
            return new Vector3(0, currentLayer, 0);
        }
    }
}