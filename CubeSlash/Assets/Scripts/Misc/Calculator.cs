using UnityEngine;

public static class Calculator
{
    public static float DST_Distance(float speed, float time) => speed * time;
    public static float DST_Speed(float distance, float time) => distance / time;
    public static float DST_Time(float distance, float speed) => distance / speed;
}