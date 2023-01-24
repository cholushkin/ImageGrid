using System.Collections;
using GameLib.Random;
using UnityEngine;

public class DemoRandomSpawns : MonoBehaviour
{
    public ImageGrid ImageGrid;
    [Range(0f, 1f)]
    public float NextStepDelay;
    public long Seed;

    private IPseudoRandomNumberGenerator _rnd;


    void Start()
    {
        _rnd = RandomHelper.CreateRandomNumberGenerator(Seed);
        Seed = _rnd.GetState().AsNumber();
        StartCoroutine(Demo());
    }

    private IEnumerator Demo()
    {
        while (true)
        {
            var x = _rnd.Range(0, ImageGrid.GridSize.x);
            var y = _rnd.Range(0, ImageGrid.GridSize.y);
            var cellValue = new ImageGrid.BaseCellValue();
            cellValue.Primitive = _rnd.FromEnum<ImageGrid.BaseCellValue.PrimitiveType>();
            cellValue.Color = _rnd.ColorHSV();
            cellValue.Scale = 0.6f;
            ImageGrid.Set(x, y, cellValue);
            
            ImageGrid.Connect(x, y, x + 1, y);
            ImageGrid.Connect(x, y, x - 1, y);
            ImageGrid.Connect(x, y, x, y + 1);
            ImageGrid.Connect(x, y, x, y - 1);

            yield return new WaitForSeconds(NextStepDelay);
        }
    }
}
