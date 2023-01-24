using System.Collections;
using GameLib.Random;
using UnityEngine;

public class DemoRandomColors : MonoBehaviour
{
	public ImageGrid ImageGrid;
	[Range(0f, 1f)]
	public float DelayForNextColorChange;

	public bool RandomShapes;
	public Range ScaleRnd;

	public long Seed;
	private IPseudoRandomNumberGenerator _rnd;


	void Start()
	{
		_rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
		Seed = _rnd.GetState().AsNumber();
		ImageGrid.FillRndColor(_rnd);
		StartCoroutine(Demo());
	}

	private IEnumerator Demo()
	{
		while (true)
		{
			for (int x = 0; x < ImageGrid.GridSize.x; ++x)
			{
				for (int y = 0; y < ImageGrid.GridSize.y; ++y)
				{
					var primitive = RandomShapes ? _rnd.FromEnum<ImageGrid.BaseCellValue.PrimitiveType>() : ImageGrid.BaseCellValue.PrimitiveType.Square;
					var scale = _rnd.FromRange(ScaleRnd);
					ImageGrid.Set(x, y, new ImageGrid.BaseCellValue { Color = _rnd.ColorHSV(), Primitive = primitive, Scale = scale });
					yield return new WaitForSeconds(DelayForNextColorChange);
				}
			}
		}
	}
}
