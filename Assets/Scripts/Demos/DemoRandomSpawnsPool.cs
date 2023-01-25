using System.Collections;
using GameLib.Random;
using UnityEngine;

public class DemoRandomSpawnsPool : MonoBehaviour
{
    public ImageGrid ImageGrid;
    [Range(0f, 1f)]
    public float NextStepDelay;
    public float ShowBoardDelay;
    public bool ConnectWithSameColorOnly;
    public long Seed;

    public Color[] Colors;
    public float[] Scales;
    public ImageGrid.BaseCellValue.PrimitiveType[] Primitives;

    private IPseudoRandomNumberGenerator _rnd;
    private int _step;


    void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        StartCoroutine(Demo());
    }

    private IEnumerator Demo()
    {
        yield return new WaitForSeconds(ShowBoardDelay);
        while (true)
        {
            _step++;
            var x = _rnd.Range(0, ImageGrid.GridSize.x);
            var y = _rnd.Range(0, ImageGrid.GridSize.y);

            if (!ImageGrid.HasAnyConnection(x, y))
            {
                var cellValue = new ImageGrid.BaseCellValue();
                cellValue.Primitive = _rnd.FromArray(Primitives);
                cellValue.Color = _rnd.FromArray(Colors);
                cellValue.Scale = _rnd.FromArray(Scales);

                ImageGrid.Set(x, y, cellValue);

                if (ConnectWithSameColorOnly)
                {
                    if (ImageGrid.Get(x + 1, y)?.Color == cellValue.Color)
                        ImageGrid.Connect(x, y, x + 1, y);
                    if (ImageGrid.Get(x - 1, y)?.Color == cellValue.Color)
                        ImageGrid.Connect(x, y, x - 1, y);
                    if (ImageGrid.Get(x, y + 1)?.Color == cellValue.Color)
                        ImageGrid.Connect(x, y, x, y + 1);
                    if (ImageGrid.Get(x, y - 1)?.Color == cellValue.Color)
                        ImageGrid.Connect(x, y, x, y - 1);
                }
                else
                {
                    ImageGrid.Connect(x, y, x + 1, y);
                    ImageGrid.Connect(x, y, x - 1, y);
                    ImageGrid.Connect(x, y, x, y + 1);
                    ImageGrid.Connect(x, y, x, y - 1);
                }
            }
            yield return new WaitForSeconds(NextStepDelay);
        }
    }
}
