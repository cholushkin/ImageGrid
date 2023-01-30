using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GameLib;
using GameLib.Random;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using WaveProcessor;
using RangeInt = GameLib.Random.RangeInt;

public partial class DemoSelfAvoidingWalkWaveCheck : MonoBehaviour
{
    enum State
    {
        Success,
        Fail,
        Processing
    }

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
    private SimpleWaveProcessor<ImageGrid.BaseCellValue> _waveProcessor;
    private int _step;
    private State _state;

    private void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        StartCoroutine(Walk());
    }

    private static bool WallFunction(ImageGrid.BaseCellValue val)
    {
        return (val != null);
    }

    private IEnumerator Walk()
    {
        yield return new WaitForSeconds(ShowBoardDelay);

        _state = State.Processing;
        _waveProcessor = new SimpleWaveProcessor<ImageGrid.BaseCellValue>(ImageGrid.GetCells, WallFunction);

        // Set the initial position to the random position
        Vector2Int prevPointer = Vector2Int.zero;
        Vector2Int pointer = new Vector2Int(
            _rnd.Range(0, ImageGrid.GridSize.x),
            _rnd.Range(0, ImageGrid.GridSize.y));

        // Set a block at the initial position
        ImageGrid.Set(pointer.x, pointer.y, new CellValue { Scale = 0.65f, Color = Color.gray });
        ++_step;
        yield return new WaitForSeconds(NextStepDelay);

        while (_state == State.Processing)
        {
            if (SuccessCond.Steps.To != -1 && (_step >= SuccessCond.Steps.To))
                break;

            // Get a random direction to a list of available directions
            List<int> availableDirections = new List<int>(4);
            for (int i = 0; i < 4; ++i)
                if (IsValidMove(pointer + Direction2D.OrthogonalDirections[i]) && (ImageGrid.Get(pointer.x, pointer.y) as CellValue).Directions[i] == false)
                    availableDirections.Add(i);

            // Propagate wave
            if (availableDirections.Count > 1 && HasSplit(pointer, availableDirections))
            {
                // Roll back
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
            else
            {
                if (availableDirections.Count == 0)
                {
                    // Check finish condition
                    if (ImageGrid.BlocksCounter == ImageGrid.GridSize.x * ImageGrid.GridSize.y || ImageGrid.BlocksCounter == ImageGrid.GridSize.x * ImageGrid.GridSize.y-1)
                    {
                        _state = State.Success;
                        continue;
                    }
                    // Roll back
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
                }
            }

            yield return new WaitForSeconds(NextStepDelay);
        }

        Debug.Log($"{Seed};{ImageGrid.GridSize.x}x{ImageGrid.GridSize.y};{_step};{_state}");
        // Log experiment results to CSV file
        if (!string.IsNullOrEmpty(CsvFileName))
        {
            var header = "Seed;GridSize;Steps;Success";
            var line = $"{Seed};{ImageGrid.GridSize.x}x{ImageGrid.GridSize.y};{_step};{_state}";
            AppendStringToCSV(Application.dataPath + "/" + CsvFileName, header, line);
        }

        if (RestartOnSuccess)
        {
            // Reload current scene
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }

    private bool HasSplit(Vector2Int pointer, List<int> directions)
    {
        Assert.IsTrue(directions.Count > 1);

        List<Vector2Int> availableDirections = new List<Vector2Int>(4);
        foreach (var orthogonalDirection in Direction2D.OrthogonalDirections)
        {
            var dir = pointer + orthogonalDirection;
            if (IsValidMove(dir))
                availableDirections.Add(dir);
        }
                   
        _waveProcessor.Clear();
        int waveIndex = 0;
        int contacts = availableDirections.Count - 1;
        foreach (var wave in _waveProcessor.ComputeWaves(availableDirections[0]))
        {
            _step += wave.Cells.Count;
            if (waveIndex++ == 0)
                continue;
            foreach (var cell in wave.Cells)
            {
                if (cell == availableDirections[1])
                    contacts--;
                if (availableDirections.Count >= 3 && cell == availableDirections[2])
                    contacts--;
                if (availableDirections.Count >= 4 && cell == availableDirections[3]) // only for the first cell in the path
                    contacts--;
            }
            if (contacts == 0)
                return false;
        }
        return true;
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
            if (newFile)
                sw.WriteLine(header);
            sw.WriteLine(newLine);
        }
    }
}
