using System.Collections.Generic;
using UnityEngine;

public class ImageGrid : MonoBehaviour
{
    public class BaseCellValue
    {
        public enum PrimitiveType
        {
            Square,
            Circle
        }

        public BaseCellValue()
        {
            Primitive = PrimitiveType.Square;
            Scale = 1.0f;
        }



        public PrimitiveType Primitive;
        public GameObject PrimitiveGameObject;
        public float Scale;
        public Color Color;
        public List<BaseCellValue> Connections;
    }
    public SpriteRenderer SpriteRenderer;
    [Tooltip("Image grid size in cells")]
    public Vector2Int GridSize;
    public GameObject blockPrefab;
    public Camera Camera;
    private BaseCellValue[,] _cells;


    private void OnValidate()
    {
        if (GridSize.x < 0)
            GridSize.x = 0;
        if (GridSize.y < 0)
            GridSize.y = 0;
    }

    private void Start()
    {
        AdjustSpriteSize();
        AdjustCameraSizeFitSpriteInScreen(SpriteRenderer, Camera);
        CreateModel();
        TestFillAllCells();
        //TestFillRandom();
    }

    private void CreateModel()
    {
        _cells = new BaseCellValue[GridSize.x, GridSize.y];
    }

    public GameObject Set(int x, int y, BaseCellValue cellValue)
    {
        // Check range of cell coordinates
        if (x < 0 || y < 0 || x >= GridSize.x || y >= GridSize.y)
        {
            Debug.LogError($"{x}|{y} is out of GridSize range {GridSize}");
            return null;
        }

        // Delete previous value
        _cells[x, y] = cellValue;

        if (cellValue != null)
        {
            var block = Instantiate(blockPrefab, new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
            block.GetComponent<SpriteRenderer>().color = cellValue.Color;
            block.transform.localScale = Vector3.one * cellValue.Scale;
            cellValue.PrimitiveGameObject = block;
            return block;
        }
        return null;
    }

    private void TestFillAllCells()
    {
        for (int x = 0; x < GridSize.x; ++x)
            for (int y = 0; y < GridSize.y; ++y)
                Set(x, y, new BaseCellValue { Color = Random.ColorHSV(), Scale = Random.value});
    }

    private void TestFillRandom()
    {
        for (int x = 0; x < GridSize.x; ++x)
            for (int y = 0; y < GridSize.y; ++y)
            {
                if (Random.value < 0.25f)
                    Set(x, y, new BaseCellValue{Color = Random.ColorHSV()});
            }
    }

    void AdjustCameraSizeFitSpriteInScreen(SpriteRenderer sprite, Camera cam)
    {
        var spriteWorldHeight = sprite.size.y;
        var spriteWorldWidth = sprite.size.x;

        var screenAspectRatio = Screen.height / (float)Screen.width;
        var cameraHeight = spriteWorldHeight;
        var cameraWidth = spriteWorldHeight / screenAspectRatio;
        cam.orthographicSize = cameraHeight * 0.5f;
        cam.transform.position += Vector3.right * spriteWorldWidth * 0.5f;
        cam.transform.position += Vector3.up * cameraHeight * 0.5f;
    }


    private void AdjustSpriteSize()
    {
        SpriteRenderer.size = GridSize;
    }
}
