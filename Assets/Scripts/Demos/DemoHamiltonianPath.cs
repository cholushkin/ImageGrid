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

	public ImageGrid ImageGrid;
	[Range(0f, 1f)]
	public float NextStepDelay;
	public float ShowBoardDelay;
	public long Seed;
	public bool RestartOnSuccess;
	public string CsvFileName;

	private IPseudoRandomNumberGenerator _rnd;
	private int _step;
	private State _state;

	private void Start()
	{
		_state = State.Initializing;
		_rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
		Seed = _rnd.GetState().AsNumber();
		Assert.IsTrue(ImageGrid.GridSize.x % 2 == 0);
		StartCoroutine(Generate());
	}

	private IEnumerator Generate()
	{
		yield return new WaitForSeconds(ShowBoardDelay);

		_state = State.Processing;

		CreateSimpleHamiltonianCycle(ImageGrid);

		if (RestartOnSuccess && _state == State.Success)
		{
			// Reload current scene
			Scene scene = SceneManager.GetActiveScene();
			SceneManager.LoadScene(scene.name);
		}
	}

	private void CreateSimpleHamiltonianCycle(ImageGrid grid)
	{
		Vector2Int prev = Vector2Int.zero;

        void Step(int x, int y)
        {
            grid.Set(x, y, new ImageGrid.BaseCellValue { Scale = 0.7f });
            grid.Connect(x, y, prev.x, prev.y);
            prev.Set(x, y);
		}

		for (int col = 0; col < grid.GridSize.x; col += 2)
		{
			// Go up
			for (int row = 1; row < grid.GridSize.y; row++)
                Step(col, row);

			// Go down
			for (int row = grid.GridSize.y - 1; row > 0; row--)
                Step(col + 1, row);
		}

		// Go left
		for (int col = grid.GridSize.x - 1; col >= 0; --col)
            Step(col, 0);
        ImageGrid.Connect(0, 0, 0, 1);
    }
}
