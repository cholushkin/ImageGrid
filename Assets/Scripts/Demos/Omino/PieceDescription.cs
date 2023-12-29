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

            public PieceDescription(string label, float probability, IEnumerable<string> pattern)
			{
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
			new PieceDescription("Pm", 1, new[]{
				"XXX",
				"XXX",
				" XX"}),
			new PieceDescription("Lw", 1, new[]{
				"XXX",
				"X  ",
				"X  "}),

 
            //new PieceDescription("F", new[]{
            //    " XX",
            //    "XX ",
            //    " X "
            //}),
            //new PieceDescription("I", new[]{
            //    "X",
            //    "X",
            //    "X",
            //    "X",
            //    "X"
            //}),
            //new PieceDescription("L", new[]{
            //    "X ",
            //    "X ",
            //    "X ",
            //    "XX"
            //}),
            //new PieceDescription("P", new[]{
            //    "XX",
            //    "XX",
            //    "X "
            //}),
            //new PieceDescription("N", new[]{
            //    " X",
            //    "XX",
            //    "X ",
            //    "X "
            //}),
            //new PieceDescription("T", new[]{
            //    "XXX",
            //    " X ",
            //    " X "
            //}),
            //new PieceDescription("U", new[]{
            //    "X X",
            //    "XXX"
            //}),
            //new PieceDescription("V", new[]{
            //    "X  ",
            //    "X  ",
            //    "XXX"
            //}),
            //new PieceDescription("W", new[]{
            //    "X  ",
            //    "XX ",
            //    " XX"
            //}),
            //new PieceDescription("X", new[]{
            //    " X ",
            //    "XXX",
            //    " X "
            //}),
            //new PieceDescription("Y", new[]{
            //    " X",
            //    "XX",
            //    " X",
            //    " X"
            //}),
            //new PieceDescription("Z", new[]{
            //    "XX ",
            //    " X ",
            //    " XX"
            //}),
        };
	}
}
