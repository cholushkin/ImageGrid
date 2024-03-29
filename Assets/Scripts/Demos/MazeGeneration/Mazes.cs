//using System.Collections.Generic;
//using System.Linq;
//using GameLib.Random;
//using UnityEngine;

//namespace Mazes
//{
//    public static class Maze
//    {
//        public enum GenerationAlgorithm
//        {
//            BinaryTree,
//            Sidewinder,
//            AldousBroder,
//            Wilson,
//            HuntAndKill,
//            RecursiveBacktracker
//        }

//        public delegate void GenerateMaze(ImageGrid grid);

//        //.N.
//        //W.E
//        //.S.
//        public static bool HasNorthCell(ImageGrid grid, int column, int row)
//        {
//            return row + 1 < grid.GridSize.y && grid.Cells[column, row + 1] != null;
//        }

//        public static ImageGrid.BaseCellValue GetNorthCell(ImageGrid grid, int column, int row)
//        {
//            return grid.Get(column, row + 1);
//        }

//        public static bool HasEastCell(ImageGrid grid, int column, int row)
//        {
//            return column + 1 < grid.GridSize.x && grid.Cells[column + 1, row] != null;
//        }

//        public static ImageGrid.BaseCellValue GetEastCell(ImageGrid grid, int column, int row)
//        {
//            return grid.Get(column + 1, row);
//        }

//        public static bool HasWestCell(ImageGrid grid, int column, int row)
//        {
//            return column - 1 >= 0 && grid.Cells[column - 1, row] != null;
//        }

//        public static ImageGrid.BaseCellValue GetWestCell(ImageGrid grid, int column, int row)
//        {
//            return grid.Get(column - 1, row);
//        }

//        public static bool HasSouthCell(ImageGrid grid, int column, int row)
//        {
//            return row - 1 >= 0 && grid.Cells[column, row - 1] != null;
//        }

//        public static ImageGrid.BaseCellValue GetSouthCell(ImageGrid grid, int column, int row)
//        {
//            return grid.Get(column, row - 1);
//        }

//        public static void GenerateBinaryTree(ImageGrid grid, IPseudoRandomNumberGenerator rnd)
//        {
//            for(int column = 0; column < grid.GridSize.x; ++column)
//	            for (int row = 0; row < grid.GridSize.y; ++row)
//	            {
//		            var cell = grid.Get(column, row);
//	                bool hasNorthCell = HasNorthCell(grid, column, row);
//	                bool hasEastCell = HasEastCell(grid, column, row);

//	                if (hasNorthCell && hasEastCell)
//	                {
//	                    if (rnd.ValueFloat() > 0.5f)
//	                    {
//		                    cell.Link(GetNorthCell(grid, cell.Column, cell.Row), true);
//	                    }
//	                    else
//	                    {
//	                        cell.Link(GetEastCell(grid, cell.Column, cell.Row), true);
//	                    }
//	                }
//	                else if (!hasNorthCell && hasEastCell)
//	                {
//	                    cell.Link(GetEastCell(grid, cell.Column, cell.Row), true);
//	                }
//	                else if (hasNorthCell && !hasEastCell)
//	                {
//	                    cell.Link(GetNorthCell(grid, cell.Column, cell.Row), true);
//	                }
//	            }
//        }

//        public static void GenerateSidewinder(ImageGrid grid)
//        {
//            var run = new List<Cell>(grid.Columns);
//            //For Each Row
//            for (int row = 0; row < grid.Rows; ++row)
//            {
//                //For each cell in a row
//                for (int col = 0; col < grid.Columns; ++col)
//                {
//                    var cell = grid.Cells[col, row];
//                    run.Add(cell);

//                    bool hasNorthCell = HasNorthCell(grid, cell.Column, cell.Row);
//                    bool hasEastCell = HasEastCell(grid, cell.Column, cell.Row);
//                    bool shouldCloseOut = !hasEastCell || (hasNorthCell && Random.value > 0.5f);

//                    if (shouldCloseOut)
//                    {
//                        var member = SampleUtil<Cell>.Sample(run);
//                        if (HasNorthCell(grid, member.Column, member.Row))
//                            member.Link(GetNorthCell(grid, member.Column, member.Row), true);
//                        run.Clear();
//                    }
//                    else
//                    {
//                        cell.Link(GetEastCell(grid, cell.Column, cell.Row), true);
//                    }
//                }
//            }
//        }

//        public static void GenerateAldousBroder(ImageGrid grid)
//        {
//            var cell = grid.GetRandomCell();
//            int unvisited = grid.Cells.Length - 1;

//            while (unvisited > 0)
//            {
//                var neighbours = cell.GetNeighboursList();
//                var neighbour = SampleUtil<Cell>.Sample(neighbours);

//                if (neighbour.Links.Count == 0)
//                {
//                    cell.Link(neighbour, true);
//                    unvisited -= 1;
//                }

//                cell = neighbour;
//            }
//        }

//        public static void GenerateWilson(ImageGrid grid)
//        {
//            var unvisitedCells = new List<Cell>(grid.Cells.Length);
//            foreach (var cell in grid.Cells)
//            {
//                unvisitedCells.Add(cell);
//            }

//            var firstCell = SampleUtil<Cell>.Sample(unvisitedCells);
//            unvisitedCells.Remove(firstCell);

//            while (unvisitedCells.Count > 0)
//            {
//                var cell = SampleUtil<Cell>.Sample(unvisitedCells);
//                List<Cell> path = new List<Cell> { cell };

//                while (unvisitedCells.Contains(cell))
//                {
//                    cell = SampleUtil<Cell>.Sample(cell.GetNeighboursList());
//                    var cellIndex = path.IndexOf(cell);

//                    if (cellIndex == -1)
//                        path.Add(cell);
//                    else
//                        path = path.GetRange(0, cellIndex + 1);
//                }

//                for (int i = 0; i < path.Count - 1; ++i)
//                {
//                    path[i].Link(path[i + 1], true);
//                    unvisitedCells.Remove(path[i]);
//                }
//            }
//        }

//        public static void GenerateHuntAndKill(ImageGrid grid)
//        {
//            var currentCell = grid.GetRandomCell();

//            while (currentCell != null)
//            {
//                var unvisitedNeighbours = currentCell.GetNeighboursList().FindAll(c => c.Links.Count == 0);
//                if (unvisitedNeighbours.Count > 0)
//                {
//                    var neighbour = unvisitedNeighbours.Sample();
//                    currentCell.Link(neighbour, true);
//                    currentCell = neighbour;
//                }
//                else
//                {
//                    currentCell = null;
//                    foreach (var cell in grid.Cells)
//                    {
//                        var visitedNeighbours = cell.GetNeighboursList().FindAll(c => c.Links.Count > 0);
//                        if (cell.Links.Count == 0 && visitedNeighbours.Count > 0)
//                        {
//                            currentCell = cell;
//                            var neighbour = visitedNeighbours.Sample();
//                            currentCell.Link(neighbour, true);
//                            break;
//                        }
//                    }
//                }
//            }
//        }

//        public static void GenerateRecursiveBacktracker(ImageGrid grid)
//        {
//            GenerateRecursiveBacktracker(grid, grid.GetRandomCell());
//        }

//        public static void GenerateRecursiveBacktracker(ImageGrid grid, Cell start)
//        {
//            var stack = new Stack<Cell>();
//            stack.Push(start);

//            while (stack.Count > 0)
//            {
//                Cell currentCell = stack.Peek();
//                var unvisitedNeighbours = currentCell.GetNeighboursList().FindAll(cell => cell.Links.Count == 0);

//                if (unvisitedNeighbours.Count == 0)
//                    stack.Pop();
//                else
//                {
//                    var neighbour = unvisitedNeighbours.Sample();
//                    currentCell.Link(neighbour, true);
//                    stack.Push(neighbour);
//                }
//            }
//        }

//        public static Dictionary<Cell, int> CalculateDistancesFromRoot(ImageGrid grid, Cell root)
//        {
//            var result = new Dictionary<Cell, int>(grid.Cells.Length);
//            var frontier = new HashSet<Cell> { root };

//            result.Add(root, 0);
//            while (frontier.Count > 0)
//            {
//                var newFrontier = new HashSet<Cell>();
//                foreach (Cell cell in frontier)
//                {
//                    foreach (Cell link in cell.Links)
//                    {
//                        if (result.ContainsKey(link)) continue;

//                        result.Add(link, result[cell] + 1);
//                        newFrontier.Add(link);
//                    }
//                }

//                frontier = newFrontier;
//            }

//            return result;
//        }

//        public static List<Cell> CalculatePath(ImageGrid grid, Cell root, Cell goal)
//        {
//            var result = new List<Cell>();
//            var current = goal;
//            result.Add(current);

//            var distances = CalculateDistancesFromRoot(grid, root);

//            while (current != root)
//            {
//                foreach (var link in current.Links)
//                {
//                    if (distances[link] < distances[current])
//                    {
//                        result.Add(link);
//                        current = link;
//                        break;
//                    }
//                }
//            }

//            result.Reverse();
//            return result;
//        }

//        public static List<Cell> CalculateLongestPath(ImageGrid grid)
//        {
//            var distances = CalculateDistancesFromRoot(grid, grid.Cells[0, 0]);
//            var maxDistanceCell = distances.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

//            distances.Clear();
//            distances = CalculateDistancesFromRoot(grid, maxDistanceCell);
//            var newMaxDistanceCell = distances.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;

//            return CalculatePath(grid, maxDistanceCell, newMaxDistanceCell);
//        }

//        public static List<Cell> CalculateDeadEnds(ImageGrid grid)
//        {
//            List<Cell> result = new List<Cell>();
//            foreach (var cell in grid.Cells)
//            {
//                if (cell.Links.Count == 1)
//                    result.Add(cell);
//            }

//            return result;
//        }
//    }
//}