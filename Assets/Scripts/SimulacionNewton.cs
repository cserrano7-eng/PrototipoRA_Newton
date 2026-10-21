using UnityEngine;

public class SimulacionNewton : MonoBehaviour
{
    [Header("Parámetros (RF-03)")]
    [Range(0.5f, 20f)] public float masa = 2f;      // kg
    [Range(0f, 100f)]  public float fuerza = 10f;   // N
    [Range(0f, 1f)]    public float mu = 0.1f;      // coeficiente de fricción

    [Header("Escena AR")]
    [Tooltip("Metros simulados → unidades de la escena. Ajusta para que el cubo quede dentro del marcador.")]
    public float escalaVisual = 0.02f;

    [Header("Prueba")]
    public float tiempoMaximo = 2f;

    [Header("Estado (RF-06)")]
    public bool enEjecucion = false;

    [Header("Lectura (RF-05)")]
    public float aTeorica;
    public float aSimulada;
    public float errorPorcentual;
    public float tiempo;
    public float distanciaMedida;
    public float distanciaTeorica;
    public float errorDistancia;

    private Vector3 posicionInicialLocal;
    private float velocidad;   // m/s
    private float x;           // m

    void Awake()
    {
        posicionInicialLocal = transform.localPosition;
    }

    public void Iniciar() => enEjecucion = true;
    public void Pausar()  => enEjecucion = false;

    public void Reiniciar()
    {
        enEjecucion = false;
        velocidad = 0f;
        x = 0f;
        tiempo = 0f;
        aSimulada = 0f;
        errorPorcentual = 0f;
        distanciaMedida = 0f;
        distanciaTeorica = 0f;
        errorDistancia = 0f;
        transform.localPosition = posicionInicialLocal;
    }

    void FixedUpdate()
    {
        aTeorica = ModeloFisico.Aceleracion(fuerza, masa, mu);
        if (!enEjecucion) return;

        float dt = Time.fixedDeltaTime;
        float velocidadAnterior = velocidad;

        // Euler semiimplícito (modelo en metros)
        velocidad += aTeorica * dt;
        x += velocidad * dt;
        tiempo += dt;

        aSimulada = (velocidad - velocidadAnterior) / dt;
        errorPorcentual = ModeloFisico.ErrorPorcentual(aSimulada, aTeorica);

        distanciaMedida = x;
        distanciaTeorica = 0.5f * aTeorica * tiempo * tiempo;
        errorDistancia = ModeloFisico.ErrorPorcentual(distanciaMedida, distanciaTeorica);

        // Representación visual dentro del marcador
        transform.localPosition = posicionInicialLocal + Vector3.right * x * escalaVisual;

        if (tiempo >= tiempoMaximo) Pausar();
    }
}