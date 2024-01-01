using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameLib;
using GameLib.Random;
using NUnit.Framework;
using PentominoesLib;
using UnityEngine;
using RangeInt = GameLib.Random.RangeInt;

public class DemoSelfAvoidingOmino : MonoBehaviour
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
    
    [UnityEngine.Range(0f, 1f)]
    public float NextStepDelay;
    
    public float ShowBoardDelay;
    public long Seed;
    
    public Color[] Colors;

    [UnityEngine.Range(0f, 1f)]
    public float Obstacles;
    public bool RestartOnSuccess;
    public string CsvFileName;
    public SuccessConditions SuccessCond;

    private IPseudoRandomNumberGenerator _rnd;


    private int _blobIndex;
    private List<Vector2Int> _perimeter;
    private List<Vector2Int> _availableCells; // set of all empty cells available for piece spawning
    private float[] _normProbs; // target percent of each piece (normalized)
    private float[] _normProbsCurrent; // target percent of each piece (normalized) modified to increase a chance to reach _normProbs. Get updated each spawn
    private int[] _pieceTypeSpawnedCounter; // number of each type of the piece being spawned

    private void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        if (Obstacles > 0f)
        {
            Debug.Log($"Filling with obstacles {Obstacles}");
            ImageGrid.FillWithObstacles(Obstacles, _rnd);
        }

        StartCoroutine(Walk());
    }

    // While there are available cells try to spawn and grow blobs of pieces distribution
    private IEnumerator Walk()
    {
        // Initialization
        _availableCells = GetAvailableCells();
        _normProbs = GetPercentageOfAllPieces();
        _normProbsCurrent = (float[])_normProbs.Clone();
        _pieceTypeSpawnedCounter = new int[PieceDescriptions.AllPieceDescriptions.Length];
        var initialEmptyCellCount = _availableCells.Count;


        var fieldCovered = false;
        int safeCounter = 100000;

        while (!fieldCovered)
        {
            // Get random position
            Vector2Int blobCenter = _rnd.FromList(_availableCells);
            _perimeter = CreateProxyPerimeter(blobCenter);


            // Get piece using probability
            var pieceID = _rnd.SpawnEvent(_normProbsCurrent);
            var p = PieceDescriptions.AllPieceDescriptions[pieceID];

            var allPieces = PieceDescriptions.AllPieceDescriptions.OrderBy(p => p.Probability).ToList();// todo: check if sort by _normProbsCurrent is better

            // Blobbing
            while (true)
            {
                //// Visualization: perimeter
                //foreach (var pv in _perimeter)
                //    ImageGrid.Set(pv.x, pv.y,
                //        new ImageGrid.BaseCellValue
                //            {Color = Color.blue, 
                //             Primitive = ImageGrid.BaseCellValue.PrimitiveType.Circle});
                //yield return new WaitForSeconds(NextStepDelay);
                //foreach (var pv in _perimeter)
                //    ImageGrid.Set(pv.x, pv.y, null);


                var ableToSpawnCurrentPiece = false;
                foreach (var pv in _perimeter)
                {
                    var locations = ConvertToGlobalPieces(p, pv);
                    foreach (var pieceLocationVar in locations)
                    {
                        if (CanSpawnPiece(pieceLocationVar))
                        {
                            yield return new WaitForSeconds(NextStepDelay);
                            SpawnPiece(pieceLocationVar, Colors[pieceID]);
                            _pieceTypeSpawnedCounter[pieceID]++;
                            UpdatePerimeter(_perimeter, pieceLocationVar);
                            UpdateProbabilities(initialEmptyCellCount);

                            Debug.Log($"{string.Join('|', _pieceTypeSpawnedCounter)} {string.Join(' ', _normProbsCurrent)}   {pieceID}");
                            _availableCells.RemoveAll(pieceLocationVar.Contains);
                            ableToSpawnCurrentPiece = true;
                            break;
                        }
                    }
                    if(ableToSpawnCurrentPiece)
                        break;
                }

                if(_perimeter.Count == 0)
                    break;

                if (ableToSpawnCurrentPiece)
                {
                    // Get piece using probability
                    allPieces = PieceDescriptions.AllPieceDescriptions.OrderBy(p => p.Probability).ToList();// todo: check if sort by _normProbsCurrent is better
                    pieceID = _rnd.SpawnEvent(_normProbsCurrent);
                    p = PieceDescriptions.AllPieceDescriptions[pieceID];
                }
                else
                {
                    // Get next piece
                    allPieces.Remove(p);
                    if (allPieces.Count == 0)
                        break;
                    (p, pieceID) = GetNexPiece(allPieces);
                }
            }

            _availableCells.RemoveAll(_perimeter.Contains); // nothing could be spawned on current blob perimeter

            fieldCovered = _availableCells.Count == 0;
            if (safeCounter-- < 0)
            {
                Debug.LogError("safe counter hit");
                break;
            }
        }
        Debug.Log("done");
        yield return new WaitForSeconds(ShowBoardDelay);
    }

    private (PieceDescriptions.PieceDescription p, int pieceID) GetNexPiece(List<PieceDescriptions.PieceDescription> allPieces)
    {
        var p = allPieces.First();
        allPieces.RemoveAt(0); 
        return (p, p.ID);
    }

    private void UpdatePerimeter(List<Vector2Int> perimeter, List<Vector2Int> gPiece)
    {
        // remove all cells of the piece from the perimeter
        _perimeter.RemoveAll(gPiece.Contains);

        // add neigbours of the piece (only orthogonal ones) to the perimeter
        foreach (var nCell in gPiece)
        {
            var c = new Vector2Int(nCell.x - 1, nCell.y);
            if (ImageGrid.IsEmptyCell(c.x, c.y) && !_perimeter.Contains(c))
                _perimeter.Add(c);
            c = new Vector2Int(nCell.x + 1, nCell.y);
            if (ImageGrid.IsEmptyCell(c.x, c.y) && !_perimeter.Contains(c))
                _perimeter.Add(c);
            c = new Vector2Int(nCell.x, nCell.y-1);
            if (ImageGrid.IsEmptyCell(c.x, c.y) && !_perimeter.Contains(c))
                _perimeter.Add(c);
            c = new Vector2Int(nCell.x, nCell.y + 1);
            if (ImageGrid.IsEmptyCell(c.x, c.y) && !_perimeter.Contains(c))
                _perimeter.Add(c);
        }
    }

    private void SpawnPiece(List<Vector2Int> gPiece, Color color)
    {
        var spawnedCells = new List<Vector2Int>();
        foreach (var cell in gPiece)
        {
            ImageGrid.Set(cell.x, cell.y,
                new ImageGrid.BaseCellValue
                    { Color = color, Primitive = ImageGrid.BaseCellValue.PrimitiveType.Square, Scale = 0.7f });
            spawnedCells.Add(cell);
        }

        // Connect each cell to its adjacent neighbors (up, left, right, down)
        foreach (var cellPos in spawnedCells)
        {
            if (spawnedCells.Contains(new(cellPos.x, cellPos.y - 1)))
                ImageGrid.Connect(cellPos.x, cellPos.y - 1, cellPos.x, cellPos.y); // Connect to cell above
            if (spawnedCells.Contains(new(cellPos.x - 1, cellPos.y)))
                ImageGrid.Connect(cellPos.x - 1, cellPos.y, cellPos.x, cellPos.y); // Connect to cell on the left
            if (spawnedCells.Contains(new(cellPos.x + 1, cellPos.y)))
                ImageGrid.Connect(cellPos.x + 1, cellPos.y, cellPos.x, cellPos.y); // Connect to cell on the right
            if (spawnedCells.Contains(new(cellPos.x, cellPos.y + 1)))
                ImageGrid.Connect(cellPos.x, cellPos.y + 1, cellPos.x, cellPos.y); // Connect to cell below
        }
    }

    private bool CanSpawnPiece( List<Vector2Int> gPiece)
    {
        return gPiece.All(cell => ImageGrid.IsEmptyCell(cell.x, cell.y));
    }

    private List<List<Vector2Int>> ConvertToGlobalPieces(PieceDescriptions.PieceDescription pieceDescription, Vector2Int pointer)
    {
        var pieces = new List<List<Vector2Int>>();
        foreach (var coord in pieceDescription.Coords)
        {
            var piece = new List<Vector2Int>();
            foreach (var offset in pieceDescription.Coords)
            {
                var localCoord = offset - coord; 
                piece.Add(localCoord + pointer);
            }
            pieces.Add(piece);
        }
        return pieces;
    }

    private void UpdateProbabilities(int initialEmptyCellCount)
    {
        // real distribution
        float sum = _pieceTypeSpawnedCounter.Sum();
        Assert.IsTrue(sum != 0f);

        var realPercentage = new float [_normProbs.Length];
        int i = 0;
        foreach (var cnt in _pieceTypeSpawnedCounter)
        {
            if (cnt == 0)
                _normProbsCurrent[i] = _normProbs[i];
            else
            {
                var expectedPercent = _normProbs[i];
                var expectedPiecesOfCurTypeCount = (expectedPercent * initialEmptyCellCount) /
                                                   PieceDescriptions.AllPieceDescriptions[i].Coords.Count;
                var realPercent = cnt / expectedPiecesOfCurTypeCount;
                _normProbsCurrent[i] = _normProbs[i] * (expectedPercent / realPercent);
            }
            i++;
        }
    }

    private List<Vector2Int> CreateProxyPerimeter(Vector2Int pointer)
    {
        return new List<Vector2Int>{pointer};
    }

    private float[] GetPercentageOfAllPieces()
    {
	    float[] probsNorm = new float[PieceDescriptions.AllPieceDescriptions.Count()];
	    float sum = PieceDescriptions.AllPieceDescriptions.Sum(allPieceDescription => allPieceDescription.Probability);
	    int i = 0;
	    foreach (var dsc in PieceDescriptions.AllPieceDescriptions)
		    probsNorm[i++] = dsc.Probability / sum;
        return probsNorm;
    }

    private List<Vector2Int> GetAvailableCells()
    {
	    var res = new List<Vector2Int>();
	    var width = ImageGrid.GridSize.x;
	    var height = ImageGrid.GridSize.y;

	    for (int x = 0; x < width; ++x)
	        for (int y = 0; y < height; ++y)
	        {
		        if (ImageGrid.IsEmptyCell(x, y))
			        res.Add(new Vector2Int(x, y));
	        }

	    return res;
    }
  
}
