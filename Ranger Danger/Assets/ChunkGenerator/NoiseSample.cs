using UnityEngine;

public struct NoiseSample
{
    public float value;
    public Vector3 derivative;
    public Vector3 point;
    public Vector3 point0;

    public static NoiseSample operator +(NoiseSample a, float b)
    {
        a.value += b;
        return a;
    }
    public static NoiseSample operator +(float a, NoiseSample b)
    {
        b.value += a;
        return b;
    }
    public static NoiseSample operator +(NoiseSample a, NoiseSample b)
    {
        a.value += b.value;
        a.derivative += b.derivative;
        return a;
    }

    public static NoiseSample operator -(NoiseSample a, float b)
    {
        a.value -= b;
        return a;
    }
    public static NoiseSample operator -(float a, NoiseSample b)
    {
        b.value = a - b.value;
        b.derivative = -b.derivative;
        return b;
    }
    public static NoiseSample operator -(NoiseSample a, NoiseSample b)
    {
        a.value -= b.value;
        a.derivative -= b.derivative;
        return a;
    }

    public static NoiseSample operator *(NoiseSample a, float b)
    {
        a.value *= b;
        a.derivative *= b;
        return a;
    }
    public static NoiseSample operator *(float a, NoiseSample b)
    {
        b.value *= a;
        b.derivative *= a;
        return b;
    }
    public static NoiseSample operator *(NoiseSample a, NoiseSample b)
    {
        a.value *= b.value;
        a.derivative = a.value * b.derivative + b.value * a.derivative;
        return a;
    }
}
