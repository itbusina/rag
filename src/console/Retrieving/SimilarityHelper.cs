namespace console.Retrieving
{
    public static class SimilarityHelper
    {
        public static double CosineSimilarity(float[] v1, float[] v2)
        {
            double dot = 0, norm1 = 0, norm2 = 0;
            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                norm1 += v1[i] * v1[i];
                norm2 += v2[i] * v2[i];
            }
            return dot / (Math.Sqrt(norm1) * Math.Sqrt(norm2));
        }
    }
}