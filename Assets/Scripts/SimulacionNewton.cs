using UnityEngine;

public class SimulacionNewton : MonoBehaviour
{
    public enum VariablePrincipal { Ninguna, Fuerza, Masa, Friccion }

    [Header("Escenario (marcador)")]
    public string nombreEscenario = "Segunda ley de Newton";
    public VariablePrincipal variablePrincipal = VariablePrincipal.Ninguna;

    [Header("Parámetros (RF-03)")]
    public float masa = 5f;     // kg
    public float fuerza = 20f;  // N
    public float mu = 0f;       // coeficiente de fricción cinética

    [Header("Rangos y pasos de los deslizadores")]
    public float masaMin = 1f, masaMax = 20f, pasoMasa = 0.5f;
    public float fuerzaMin = 0f, fuerzaMax = 100f, pasoFuerza = 1f;
    public float muMin = 0f, muMax = 0.6f, pasoMu = 0.05f;

    [Header("Pista y tiempo")]
    public float longitudPista = 5f;  // m
    public float tiempoMaximo = 15f;  // s

    [Header("Escena AR")]
    public float escalaVisual = 0.028f; // 14 cm de pista / 5 m

    [Header("Estado (RF-06)")]
    public bool enEjecucion = false;
    public bool finalizada = false;

    [Header("Lecturas (RF-05)")]
    public float aTeorica, aSimulada, errorPorcentual;
    public float velocidad, tiempo;
    public float distanciaMedida, distanciaTeorica, errorDistancia;
    public float tiempoLlegada, tiempoLlegadaTeorico, errorTiempo;

    private Vector3 posicionInicialLocal;

    void Awake()
    {
        posicionInicialLocal = transform.localPosition;
        ActualizarTeoricos();
    }

    void OnValidate()
    {
        masa = Ajustar(masa, masaMin, masaMax, pasoMasa);
        fuerza = Ajustar(fuerza, fuerzaMin, fuerzaMax, pasoFuerza);
        mu = Ajustar(mu, muMin, muMax, pasoMu);
        ActualizarTeoricos();
    }

    static float Ajustar(float valor, float min, float max, float paso)
    {
        float v = Mathf.Clamp(valor, min, max);
        return paso > 0f ? Mathf.Round(v / paso) * paso : v;
    }

    void ActualizarTeoricos()
    {
        aTeorica = ModeloFisico.Aceleracion(fuerza, masa, mu);
        tiempoLlegadaTeorico = aTeorica > 0f ? Mathf.Sqrt(2f * longitudPista / aTeorica) : 0f;
    }

    /// RF-03: cambia los parámetros solo antes de iniciar o en pausa.
    /// Si algún valor cambia, la simulación vuelve a cero para que la comparación con el valor teórico sea válida.
    public bool CambiarParametros(float m, float F, float coef)
    {
        if (enEjecucion) return false;
        float nm = Ajustar(m, masaMin, masaMax, pasoMasa);
        float nF = Ajustar(F, fuerzaMin, fuerzaMax, pasoFuerza);
        float nu = Ajustar(coef, muMin, muMax, pasoMu);
        if (Mathf.Approximately(nm, masa) && Mathf.Approximately(nF, fuerza) && Mathf.Approximately(nu, mu))
            return false;
        masa = nm; fuerza = nF; mu = nu;
        Reiniciar();
        return true;
    }

    public void Iniciar()
    {
        if (finalizada) return; // terminó: hay que reiniciar
        enEjecucion = true;
    }

    public void Pausar() => enEjecucion = false;

    public void Reiniciar()
    {
        enEjecucion = false;
        finalizada = false;
        velocidad = tiempo = 0f;
        aSimulada = errorPorcentual = 0f;
        distanciaMedida = distanciaTeorica = errorDistancia = 0f;
        tiempoLlegada = errorTiempo = 0f;
        ActualizarTeoricos();
        transform.localPosition = posicionInicialLocal;
    }

    void FixedUpdate()
    {
        if (!enEjecucion) return;

        float dt = Time.fixedDeltaTime;
        float a = ModeloFisico.Aceleracion(fuerza, masa, mu);
        float vAnterior = velocidad;
        float xAnterior = distanciaMedida;

        // Euler semiimplícito: primero la velocidad, luego la posición
        velocidad += a * dt;
        distanciaMedida += velocidad * dt;
        tiempo += dt;

        aSimulada = (velocidad - vAnterior) / dt;
        errorPorcentual = ModeloFisico.ErrorPorcentual(aSimulada, aTeorica);

        if (distanciaMedida >= longitudPista)
        {
            // Instante de llegada interpolado dentro del último paso
            float fraccion = (longitudPista - xAnterior) / (distanciaMedida - xAnterior);
            tiempoLlegada = tiempo - dt + fraccion * dt;
            tiempo = tiempoLlegada;
            distanciaMedida = longitudPista;
            errorTiempo = ModeloFisico.ErrorPorcentual(tiempoLlegada, tiempoLlegadaTeorico);
            Finalizar();
        }
        else if (tiempo >= tiempoMaximo)
        {
            Finalizar();
        }

        distanciaTeorica = 0.5f * aTeorica * tiempo * tiempo;
        errorDistancia = ModeloFisico.ErrorPorcentual(distanciaMedida, distanciaTeorica);
        transform.localPosition = posicionInicialLocal + Vector3.right * distanciaMedida * escalaVisual;
    }

    void Finalizar()
    {
        enEjecucion = false;
        finalizada = true;
    }
}