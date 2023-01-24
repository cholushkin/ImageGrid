using System.Collections;
using UnityEngine;

public class SelfAvoidingWalk : MonoBehaviour
{
    public int gridSize = 10;
    public GameObject blockPrefab;
    public float delay = 0.5f;

    private bool[,] grid;
    private Vector2 currentPos;
    private Vector2 prevPos;
    private bool isWalking = false;

    private LineRenderer lineRenderer;

    private void Start()
    {
        // Create an empty grid
        grid = new bool[gridSize, gridSize];

        prevPos = currentPos;

        // Create a line renderer
        lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;

        // Start the walk
        StartCoroutine(Walk());
    }

    private IEnumerator Walk()
    {
        // Set the initial position to the center of the grid
        currentPos = new Vector2(gridSize / 2, gridSize / 2);

        // Instantiate a block prefab at the initial position
        var block = Instantiate(blockPrefab, currentPos, Quaternion.identity);
        block.transform.localScale = Vector3.one * 0.5f;

        while (true)
        {
            if (!isWalking)
            {
                isWalking = true;
                // Get a random direction to move in
                Vector2 moveDirection = GetRandomDirection();

                // Check if the new position is valid (not out of bounds and not already occupied)
                if (IsValidMove(currentPos + moveDirection))
                {
                    // Update the current position
                    prevPos = currentPos;
                    currentPos += moveDirection;

                    // Instantiate a block prefab at the new position
                    block = Instantiate(blockPrefab, currentPos, Quaternion.identity);
                    block.transform.localScale = Vector3.one * 0.5f;

                    // Mark the new position as occupied on the grid
                    grid[(int)currentPos.x, (int)currentPos.y] = true;
                    // Update the line renderer positions
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPos);
                }
                isWalking = false;
                // Wait for the specified delay before continuing
                yield return new WaitForSeconds(delay);
            }
        }
    }

    private Vector2 GetRandomDirection()
    {
        // Generate a random number between 0 and 3
        int randomNum = Random.Range(0, 4);

        // Return a random direction based on the random number
        if (randomNum == 0)
        {
            return Vector2.up;
        }
        else if (randomNum == 1)
        {
            return Vector2.right;
        }
        else if (randomNum == 2)
        {
            return Vector2.down;
        }
        else
        {
            return Vector2.left;
        }
    }

    private bool IsValidMove(Vector2 move)
    {
        // Check if the new position is within the grid bounds
        if (move.x < 0 || move.x >= gridSize || move.y < 0 || move.y >= gridSize)
        {
            return false;
        }

        // Check if the new position is already occupied
        if (grid[(int) move.x, (int) move.y])
        {
            return false;
        }

        return true;
    }
}