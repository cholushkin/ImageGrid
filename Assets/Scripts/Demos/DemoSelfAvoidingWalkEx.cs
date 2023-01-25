using System.Collections;
using System.Collections.Generic;
using GameLib.Random;
using UnityEngine;

public class DemoSelfAvoidingWalkEx : MonoBehaviour
{
    public class CellValue : ImageGrid.BaseCellValue
    {
        public bool[] Directions = { false, false, false, false };
    }

    public ImageGrid ImageGrid;
    [Range(0f, 1f)]
    public float NextStepDelay;
    public float ShowBoardDelay;
    public long Seed;

    private IPseudoRandomNumberGenerator _rnd;
    private static readonly Vector2Int[] _directions = new Vector2Int[4]
    {
        new Vector2Int(-1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1)
    };

    private void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        StartCoroutine(Walk());
    }

    private IEnumerator Walk()
    {
        yield return new WaitForSeconds(ShowBoardDelay);

        // Set the initial position to the random position
        Vector2Int prevPointer = Vector2Int.zero;
        Vector2Int pointer = new Vector2Int(
            _rnd.Range(0, ImageGrid.GridSize.x),
            _rnd.Range(0, ImageGrid.GridSize.y));

        // Set a block at the initial position
        ImageGrid.Set(pointer.x, pointer.y, new CellValue { Scale = 0.65f, Color = Color.gray });
        yield return new WaitForSeconds(NextStepDelay);

        while (true)
        {
            // Get a random direction to a list of available directions
            List<int> availableDirections = new List<int>(4);
            for (int i = 0; i < 4; ++i)
                if (IsValidMove(pointer + _directions[i]) && (ImageGrid.Get(pointer.x, pointer.y) as CellValue).Directions[i]==false)
                    availableDirections.Add(i);

            // Return one step back
            if (availableDirections.Count == 0)
            {
                // Get connected block coordinates
                var connections = ImageGrid.Get(pointer.x, pointer.y).Connections;
                Vector2Int offset = Vector2Int.zero;
                for (int i = 0; i < connections.Length; ++i)
                    if (connections[i] != null)
                    {
                        offset = _directions[i];
                        break;
                    }

                // Delete self
                ImageGrid.Set(pointer.x, pointer.y, null);

                // Assign pointer to prev
                pointer += offset;
            }
            // Move to direction
            else
            {
                // Choose one of available direction and mark it as used
                var dirIndex = _rnd.FromList(availableDirections);
                var moveDirection = _directions[dirIndex];
                (ImageGrid.Get(pointer.x, pointer.y) as CellValue).Directions[dirIndex] = true;

                prevPointer = pointer;
                pointer += moveDirection;

                // Set a block at the new position
                var newCellVal = new CellValue { Scale = 0.65f };
                ImageGrid.Set(pointer.x, pointer.y, newCellVal);
                ImageGrid.Connect(pointer.x, pointer.y, prevPointer.x, prevPointer.y);

                // Check finish condition
                if (ImageGrid.BlocksCounter == ImageGrid.GridSize.x * ImageGrid.GridSize.y - 1)
                {
                    // Find empty cells
                    for (int x = 0; x < ImageGrid.GridSize.x; ++x)
                        for (int y = 0; y < ImageGrid.GridSize.y; ++y)
                        {
                            if (ImageGrid.Get(x, y) == null)
                            {
                                var finalCell = ImageGrid.Set(x, y, new CellValue {Scale = 0.65f});
                                if (!ImageGrid.Connect(x, y, pointer.x, pointer.y))
                                    ImageGrid.Set(x, y, null);
                            }
                        }
                        
                    break;
                }
            }

            

            yield return new WaitForSeconds(NextStepDelay);
        }
    }

    private bool IsValidMove(Vector2Int move)
    {
        // Check if the new position is within the grid bounds
        if (move.x < 0 || move.x >= ImageGrid.GridSize.x || move.y < 0 || move.y >= ImageGrid.GridSize.y)
            return false;
        return ImageGrid.Get(move.x, move.y) == null;
    }
}
