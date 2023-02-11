using System.Collections;
using System.Collections.Generic;
using GameLib;
using GameLib.Random;
using UnityEngine;
using UnityEngine.Assertions;

public class DemoSelfAvoidingWalkMaze : MonoBehaviour
{
    public enum State
    {
        Initialization,
        Branching,
        BranchingDone,
        Done
    }
    public ImageGrid ImageGrid;
    [Range(0f, 1f)]
    public float NextStepDelay;
    public float ShowBoardDelay;
    [Range(0f, 1f)]
    public float Obstacles;
    public bool EachBranchRandomColor;
    public Color BranchColor;
    public long Seed;
    
    private IPseudoRandomNumberGenerator _rnd;
    private State _state;
    private int _curBranch;
    


    private void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        if (Obstacles > 0f)
            ImageGrid.FillWithObstacles(Obstacles, _rnd);
        _state = State.Initialization;

        StartCoroutine(Walk());
    }

    private IEnumerator Walk()
    {
        yield return new WaitForSeconds(ShowBoardDelay);

        // Set the initial position to the random position
        Vector2Int branchStartPos = new Vector2Int(
            _rnd.Range(0, ImageGrid.GridSize.x),
            _rnd.Range(0, ImageGrid.GridSize.y));
        
        while (_state != State.Done)
        {
            yield return Branch(branchStartPos);
            yield return new WaitForSeconds(NextStepDelay);
            var availableForBranch = ImageGrid.GetAll((v) => v != null && v.Primitive == ImageGrid.BaseCellValue.PrimitiveType.Circle);
            if (availableForBranch.Count == 0)
            {
                _state = State.Done;
                break;
            }
            branchStartPos = _rnd.FromList(availableForBranch).Item2;
        }
    }

    private IEnumerator Branch(Vector2Int branchStart)
    {
        _state = State.Branching;
        Vector2Int prevPointer = Vector2Int.zero;

        // Get random cell from available for branching
        Vector2Int pointer = branchStart;

        // Get available cells to connect to
        var connectTo = ImageGrid.GetNeigbours(pointer.x, pointer.y, false, (v) => v.Primitive == ImageGrid.BaseCellValue.PrimitiveType.Square);
        if (_curBranch == 0)
        {
            prevPointer = pointer;
        }
        else
        {
            Assert.IsTrue(connectTo.Count > 0);
            prevPointer = _rnd.FromList(connectTo).Item2;
        }

        while (_state != State.BranchingDone)
        {
            ImageGrid.Set(pointer.x, pointer.y, new ImageGrid.BaseCellValue { Scale = 0.65f, Color = BranchColor});
            ImageGrid.Connect(pointer.x, pointer.y, prevPointer.x, prevPointer.y);
            
            // Get all neighbors which are not path cell and not obstacle
            var neigbours = ImageGrid.GetNeigbours(pointer.x, pointer.y, true, (v) => 
                v.Primitive != ImageGrid.BaseCellValue.PrimitiveType.Hex && v.Primitive != ImageGrid.BaseCellValue.PrimitiveType.Square);

            if (neigbours.Count == 0)
            {
                _state = State.BranchingDone;
                continue;
            }

            foreach (var valueTuple in neigbours)
            {
                // Set available for branching visualization
                ImageGrid.Set(valueTuple.Item2.x, valueTuple.Item2.y,
                    new ImageGrid.BaseCellValue
                        {Primitive = ImageGrid.BaseCellValue.PrimitiveType.Circle, Color = Color.blue, Scale = 0.65f});
            }

            // Get a random direction to move in
            prevPointer = pointer;
            pointer = _rnd.FromList(neigbours).Item2;
            yield return new WaitForSeconds(NextStepDelay);
        }
        ++_curBranch;
        if (EachBranchRandomColor)
            BranchColor = _rnd.ColorHSV();
    }


    private bool IsValidMove(Vector2Int move)
    {
        // Check if the new position is within the grid bounds
        if (move.x < 0 || move.x >= ImageGrid.GridSize.x || move.y < 0 || move.y >= ImageGrid.GridSize.y)
            return false;
        return ImageGrid.Get(move.x, move.y) == null;
    }
}
