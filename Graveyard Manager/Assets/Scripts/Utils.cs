using UnityEngine;

public class Utils
{
    /// <summary>
    /// Generate a random number between low and high with a bias
    /// </summary>
    /// <param name="low">Low bound</param>
    /// <param name="high">High bound</param>
    /// <param name="bias">if less than 1 : the average is closer to low bound
    ///                    if greater than 1 : the average is closet to high bound
    ///                    if equal 1 : same as Random.Range()
    /// </param>
    /// <returns></returns>
    public static float RandomBiased(float low, float high, float bias)
    {
        float r = Random.Range(0f, 1f);
        r = Mathf.Pow(r, bias);
        return low + (high - low) * r;
    }

    /// <summary>
    /// A function that has an S shape. f(0) = 0; f(1) = 1; f(0.5) = 0.5;
    /// f( a in ]0;0.5[ ) is below y = x
    /// f ( a in ]0.5;1[ ) is above y = x
    /// </summary>
    /// <param name="x"></param>
    /// <param name="beta"></param>
    /// <returns></returns>
    public static float SCurve(float x, float beta)
    {
        return 1f / (1f + (Mathf.Pow(x / (1f - x), -beta)));
    }
}
