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

public partial class DemoHamiltonianPath : MonoBehaviour
{
	enum State
	{
		Initializing,
		Processing,
		Success
	}

	enum PathType
	{
		Path,
		Cycle
	}

	class Cell : ImageGrid.BaseCellValue
	{
		public Direction2D.RelativeDirection OutgoingDirection;
		public Direction2D.RelativeDirection IncomingDirection;
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
	private int _step;
	private State _state;

	private void Start()
	{
		_state = State.Initializing;
		_rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
		Seed = _rnd.GetState().AsNumber();
		StartCoroutine(Generate());
	}

	private IEnumerator Generate()
	{
		yield return new WaitForSeconds(ShowBoardDelay);

		_state = State.Processing;

		CreateSimpleHamiltonianCycle(ImageGrid);
		MakeConnections(ImageGrid);

		if (RestartOnSuccess && _state == State.Success)
		{
			// Reload current scene
			Scene scene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(scene.name);
		}
	}

	private void MakeConnections(ImageGrid grid)
	{
		for (int x = 0; x < grid.GridSize.x; ++x)
		{
			for (int y = 0; y < grid.GridSize.y; ++y)
			{
				var cell = grid.Get(x, y) as Cell;
				if(cell == null)
					continue;
				var offset = Direction2D.Offset(cell.OutgoingDirection);
				grid.Connect(x, y, x + offset.x, y + offset.y);
			}
		}
	}

	private void CreateSimpleHamiltonianCycle(ImageGrid grid)
	{
		for (int col = 0; col < grid.GridSize.x;)
		{
			// Go up
			for (int row = 1; row < grid.GridSize.y; row++)
			{
				var cell = new Cell
				{
					OutgoingDirection = row == grid.GridSize.y - 1 ? Direction2D.RelativeDirection.Right : Direction2D.RelativeDirection.Up,
					IncomingDirection = row == 1 ? Direction2D.RelativeDirection.Right : Direction2D.RelativeDirection.Down,
					Scale = 0.8f
				};
				grid.Set(col, row, cell);
			}
			col++;

			// Go down
			for (int row = grid.GridSize.y - 1; row > 0; row--)
			{
				var cell = new Cell
				{
					OutgoingDirection = row == 1 ? Direction2D.RelativeDirection.Right : Direction2D.RelativeDirection.Down,
					IncomingDirection = row == grid.GridSize.y - 1 ? Direction2D.RelativeDirection.Left : Direction2D.RelativeDirection.Up,
					Scale = 0.8f
				};
				grid.Set(col, row, cell);
			}
			col++;
		}

		// Go right
		for (int col = 0; col < grid.GridSize.x; col++)
		{
			var cell = new Cell
			{
				OutgoingDirection = col == 0 ? Direction2D.RelativeDirection.Up : Direction2D.RelativeDirection.Left,
				IncomingDirection = Direction2D.RelativeDirection.Center,
				Scale = 0.8f
			};
			grid.Set(col, 0, cell);
		}
	}
}
