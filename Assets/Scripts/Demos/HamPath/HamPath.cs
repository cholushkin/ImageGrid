using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

class Paths
{
    public List<Path> allPaths = new List<Path>();

    public void addPath(Path path)
    {
        allPaths.Add(path);
    }

    public void smallestPath()
    {
        if (allPaths.Count > 0)
        {
            allPaths.Sort();
            int smallestWeight = allPaths[0].getWeight();
            int numberOfSmallest = 1;
            while (numberOfSmallest < allPaths.Count && allPaths[numberOfSmallest].getWeight() == smallestWeight)
            {
                numberOfSmallest++;
            }
            if (numberOfSmallest == 1)
            {
                Debug.Log($"\nSMALLEST PATH IS :\n{allPaths[0].toString()}\t\t{allPaths[0].weightString()}");
            }
            else
            {
	            Debug.Log("\nSMALLEST PATHS ARE :");
                for (int i = 0; i < numberOfSmallest; i++)
                {
	                Debug.Log("{allPaths[i].toString()}\t\t{allPaths[i].weightString()}");
                }
            }

        }
    }

    public void printAllPaths()
    {
        if (allPaths.Count > 0)
        {
	        Debug.LogFormat("\nTHERE ARE {0} PATHS THAT SATISFIES REQUIRED CONDITIONS :", allPaths.Count);
            foreach (var item in allPaths)
            {
	            Debug.LogFormat("{0}\t\t{1}", item.toString(), item.weightString());
            }
        }
        else
        {
	        Debug.LogFormat("\nTHERE ARE NO PATHS THAT SATISFIES REQUIRED CONDITIONS.");
        }
    }


    public class Path : IComparable<Path>
    {
        public List<graphNode> path = new List<graphNode>();

        public Path()
        {

        }

        public Path(List<graphNode> nodes)
        {
            path = nodes;
        }

        public void addNode(graphNode node)
        {
            path.Add(node);
        }

        public void removeLast()
        {
            if (path.Count > 0)
            {
                path.RemoveAt(path.Count - 1);
            }
        }

        public String weightString()
        {
            String s = String.Empty;
            for (int i = 1; i < path.Count; i++)
            {
                int temp = path[i].getWeight(path[i - 1].getNum());
                s = String.Concat(s, " + ", temp.ToString());
            }
            s = String.Concat(s, " = ", getWeight());
            s = s.Substring(3);
            return s;
        }

        public int getWeight()
        {
            int weight = 0;
            for (int i = 1; i < path.Count; i++)
            {
                int temp = path[i].getWeight(path[i - 1].getNum());
                weight += temp;
            }
            return weight;
        }

        public graphNode getLast()
        {
            if (path.Count > 0)
            {
                return path[path.Count - 1];
            }
            return null;
        }

        public int CompareTo(Path path)
        {
            if (path == null)
            {
                return 1;
            }
            else
                return this.getWeight().CompareTo(path.getWeight());
        }

        public String toString()
        {
            String s = String.Empty;
            foreach (var node in path)
            {
                s = String.Concat(s, node.getNum().ToString(), " ");
            }
            s = s.Trim();
            return s;
        }
    }

    public class edges
    {
        private int nodeNum { get; set; }
        private int weight { get; set; }

        public edges(int nodeNum, int weight)
        {
            this.nodeNum = nodeNum;
            this.weight = weight;
        }

        public int getNum()
        {
            return nodeNum;
        }

        public int getWeight()
        {
            return weight;
        }
    }

    public class graphNode
    {
        private int cityNum { get; set; }
        private int visited { get; set; }
        public List<edges> adjList = new List<edges>();

        public graphNode(int cityNum)
        {
            this.cityNum = cityNum;
        }

        public void visit()
        {
            visited = 1;
        }

        public void devisit()
        {
            visited = 0;
        }

        public int getVisited()
        {
            return visited;
        }
        public void addNeighbour(int nodeNum, int weight)
        {
            adjList.Add(new edges(nodeNum, weight));
        }

        public int getNum()
        {
            return cityNum;
        }

        public int getWeight(int adjacent)
        {
            foreach (var node in adjList)
            {
                if (node.getNum() == adjacent)
                {
                    return node.getWeight();
                }
            }
            return 0;
        }
        public Boolean isNeighbour(int adjacent)
        {
            if (cityNum == adjacent)
                return true;
            foreach (var item in adjList)
            {
                if (item.getNum() == adjacent)
                    return true;
            }
            return false;
        }

        public String getAdjList()
        {
            String list = String.Empty;
            if (adjList.Count == 0)
                return "-";
            foreach (var node in adjList)
            {
                list = string.Concat(list, node.getNum(), " ");
            }
            return list;
        }

        public List<int> next()
        {
            List<int> allAdj = new List<int>();
            foreach (edges edge in adjList)
            {
                allAdj.Add(edge.getNum());
            }
            return allAdj;
        }
    }

    public class Graph
    {
        private int size { get; set; }
        public int edges { get; set; }
        public List<graphNode> graphList = new List<graphNode>();

        public void addCity()
        {
            graphList.Add(new graphNode(graphList.Count));
            size++;
        }

        public graphNode getNode(int nodeNum)
        {
            if (nodeNum < size)
            {
                return graphList[nodeNum];
            }
            else
            {
                return null;
            }
        }

        public void addEdge(int cityNum, int neighbour)
        {
            graphNode node = getNode(cityNum);
            if (!node.isNeighbour(neighbour))
            {
                var rnd = new Random();
                int weight = rnd.Next(1, 10);
                if (cityNum < size && neighbour < size)
                {
                    edges++;
                    node.addNeighbour(neighbour, weight);
                    node = getNode(neighbour);
                    node.addNeighbour(cityNum, weight);
                }
            }
        }

        public int getSize()
        {
            return size;
        }

        public void printGraph()
        {
	        Debug.LogFormat("N :{0} E:{1}", size, edges);
            foreach (var city in graphList)
            {
	            Debug.LogFormat("Vertex {0} has these as adjecent:\n{1}", city.getNum(), city.getAdjList());
            }
        }
    }
}



namespace hamiltonian
{
    public class Program
    {
        public static void Main(int N)
        {
            Paths.Graph graph = new Paths.Graph();
            constructGraph(graph, N);
            graph.printGraph();
            bool[] visited = new bool[graph.getSize()];
            Paths.Path path = new Paths.Path();
            Paths pathCol = new Paths();
            int startNode = new Random().Next(0, graph.getSize());//Select a random node to start.
            path.addNode(graph.getNode(startNode)); //Start your path with your started node.
            hamiltonianCircuits(graph, graph.graphList[startNode], visited, path, graph.graphList[startNode], pathCol);
            pathCol.printAllPaths();
            pathCol.smallestPath();
        }



        static void hamiltonianCircuits(Paths.Graph graph, Paths.graphNode start, bool[] visited, Paths.Path currentPath, Paths.graphNode init, Paths pathCol)
        {
            if (currentPath.path.Count == graph.getSize() + 1 && currentPath.getLast() == init)
            {
                List<Paths.graphNode> finalPath = new List<Paths.graphNode>();
                copyList(finalPath, currentPath);
                pathCol.addPath(new Paths.Path(finalPath));
                return;
            }
            foreach (int city in start.next())
            {
                if (!visited[city])
                {
                    visited[city] = true;
                    currentPath.addNode(graph.getNode(city));
                    hamiltonianCircuits(graph, graph.getNode(city), visited, currentPath, init, pathCol);
                    visited[city] = false;
                    currentPath.removeLast();
                }
            }
        }

        static void copyList(List<Paths.graphNode> finalPath, Paths.Path path) //Used to copy currentPath to allPaths
        {
            foreach (var item in path.path)
            {
                finalPath.Add(item);
            }
        }

        static void constructGraph(Paths.Graph graph, int N) //Creates a random graph with N vertices and E edges.
        {
            var rnd = new Random(Guid.NewGuid().GetHashCode()); //Random value with guid hash code to guarantee distinct values.
            Debug.LogFormat("Enter the number of vertexes.");
            if (N < 3)
            {
	            Debug.LogFormat("There must be atleast 3 vertexes.");
                System.Environment.Exit(0);
            }
            
            int E = rnd.Next(N, ((N * (N - 1)) / 2) + 1);
            for (int i = 0; i < N; i++)
            {
                graph.addCity();
            }
            for (int i = 0; i < N; i++) //Making sure of every node has atleast 1 adjacent node.
            {
                graph.addEdge(i, rnd.Next(N));
            }
            int j = 0;
            int size = graph.getSize();
            while (graph.edges != E) //Making sure of number of edges matches E.
            {
                if (j == size)
                    j = 0;
                graph.addEdge(j, rnd.Next(N)); //Distrubiting rest of the edges randomly.
                j++;
            }
        }
    }
}