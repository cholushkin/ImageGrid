using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameLib;
using GameLib.Random;
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
    [Range(0f, 1f)]
    public float NextStepDelay;
    public float ShowBoardDelay;
    public long Seed;
    [Range(0f, 1f)]
    public float Obstacles;
    public bool RestartOnSuccess;
    public string CsvFileName;
    public SuccessConditions SuccessCond;

    private IPseudoRandomNumberGenerator _rnd;
    private int _step;



    private List<Vector2Int> _availableCells;
    private float[] _normProbs;
    private float[] _normProbsCurrent;

    private void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        if (Obstacles > 0f)
            ImageGrid.FillWithObstacles(Obstacles, _rnd);
        StartCoroutine(Walk());
    }

    // _availableCells - set of all empty cells
    //_normProbs - target percent of each piece (normalized)
    //_normProbsCurrent - target percent of each piece (normalized) modified to increase a chance to reach _normProbs. Get updated each step
    // _spawnedPieces - amount of each spawned pieces type

    // 1* get random point from _availableCells
    // * F = get piece using _normProbsCurrent
    // * trySpawn if not goto 1*
    //* update _spawnedPieces and _normProbsCurrent

    //1.5* update Pc
    //* Z = Построить периметр текущего блоба
    //* Если Z пуст - выход
    // * F = получить фигуру с учётом Pc
    //2* проходимся по периметру и пытаемся встроить фигуру F в ячейку. 
    //* Если не удалось берём следубщую фигуру из сета без учета вероятностей -> F и идём на шаг 2
    //* Если доступные фигуры закончились - выход
    // * update SF
    // * goto 1.5

    private IEnumerator Walk()
    {
        // Initialization
        _availableCells = GetAvailableCells();
        _normProbs = GetPercentageOfAllPieces();
        _normProbsCurrent = _normProbs;

        // Set the initial position to the random position
        Vector2Int pointer = _rnd.FromList(_availableCells);


        var p = PieceDescriptions.AllPieceDescriptions[_rnd.SpawnEvent(_normProbsCurrent)];

	    SetPiece(p, pointer);

        yield return new WaitForSeconds(ShowBoardDelay);

        var success = false;
  
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
		    var val = ImageGrid.Get(x, y);
		    if (val != null)
			    res.Add(new Vector2Int(x, y));
	    }

	    return res;
    }


    private void SetPiece(PieceDescriptions.PieceDescription pieceDescription, Vector2Int pos)
    {
	    HashSet<Vector2Int> spawnedCells = new HashSet<Vector2Int>();

	    foreach (var pieceDescriptionCoord in pieceDescription.Coords)
	    {
		    var cPos = pos + pieceDescriptionCoord;

		    ImageGrid.Set(cPos.x, cPos.y,
			    new ImageGrid.BaseCellValue
				    { Color = Color.green, Primitive = ImageGrid.BaseCellValue.PrimitiveType.Square, Scale = 0.7f});

		    spawnedCells.Add(cPos);
	    }

	    // Connect each cell to its adjacent neighbors (up, left, right, down)
	    foreach (var cellPos in spawnedCells)
	    {
		    if(spawnedCells.Contains(new(cellPos.x, cellPos.y - 1)))
				ImageGrid.Connect(cellPos.x, cellPos.y - 1, cellPos.x, cellPos.y); // Connect to cell above
		    if (spawnedCells.Contains(new(cellPos.x-1, cellPos.y )))
				ImageGrid.Connect(cellPos.x - 1, cellPos.y, cellPos.x, cellPos.y); // Connect to cell on the left
		    if (spawnedCells.Contains(new(cellPos.x + 1, cellPos.y)))
				ImageGrid.Connect(cellPos.x + 1, cellPos.y, cellPos.x, cellPos.y); // Connect to cell on the right
		    if (spawnedCells.Contains(new(cellPos.x, cellPos.y+1)))
                ImageGrid.Connect(cellPos.x, cellPos.y + 1, cellPos.x, cellPos.y); // Connect to cell below
        }
    }
}
