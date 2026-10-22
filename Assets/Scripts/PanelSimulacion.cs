using UnityEngine;

public class PanelSimulacion : MonoBehaviour
{
    [SerializeField] private SimulacionNewton simulacion;
    [SerializeField] private int tamanioTexto = 18;
    [SerializeField] private int tamanioBoton = 22;
    [SerializeField] private int altoDeslizador = 28;
    [Tooltip("Fracción del ancho de pantalla que ocupa el panel (0,4 = 40 %)")]
    [SerializeField, Range(0.2f, 1f)] private float anchoPanel = 0.4f;
    private bool estiloListo = false;

    void OnGUI()
    {
        if (simulacion == null) return;
        var s = simulacion;

        if (!estiloListo)
        {
            GUI.skin.label.fontSize = tamanioTexto;
            GUI.skin.button.fontSize = tamanioBoton;
            GUI.skin.horizontalSlider.fixedHeight = altoDeslizador;
            GUI.skin.horizontalSliderThumb.fixedHeight = altoDeslizador;
            GUI.skin.horizontalSliderThumb.fixedWidth = altoDeslizador;
            estiloListo = true;
        }

        GUILayout.BeginArea(new Rect(20, 20, Screen.width * anchoPanel, Screen.height - 40));
        GUILayout.Label("Segunda ley de Newton");

        // RF-03: deslizadores, activos solo antes de iniciar o con la simulación en pausa
        GUI.enabled = !s.enEjecucion;
        GUILayout.Label($"Masa  m = {s.masa:F1} kg");
        float m = GUILayout.HorizontalSlider(s.masa, s.masaMin, s.masaMax);
        GUILayout.Label($"Fuerza  F = {s.fuerza:F0} N");
        float F = GUILayout.HorizontalSlider(s.fuerza, s.fuerzaMin, s.fuerzaMax);
        GUILayout.Label($"Fricción  μ = {s.mu:F2}");
        float u = GUILayout.HorizontalSlider(s.mu, s.muMin, s.muMax);
        GUI.enabled = true;
        s.CambiarParametros(m, F, u);

        // RF-05: resultados con dos decimales y unidades
        float fMax = ModeloFisico.Friccion(s.mu, s.masa);
        float f = s.fuerza > fMax ? fMax : s.fuerza;
        GUILayout.Label($"a teórica = {s.aTeorica:F2} m/s²   a simulada = {s.aSimulada:F2} m/s²");
        GUILayout.Label($"F neta = {ModeloFisico.FuerzaNeta(s.fuerza, s.masa, s.mu):F2} N   f = {f:F2} N");
        GUILayout.Label($"t = {s.tiempo:F2} s   v = {s.velocidad:F2} m/s   x = {s.distanciaMedida:F2} m");

        if (s.tiempoLlegada > 0f)
            GUILayout.Label($"Llegó a {s.longitudPista:F0} m en {s.tiempoLlegada:F2} s (teórico {s.tiempoLlegadaTeorico:F2} s; error {s.errorTiempo:F2} %)");
        else if (s.finalizada)
            GUILayout.Label("Tiempo máximo alcanzado: el bloque no llegó al final de la pista");
        else
            GUILayout.Label($"x teórica = {s.distanciaTeorica:F2} m   error x = {s.errorDistancia:F2} %");

        // RF-06: botones en una sola fila para ahorrar espacio
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Iniciar")) s.Iniciar();
        if (GUILayout.Button("Pausar")) s.Pausar();
        if (GUILayout.Button("Reiniciar")) s.Reiniciar();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}