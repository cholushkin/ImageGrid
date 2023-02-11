using System.Collections;
using GameLib;
using GameLib.Random;
using UnityEngine;

public class DemoSelfAvoidingWalk : MonoBehaviour
{
    public ImageGrid ImageGrid;
    [Range(0f, 1f)]
    public float NextStepDelay;
    public float ShowBoardDelay;
    [Range(0f, 1f)]
    public float Obstacles;
    public long Seed;

    private IPseudoRandomNumberGenerator _rnd;

    private void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        if (Obstacles > 0f)
            ImageGrid.FillWithObstacles(Obstacles, _rnd);
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
        ImageGrid.Set(pointer.x, pointer.y, new ImageGrid.BaseCellValue { Scale = 0.65f, Color = Color.gray });
        yield return new WaitForSeconds(NextStepDelay);

        while (true)
        {
            // Get a random direction to move in
            Vector2Int moveDirection = _rnd.FromArray(Direction2D.OrthogonalDirections);

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
