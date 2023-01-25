using System.Collections;
using GameLib.Random;
using UnityEngine;

public class DemoSelfAvoidingWalk : MonoBehaviour
{
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
        ImageGrid.Set(pointer.x, pointer.y, new ImageGrid.BaseCellValue { Scale = 0.65f, Color = Color.gray});
        yield return new WaitForSeconds(NextStepDelay);

        while (true)
        {
            // Get a random direction to move in
            Vector2Int moveDirection = _rnd.FromArray(_directions);

            // Check if the new position is valid (not out of bounds and not already occupied)
            if (IsValidMove(pointer + moveDirection))
            {
                // Update the current position
                prevPointer = pointer;
                pointer += moveDirection;

                // Set a block at the new position
                ImageGrid.Set(pointer.x, pointer.y, new ImageGrid.BaseCellValue { Scale = 0.65f });
                ImageGrid.Connect(pointer.x, pointer.y, prevPointer.x, prevPointer.y);
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
