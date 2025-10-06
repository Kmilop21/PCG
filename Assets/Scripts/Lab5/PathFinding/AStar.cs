using Container;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Foike.PathFinding
{
    public static class Search
    {
        /// <summary>
        /// An A* state capable to iterate through its children "states"
        /// </summary>
        public interface IState : IEnumerable<IState>, IEquatable<IState>
        {
            bool IsValid { get; }
            float Cost { get; }
        }

        public delegate float SingleStateHeuristic<TState>(TState state) where TState : IState;
        public delegate float Heuristic<TState>(TState state, TState goal) where
            TState : IState;

        public delegate bool Goal<TState>(TState state) where TState : IState;
        private class Node<TState> : IComparer<Node<TState>> where TState : IState
        {
            //g(n)
            public float AccumulatedCost;
            //h(n)
            public float EstimatedCost;
            public float TotalCost { get { return AccumulatedCost + EstimatedCost; } }
            public TState State;
            public Node<TState> Parent { private set; get; }

            public Node()
            {
                AccumulatedCost = 0;
                EstimatedCost = 0;
                Parent = null;
                State = default;
            }

            public Node(TState position, float accumulatedCost, float estimatedCost, 
                Node<TState> previous = null) 
                : this()
            {
                State = position;
                AccumulatedCost = accumulatedCost;
                EstimatedCost = estimatedCost;
                Parent = previous;
            }

            public int Compare(Node<TState> x, Node<TState> y)
            {
                if (x.TotalCost > y.TotalCost)
                    return -1;
                if (x.TotalCost < y.TotalCost)
                    return 1;
                return 0;
            }
        }

        public static float ManhattanDistance(Vector3 point1, Vector3 point2)
        {
            return Mathf.Abs(point1.x - point2.x) + Mathf.Abs(point1.y - point2.y) 
                + Mathf.Abs(point1.z - point2.z);
        }

        public static Stack<TState> AStar<TState>(TState start, TState goal,
            Heuristic<TState> heuristic) where TState : IState
        {
            return AStar(start, goal, heuristic, (state) => state.Equals(goal));
        }
        public static Stack<TState> AStar<TState>(TState start, SingleStateHeuristic<TState> heuristic,
            Goal<TState> evaluate) where TState: IState
        {
            return AStar(start, default, (state, goal) => heuristic(state), evaluate);
        }
        private static Stack<TState> AStar<TState>(TState start, TState goal,
            Heuristic<TState> heuristic, Goal<TState> evaluate) where TState : IState
        {
            PriorityQueue<Node<TState>> Open = new();
            Open.Enqueue(new Node<TState>(start, 0, heuristic(start, goal)));
            List<Node<TState>> Close = new();
            Stack<TState> stack = new Stack<TState>();

            bool ignoreNewGByOpen(float newG, TState child)
            {
                foreach (Node<TState> node in Open)
                {
                    if (node.State.Equals(child))
                    {
                        if (node.AccumulatedCost <= newG)
                            return true;
                        return false;
                    }
                }
                return false;
            }

            bool ignoreNewGByClose(float newG, TState child, ref List<Node<TState>> reopen)
            {
                foreach (Node<TState> node in Close)
                {
                    if (node.State.Equals(child))
                    {
                        if (node.AccumulatedCost <= newG)
                            return true;
                        else
                            reopen.Add(node);
                    }
                }

                return false;
            }

            while (Open.Count != 0)
            {
                Node<TState> node = Open.Top;
                Open.Dequeue();

                if (evaluate(node.State))
                {
                    Node<TState> aux = node;

                    while (aux.Parent != null)
                    {
                        stack.Push(aux.State);
                        aux = aux.Parent;
                    }
                    return stack;
                }


                foreach (IState state in node.State)
                {
                    if (state.IsValid && state is TState child)
                    {
                        float newG = node.AccumulatedCost + child.Cost;
                        List<Node<TState>> reopen = new List<Node<TState>>();

                        if (ignoreNewGByOpen(newG, child) || ignoreNewGByClose(newG, child, ref reopen))
                            continue;

                        foreach (Node<TState> c in reopen)
                            Close.Remove(c);

                        Open.Enqueue(new Node<TState>(child, newG, heuristic(child, goal), node));
                    }
                }

                Close.Add(node);
            }

            Debug.Log("Not found");
            return stack;
        }

        public static Stack<TState> IDAStar<TState>(TState start, TState goal, 
            Heuristic<TState> heuristic) where TState : IState
        {
            return IDAStar(start, goal, heuristic, (state) => state.Equals(goal));
        }
        public static Stack<TState> IDAStar<TState>(TState start, SingleStateHeuristic<TState> heuristic,
            Goal<TState> evaluate) where TState : IState
        {
            return IDAStar(start, default, (state, goal) => heuristic(state), evaluate);
        }
        private static Stack<TState> IDAStar<TState>(TState start, TState goal,
            Heuristic<TState> heuristic, Goal<TState> evaluate) where TState : IState
        {
            Node<TState> search(Node<TState> current, float threshold)
            {
                if (current.TotalCost > threshold || evaluate(current.State))
                    return current;

                Node<TState> mininum = null;

                foreach (IState state in current.State)
                {
                    if (state.IsValid && state is TState child)
                    {
                        float newG = current.AccumulatedCost + child.Cost;
                        Node<TState> node = new Node<TState>(child, newG, heuristic(child, goal), current);
                        Node<TState> result = search(node, threshold);
                        if (result != null && evaluate(result.State))
                            return result;
                        if (mininum == null || (result != null && result.TotalCost < mininum.TotalCost))
                            mininum = result;
                    }
                }

                return mininum;
            }

            Stack<TState> path = new Stack<TState>();
            Node<TState> node = new Node<TState>(start, 0, heuristic(start, goal));
            float threshold = node.TotalCost;
            while (true)
            {
                Node<TState> result = search(node, threshold);

                if (evaluate(result.State))
                {
                    Node<TState> aux = result;
                    path.Push(aux.State);
                    while (aux.Parent != null)
                    {
                        path.Push(aux.State);
                        aux = aux.Parent;
                    }
                    return path;
                }

                if (result == null)
                    return path;

                threshold = result.TotalCost;
            }
        }
        public static Stack<TState> AStarPath<TState>(this TState start, TState goal,
            Heuristic<TState> heuristic) where TState : IState
        {
            return AStar(start, goal, heuristic);
        }

        public static Stack<TState> AStarPath<TState>(this TState start, 
            SingleStateHeuristic<TState> heuristic, Goal<TState> evaluate) where TState : IState
        {
            return AStar(start, heuristic, evaluate);
        }

        public static Stack<TState> IDAStarPath<TState>(this TState start, TState goal,
            Heuristic<TState> heuristic) where TState : IState
        {
            return IDAStar(start, goal, heuristic);
        }

        public static Stack<TState> IDAStarPath<TState>(this TState start, 
            SingleStateHeuristic<TState> heuristic, Goal<TState> evaluate) where TState : IState
        {
            return IDAStar(start, heuristic, evaluate);
        }
    }
}
