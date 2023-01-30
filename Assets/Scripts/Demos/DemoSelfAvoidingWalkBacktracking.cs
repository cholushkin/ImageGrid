using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameLib;
using GameLib.Random;
using UnityEngine;
using UnityEngine.SceneManagement;
using RangeInt = GameLib.Random.RangeInt;

public partial class DemoSelfAvoidingWalkBacktracking : MonoBehaviour
{
    public class CellValue : ImageGrid.BaseCellValue
    {
        public bool[] Directions = { false, false, false, false };
    }

    [Serializable]
    public class SuccessConditions
    {
        public RangeInt Steps;
        public float FillPercent;
    }

    public ImageGrid ImageGrid;
    [Range(0f, 1f)]
    public float NextStepDelay;
    public float ShowBoardDelay;
    public long Seed;
    public bool RestartOnSuccess;
    public string CsvFileName;
    public SuccessConditions SuccessCond;

    private IPseudoRandomNumberGenerator _rnd;
    private int _step;

    private void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        StartCoroutine(Walk());
    }

    private IEnumerator Walk()
    {
        yield return new WaitForSeconds(ShowBoardDelay);

        var success = false;

        // Set the initial position to the random position
        Vector2Int prevPointer = Vector2Int.zero;
        Vector2Int pointer = new Vector2Int(
            _rnd.Range(0, ImageGrid.GridSize.x),
            _rnd.Range(0, ImageGrid.GridSize.y));

        // Set a block at the initial position
        ImageGrid.Set(pointer.x, pointer.y, new CellValue { Scale = 0.65f, Color = Color.gray });
        ++_step;
        yield return new WaitForSeconds(NextStepDelay);

        while (true)
        {
            if (SuccessCond.Steps.To !=-1 && (_step >= SuccessCond.Steps.To))
            {
                break;
            }
            // Get a random direction to a list of available directions
            List<int> availableDirections = new List<int>(4);
            for (int i = 0; i < 4; ++i)
                if (IsValidMove(pointer + Direction2D.OrthogonalDirections[i]) && (ImageGrid.Get(pointer.x, pointer.y) as CellValue).Directions[i]==false)
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
                        offset = Direction2D.OrthogonalDirections[i];
                        break;
                    }

                // Delete self
                ImageGrid.Set(pointer.x, pointer.y, null);

                // Assign pointer to prev
                pointer += offset;
                ++_step;
            }
            // Move to direction
            else
            {
                // Choose one of available direction and mark it as used
                var dirIndex = _rnd.FromList(availableDirections);
                var moveDirection = Direction2D.OrthogonalDirections[dirIndex];
                (ImageGrid.Get(pointer.x, pointer.y) as CellValue).Directions[dirIndex] = true;

                prevPointer = pointer;
                pointer += moveDirection;

                // Set a block at the new position
                var newCellVal = new CellValue { Scale = 0.65f };
                ImageGrid.Set(pointer.x, pointer.y, newCellVal);
                ImageGrid.Connect(pointer.x, pointer.y, prevPointer.x, prevPointer.y);
                ++_step;

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

                    success = true;
                        
                    break;
                }
            }

            yield return new WaitForSeconds(NextStepDelay);
        }

        // Log experiment results to CSV file
        if (!string.IsNullOrEmpty(CsvFileName) /*&& success*/)
        {
            var header = "Seed;GridSize;Steps;Success";
            var line = $"{Seed};{ImageGrid.GridSize.x}x{ImageGrid.GridSize.y};{_step};{success}";
            AppendStringToCSV(Application.dataPath + "/" + CsvFileName, header, line);
        }

        if (RestartOnSuccess)
        {
            // Reload current scene
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    private bool IsValidMove(Vector2Int move)
    {
        // Check if the new position is within the grid bounds
        if (move.x < 0 || move.x >= ImageGrid.GridSize.x || move.y < 0 || move.y >= ImageGrid.GridSize.y)
            return false;
        return ImageGrid.Get(move.x, move.y) == null;
    }

    public static void AppendStringToCSV(string filePath, string header, string newLine)
    {
        var newFile = false;
        // Check if the file already exists
        if (!File.Exists(filePath))
        {
            // If the file doesn't exist, create it
            File.Create(filePath).Dispose();
            newFile = true;
        }

        // Open the file to write the new line
        using (StreamWriter sw = File.AppendText(filePath))
        {
            if(newFile)
                sw.WriteLine(header);
            sw.WriteLine(newLine);
        }
    }
}
