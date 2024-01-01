using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PentominoesLib
{
    public static class PieceDescriptions
    {
        public class PieceDescription
        {
            public readonly string Label;
            public readonly float Probability;
            public List<Vector2Int> Coords;
            public readonly int ID;

            public PieceDescription(int id, string label, float probability, IEnumerable<string> pattern)
            {
                ID = id;
                Label = label;
                Probability = probability;
                Coords = CreateCoordinates(pattern);

            }

            private List<Vector2Int> CreateCoordinates(IEnumerable<string> pattern)
            {
                var coords = new List<Vector2Int>();
                int y = pattern.Count() - 1; // Start y from the bottom

                foreach (var line in pattern)
                {
                    int x = 0;
                    foreach (var character in line)
                    {
                        if (character == 'X')
                            coords.Add(new Vector2Int(x, y));
                        x++;
                    }
                    y--; // Move up one row
                }
                return coords;
            }
        }


        public static PieceDescription[] AllPieceDescriptions = new[] {
            new PieceDescription(0, "Pm", 1, new[]{
                "XX",
                " X"}),
            new PieceDescription(1, "Lw", 1, new[]{
                "XX",
                "X "}),
            new PieceDescription(2, "L", 1, new[]{
                "X ",
                "XX"}),
            new PieceDescription(3, "Lm", 1, new[]{
                " X",
                "XX"}),
            new PieceDescription(4, "I", 1, new[]{
                "X",
                "X",
                "X"}),
            new PieceDescription(5, "_", 1, new[]{
                "XXX"}),
            new PieceDescription(6, "x", 1, new[]{
                "XX"}),
            new PieceDescription(7, "xx", 1, new[]{
                "X",
                "X"}),
            new PieceDescription(8, "xxx", 0, new[]{
                "X"})

        };
    }
}
