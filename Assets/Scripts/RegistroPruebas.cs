using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Vuforia;

/// RNF-03 y RNF-04: mide los cuadros por segundo durante la simulación, el tiempo de arranque,
/// la memoria y el estado de la red, y los muestra en un panel en pantalla.
public class RegistroPruebas : MonoBehaviour
{
    [Header("Rendimiento")]
    [Tooltip("En Android, Unity limita a 30 FPS si no se indica otro valor.")]
    [SerializeField] private int fpsObjetivo = 60;

    [Header("Panel")]
    [SerializeField] private bool mostrarPanel = true;
    [SerializeField] private int tamanioTexto = 18;
    [SerializeField, Range(0.2f, 0.6f)] private float anchoPanel = 0.36f;

    private SimulacionNewton[] simulaciones;
    private readonly List<float> tiemposCuadro = new List<float>(); // segundos por cuadro, solo durante la simulación
    private float acumulado; private int cuadrosVentana; private float fpsActual;
    private float tPrimerCuadro = -1f, tVuforiaLista = -1f;
    private float tRecalculo;
    // Resultados en caché (se recalculan una vez por segundo para no afectar la medición)
    private float fpsPromedio, fpsMinimo, fps95, segundosMedidos; private int cuadrosMedidos;
    private long memoriaMB;
    private GUIStyle estilo, estiloBoton;

    void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fpsObjetivo;
    }

    void Start()
    {
        simulaciones = FindObjectsByType<SimulacionNewton>(FindObjectsSortMode.None);
        VuforiaApplication.Instance.OnVuforiaStarted += AlIniciarVuforia;
        if (VuforiaApplication.Instance.IsRunning) AlIniciarVuforia();
    }

    void OnDestroy()
    {
        if (VuforiaApplication.Instance != null)
            VuforiaApplication.Instance.OnVuforiaStarted -= AlIniciarVuforia;
    }

    void AlIniciarVuforia()
    {
        if (tVuforiaLista < 0f) tVuforiaLista = Time.realtimeSinceStartup;
    }

    void Update()
    {
        if (tPrimerCuadro < 0f) { tPrimerCuadro = Time.realtimeSinceStartup; return; } // el primer cuadro no se mide

        float dt = Time.unscaledDeltaTime;
        acumulado += dt; cuadrosVentana++;
        if (acumulado >= 0.5f) { fpsActual = cuadrosVentana / acumulado; acumulado = 0f; cuadrosVentana = 0; }

        if (HaySimulacionEnCurso() && tiemposCuadro.Count < 100000) tiemposCuadro.Add(dt);

        if (Time.unscaledTime >= tRecalculo) { Calcular(); tRecalculo = Time.unscaledTime + 1f; }
    }

    bool HaySimulacionEnCurso()
    {
        foreach (var s in simulaciones) if (s != null && s.enEjecucion) return true;
        return false;
    }

    void Calcular()
    {
        memoriaMB = Profiler.GetTotalReservedMemoryLong() / (1024 * 1024);
        cuadrosMedidos = tiemposCuadro.Count;
        if (cuadrosMedidos == 0) { fpsPromedio = fpsMinimo = fps95 = segundosMedidos = 0f; return; }

        float total = 0f, peor = 0f;
        foreach (float t in tiemposCuadro) { total += t; if (t > peor) peor = t; }
        segundosMedidos = total;
        fpsPromedio = cuadrosMedidos / total;
        fpsMinimo = 1f / peor;

        var orden = new List<float>(tiemposCuadro); orden.Sort();
        float t95 = orden[Mathf.Clamp(Mathf.CeilToInt(0.95f * cuadrosMedidos) - 1, 0, cuadrosMedidos - 1)];
        fps95 = 1f / t95; // el 95 % de los cuadros se dibujó a esta tasa o más
    }

    public void ReiniciarMedicion() { tiemposCuadro.Clear(); Calcular(); }

    string EstadoRed()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable: return "Sin conexión";
            case NetworkReachability.ReachableViaCarrierDataNetwork: return "Datos móviles";
            default: return "Wi-Fi";
        }
    }

    void OnGUI()
    {
        if (estilo == null)
        {
            estilo = new GUIStyle(GUI.skin.label) { fontSize = tamanioTexto };
            estiloBoton = new GUIStyle(GUI.skin.button) { fontSize = tamanioTexto };
        }

        float ancho = Screen.width * anchoPanel;
        float x = Screen.width - ancho - 20;

        if (!mostrarPanel)
        {
            if (GUI.Button(new Rect(Screen.width - 140, 20, 120, tamanioTexto * 2.2f), "FPS", estiloBoton)) mostrarPanel = true;
            return;
        }

        float alto = tamanioTexto * 15f;
        GUI.Box(new Rect(x - 10, 10, ancho + 20, alto), GUIContent.none);
        GUILayout.BeginArea(new Rect(x, 20, ancho, alto));
        GUILayout.Label("Rendimiento (RNF-03)", estilo);
        GUILayout.Label($"FPS actual: {fpsActual:F0}  (objetivo {fpsObjetivo})", estilo);
        GUILayout.Label($"Simulación: prom. {fpsPromedio:F1} · mín. {fpsMinimo:F0} · 95 % ≥ {fps95:F0}", estilo);
        GUILayout.Label($"Medido: {cuadrosMedidos} cuadros en {segundosMedidos:F1} s", estilo);
        GUILayout.Label($"Arranque: primer cuadro {tPrimerCuadro:F1} s · Vuforia lista {(tVuforiaLista < 0 ? "—" : tVuforiaLista.ToString("F1"))} s", estilo);
        GUILayout.Label($"Memoria: {memoriaMB} MB · RAM del equipo: {SystemInfo.systemMemorySize} MB", estilo);
        GUILayout.Label($"Red (RNF-04): {EstadoRed()}", estilo);
        GUILayout.Label($"{SystemInfo.deviceModel} · {SystemInfo.operatingSystem}", estilo);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Reiniciar medición", estiloBoton)) ReiniciarMedicion();
        if (GUILayout.Button("Ocultar", estiloBoton)) mostrarPanel = false;
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
}