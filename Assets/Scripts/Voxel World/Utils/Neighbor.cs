using UnityEngine;

public static class Neighbor
{
    public static Vector2 LEFT = new Vector2(-1, 0);
    public static Vector2 RIGHT = new Vector2(1, 0);
    public static Vector2 TOP = new Vector2(0, 1);
    public static Vector2 BOTTOM = new Vector2(0, -1);

    public static Vector2 TOP_LEFT = TOP + LEFT;
    public static Vector2 TOP_RIGHT = TOP + RIGHT;
    public static Vector2 BOTTOM_LEFT = BOTTOM + LEFT;
    public static Vector2 BOTTOM_RIGHT = BOTTOM + RIGHT;
}