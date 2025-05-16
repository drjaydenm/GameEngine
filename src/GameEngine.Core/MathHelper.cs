namespace GameEngine.Core
{
    public static class MathHelper
    {
        public static float Lerp(float start, float end, float t)
        {
            return start + t * (end - start);
        }

        public static float InverseLerp(float start, float end, float value)
        {
            return (value - start) / (end - start);
        }
    }
}
