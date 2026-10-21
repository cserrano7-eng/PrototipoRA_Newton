using System;

public static class ModeloFisico
{
    public const float Gravedad = 9.81f; // m/s²

    // N = m·g
    public static float Normal(float masa) => masa * Gravedad;

    // f_k = μ·N
    public static float Friccion(float mu, float masa) => mu * Normal(masa);

    // F_neta = F − f_k si F > f_k; si no, 0 (el objeto no se mueve)
    public static float FuerzaNeta(float fuerza, float masa, float mu)
    {
        float fr = Friccion(mu, masa);
        return fuerza > fr ? fuerza - fr : 0f;
    }

    // a = F_neta / m
    public static float Aceleracion(float fuerza, float masa, float mu)
    {
        if (masa <= 0f) return 0f;
        return FuerzaNeta(fuerza, masa, mu) / masa;
    }

    // E% = |a_sim − a_teo| / |a_teo| × 100
    public static float ErrorPorcentual(float simulada, float teorica)
    {
        if (Math.Abs(teorica) < 1e-6f) return 0f;
        return Math.Abs(simulada - teorica) / Math.Abs(teorica) * 100f;
    }
}