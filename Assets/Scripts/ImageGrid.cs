using System;
using System.Collections.Generic;
using System.Linq;
using GameLib;
using GameLib.Random;
using UnityEngine;
using UnityEngine.Assertions;

public class ImageGrid : MonoBehaviour
{
    public class BaseCellValue
    {
        public enum PrimitiveType
        {
            Circle = 0,
            Hex = 1,
            Square = 2,
            Triangle = 3
        }

        public BaseCellValue()
        {
            Connections = new BaseCellValue[4];
            ConnectionInstances = new GameObject[4];
            Primitive = PrimitiveType.Square;
            Scale = 1.0f;
            Color = Color.white;
        }

        public PrimitiveType Primitive;
        public float Scale;
        public Color Color;

        public GameObject PrimitiveGameObject;
        public BaseCellValue[] Connections;
        public GameObject[] ConnectionInstances;

        public static int Dir2Index(Direction2D.RelativeDirection dir)
        {
            Assert.IsTrue(dir == Direction2D.RelativeDirection.Left || dir == Direction2D.RelativeDirection.Right || dir == Direction2D.RelativeDirection.Up || dir == Direction2D.RelativeDirection.Down);
            if (dir == Direction2D.RelativeDirection.Left)
                return 0;
            if (dir == Direction2D.RelativeDirection.Up)
                return 1;
            if (dir == Direction2D.RelativeDirection.Right)
                return 2;
            if (dir == Direction2D.RelativeDirection.Down)
                return 3;
            return -1;
        }
    }

    public SpriteRenderer SpriteRenderer;
    [Tooltip("Image grid size in cells")]
    public Vector2Int GridSize;
    public List<GameObject> blockPrefabs;
    public GameObject connectionPrefabHorizontal;
    public GameObject connectionPrefabVertical;
    public Camera Camera;
    public float CameraOffset;

    public int BlocksCounter { get; private set; }
    public BaseCellValue[,] Cells => _cells;

    private BaseCellValue[,] _cells;



    private void OnValidate()
    {
        if (GridSize.x < 0)
            GridSize.x = 0;
        if (GridSize.y < 0)
            GridSize.y = 0;
    }

    private void Awake()
    {
        AdjustSpriteSize();
        AdjustCameraSizeFitSpriteInScreen(SpriteRenderer, Camera);
        CreateModel();
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
        var prevVal = Get(x, y);
        if (prevVal != null)
        {
            Destroy(prevVal.PrimitiveGameObject);
            BlocksCounter--;
            BreakConnection(x, y, x - 1, y);
            BreakConnection(x, y, x, y + 1);
            BreakConnection(x, y, x + 1, y);
            BreakConnection(x, y, x, y - 1);
        }
        _cells[x, y] = cellValue;

        if (cellValue != null)
        {
            var block = Instantiate(blockPrefabs[(int)cellValue.Primitive], new Vector3(x + 0.5f, y + 0.5f, 0), Quaternion.identity);
            block.GetComponent<SpriteRenderer>().color = cellValue.Color;
            block.transform.localScale = Vector3.one * cellValue.Scale;
            cellValue.PrimitiveGameObject = block;
            block.name = $"{x}|{y}";
            block.transform.SetParent(gameObject.transform);
            BlocksCounter++;
            return block;
        }
        return null;
    }

    public BaseCellValue Get(int x, int y)
    {
        if (x < 0)
            return null;
        if (y < 0)
            return null;
        if (x >= GridSize.x)
            return null;
        if (y >= GridSize.y)
            return null;
        return _cells[x, y];
    }

    public bool Connect(int ax, int ay, int bx, int by, bool lerpColors = true)
    {
        var blockA = Get(ax, ay);
        var blockB = Get(bx, by);

        if (blockB == null || blockA == null) // Can't connect when first and second blocks are empty
            return false;

        Vector2Int offsetA2B = new Vector2Int(bx - ax, by - ay);
        var validConnection = (offsetA2B.x == 0 && offsetA2B.y == 1) || (offsetA2B.x == 0 && offsetA2B.y == -1)
            || (offsetA2B.x == -1 && offsetA2B.y == 0) || (offsetA2B.x == 1 && offsetA2B.y == 0); // No diagonal connections and non-neighbor connection allowed, as long as self-connection 
        if (!validConnection)
            return false;

        // Assign connections 
        var dirA2B = Direction2D.FromVector(offsetA2B);
        var dirB2A = Direction2D.Opposite(dirA2B);
        var connectionPrefab = Direction2D.IsHorizontal(dirB2A) ? connectionPrefabHorizontal : connectionPrefabVertical;
        BreakConnection(ax, ay, bx, by); // Break previous connections
        blockA.Connections[BaseCellValue.Dir2Index(dirA2B)] = blockB;
        blockB.Connections[BaseCellValue.Dir2Index(dirB2A)] = blockA;

        // Create visual instance
        var connection = Instantiate(connectionPrefab,
            new Vector3((ax + bx) * 0.5f + 0.5f, (ay + by) * 0.5f + 0.5f, 0),
            Quaternion.identity);
        connection.name = "Connection";
        connection.transform.SetParent(gameObject.transform);
        blockA.ConnectionInstances[BaseCellValue.Dir2Index(dirA2B)] = connection;
        blockB.ConnectionInstances[BaseCellValue.Dir2Index(dirB2A)] = connection;

        // Lerp color of connection visual instance based on 2 blocks colors
        if (lerpColors)
        {
            var mixedColor = Color.Lerp(blockA.Color, blockB.Color, 0.5f);
            connection.GetComponent<SpriteRenderer>().color = mixedColor;
        }

        return true;
    }

    public void BreakConnection(int ax, int ay, int bx, int by)
    {
        var blockA = Get(ax, ay);
        var blockB = Get(bx, by);
        if (blockA == null || blockB == null)
            return;

        Vector2Int offsetA2B = new Vector2Int(bx - ax, by - ay);
        Assert.IsTrue((offsetA2B.x == 0 && offsetA2B.y == 1) || (offsetA2B.x == 0 && offsetA2B.y == -1)
            || (offsetA2B.x == -1 && offsetA2B.y == 0) || (offsetA2B.x == 1 && offsetA2B.y == 0), "No diagonal connections and non-neighbor connection allowed");

        var dirA2B = Direction2D.FromVector(offsetA2B);
        var dirB2A = Direction2D.Opposite(dirA2B);
        blockA.Connections[BaseCellValue.Dir2Index(dirA2B)] = null;
        if (blockA.ConnectionInstances[BaseCellValue.Dir2Index(dirA2B)] != null)
            Destroy(blockA.ConnectionInstances[BaseCellValue.Dir2Index(dirA2B)]);
        blockA.ConnectionInstances[BaseCellValue.Dir2Index(dirA2B)] = null;
        blockB.Connections[BaseCellValue.Dir2Index(dirB2A)] = null;
        blockB.ConnectionInstances[BaseCellValue.Dir2Index(dirB2A)] = null;
    }

    void AdjustCameraSizeFitSpriteInScreen(SpriteRenderer sprite, Camera cam)
    {
        var spriteWorldHeight = sprite.size.y;
        var spriteWorldWidth = sprite.size.x;

        var screenAspectRatio = Screen.height / (float)Screen.width;
        var cameraHeight = spriteWorldHeight;
        var cameraWidth = spriteWorldHeight / screenAspectRatio;
        cam.orthographicSize = cameraHeight * 0.5f;
        cam.transform.position += Vector3.right * spriteWorldWidth * 0.5f + Vector3.left * CameraOffset;
        cam.transform.position += Vector3.up * cameraHeight * 0.5f;
    }

    private void AdjustSpriteSize()
    {
        SpriteRenderer.size = GridSize;
    }
}

static class ImageGridHelper
{
    public static void Fill(this ImageGrid imgGrid, ImageGrid.BaseCellValue fillValue)
    {
        for (int x = 0; x < imgGrid.GridSize.x; ++x)
            for (int y = 0; y < imgGrid.GridSize.y; ++y)
                imgGrid.Set(x, y, fillValue);
    }

    public static void FillRndColor(this ImageGrid imgGrid, IPseudoRandomNumberGenerator rnd)
    {
        for (int x = 0; x < imgGrid.GridSize.x; ++x)
            for (int y = 0; y < imgGrid.GridSize.y; ++y)
                imgGrid.Set(x, y, new ImageGrid.BaseCellValue { Color = rnd.ColorHSV() });
    }


    public static void FillWithObstacles(this ImageGrid imgGrid, float density, IPseudoRandomNumberGenerator rnd)
    {
        for (int x = 0; x < imgGrid.GridSize.x; ++x)
            for (int y = 0; y < imgGrid.GridSize.y; ++y)
                if (rnd.ValueFloat() < density)
                    imgGrid.Set(x, y, new ImageGrid.BaseCellValue { Color = Color.black, Primitive = ImageGrid.BaseCellValue.PrimitiveType.Hex });
    }

    public static bool HasAnyConnection(this ImageGrid imgGrid, int x, int y)
    {
        var cellVal = imgGrid.Get(x, y);
        if (cellVal == null)
            return false;
        foreach (var connection in cellVal.Connections)
            if (connection != null)
                return true;
        return false;
    }

    public static bool IsInsideGrid(this ImageGrid imgGrid, int x, int y)
    {
        // Check if the new position is within the grid bounds
        return (x >= 0 && x < imgGrid.GridSize.x && y >= 0 && y < imgGrid.GridSize.y);
    }

    public static bool IsEmptyCell(this ImageGrid imgGrid, int x, int y)
    {
        if (!imgGrid.IsInsideGrid(x, y))
            return false;
        return imgGrid.Get(x, y) == null;
    }


    public static List<(ImageGrid.BaseCellValue, Vector2Int)> GetAll(this ImageGrid imgGrid, Func<ImageGrid.BaseCellValue, bool> Condition)
    {
        List<(ImageGrid.BaseCellValue, Vector2Int)> result = new List<(ImageGrid.BaseCellValue, Vector2Int)>();
        for (int x = 0; x < imgGrid.GridSize.x; ++x)
            for (int y = 0; y < imgGrid.GridSize.y; ++y)
            {
                var val = imgGrid.Get(x, y);
                if (Condition(val))
                    result.Add((val, new Vector2Int(x, y)));
            }

        return result;
    }

    public static List<Vector2Int> GetEmptyCells(this ImageGrid imgGrid)
    {
        return Enumerable.Range(0, imgGrid.Cells.GetLength(0))
            .SelectMany(x => Enumerable.Range(0, imgGrid.Cells.GetLength(1))
                .Where(y => imgGrid.Cells[x, y] != null)
                .Select(y => new Vector2Int(x, y))).ToList();
    }

    // Get all orthogonal neighbors if they are:
    // - Inside the grid
    // - Meet PickupCondition if it's passed
    public static List<(ImageGrid.BaseCellValue, Vector2Int)> GetNeigbours(this ImageGrid imgGrid, int x, int y, bool isReturnEmptyNeighbour = false, Func<ImageGrid.BaseCellValue, bool> PickupCondition = null)
    {
        List<(ImageGrid.BaseCellValue, Vector2Int)> neigbours = new List<(ImageGrid.BaseCellValue, Vector2Int)>(4);
        foreach (var offset in Direction2D.OrthogonalDirections)
        {
            var checkPos = new Vector2Int(x + offset.x, y + offset.y);
            if (!imgGrid.IsInsideGrid(checkPos.x, checkPos.y))
                continue;
            var cell = imgGrid.Get(checkPos.x, checkPos.y);
            if (cell == null)
            {
                if (isReturnEmptyNeighbour)
                    neigbours.Add((cell, checkPos));
                continue;
            }

            if (PickupCondition != null)
            {
                if (PickupCondition(cell))
                    neigbours.Add((cell, checkPos));
            }
            else
            {
                neigbours.Add((cell, checkPos));
            }
        }
        return neigbours;
    }
}
