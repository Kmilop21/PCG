using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using static UnityEngine.RuleTile.TilingRuleOutput;


public struct BinarySpacePartitionTree 
{
    private class Node
    {
        public Rect Area;
        public Node Left;
        public Node Right;

        public Node(Rect area)
        {
            Area = area;
            Left = null;
            Right = null;
        }

        public void OnDrawGizmos()
        {
            Vector2[] lines = new Vector2[4];
            Vector2 pos = Area.position;
            lines[0] = pos;
            lines[1] = pos + Vector2.up * Area.height;
            lines[3] = pos + Vector2.right * Area.width;
            lines[2] = pos + Area.size;

            Gizmos.DrawLine(lines[0], lines[1]);
            Gizmos.DrawLine(lines[1], lines[2]);
            Gizmos.DrawLine(lines[2], lines[3]);
            Gizmos.DrawLine(lines[3], lines[0]);
        }
    }


    private Vector2 minSize;
    private Node root;

    private float MinArea => minSize.x * minSize.y;

    private BinarySpacePartitionTree(Rect area, Vector2 minSize)
    {
        root = new Node(area);
        this.minSize = minSize;
    }
    public void OnDrawGizmos()
    {
        Vector2 maxSize = root.Area.size; 

        static void draw(Node current)
        {
            if(current == null)
                return;

            current.OnDrawGizmos();
            draw(current.Left);
            draw(current.Right);
        }

        draw(root);
    }

    private void CreateSubAreas(Rect current, out Rect left, out Rect right)
    {
        Vector2 pos = current.position;
        Vector2 size = current.size;
        do
        {
            if (Random.value >= 0.5f)
            {
                float x = Random.Range(minSize.x, size.x - minSize.x);
                left = new Rect(pos, new Vector2(x, size.y));
                right = new Rect(new Vector2(pos.x + x, pos.y), new Vector2(size.x - x, size.y));
            }
            else
            {
                float y = Random.Range(minSize.y, size.y - minSize.y);
                left = new Rect(pos, new Vector2(size.x, y));
                right = new Rect(new Vector2(pos.x, pos.y + y), new Vector2(size.x, size.y - y));
            }
        } while (GetArea(left) < MinArea || GetArea(right) < MinArea);
    }

    private bool IsInvalidArea(Rect area)
    {
        return GetArea(area) < 2 * MinArea || area.size.x < 2 * minSize.x || area.size.y < 2 * minSize.y;
    }
    
    private void SplitToLimit(Node current)
    {
        if (IsInvalidArea(current.Area))
            return;

        CreateSubAreas(current.Area, out Rect left, out Rect right);

        current.Left = new Node(left);
        SplitToLimit(current.Left);
        current.Right = new Node(right);
        SplitToLimit(current.Right);
    }

    private void Split(Node current, int iteration)
    {
        if (iteration <= 0 || IsInvalidArea(current.Area))
            return;

        CreateSubAreas(current.Area, out Rect left, out Rect right);
        current.Left = new Node(left);
        Split(current.Left, iteration - 1);
        current.Right = new Node(right);
        Split(current.Right, iteration - 1);
    }

    public static BinarySpacePartitionTree Generate(Rect area, Vector2 size, int iteration = -10)
    {
        static void reajust(Node current, Vector2 offSet)
        {
            if(current == null)
                return;

            current.Area = new Rect(current.Area.position - offSet, current.Area.size);

            reajust(current.Left, offSet);
            reajust(current.Right, offSet);
        }
        
        BinarySpacePartitionTree tree = new BinarySpacePartitionTree(area, size);
        if(iteration == -10)
            tree.SplitToLimit(tree.root);
        else
            tree.Split(tree.root, iteration);

        reajust(tree.root, tree.root.Area.size / 2);

        return tree;
    }

    public static float GetArea(Rect area)
    {
        return Mathf.Abs((area.xMax - area.xMin) * (area.yMax - area.yMin));
    }
}
