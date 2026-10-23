using UnityEngine;

public class PanelSimulacion : MonoBehaviour
{
    [SerializeField] private SimulacionNewton simulacion;   // se asigna sola al detectar un marcador
    [SerializeField] private int tamanioTexto = 18;
    [SerializeField] private int tamanioBoton = 22;
    [SerializeField] private int altoDeslizador = 28;
    [Tooltip("Fracción del ancho de pantalla que ocupa el panel (0,4 = 40 %)")]
    [SerializeField, Range(0.2f, 1f)] private float anchoPanel = 0.4f;
    [SerializeField] private Color colorDestacado = new Color(1f, 0.85f, 0.2f);
    private bool estiloListo = false;

    public void SetSimulacion(SimulacionNewton s) => simulacion = s;
    public void Liberar(SimulacionNewton s) { if (simulacion == s) simulacion = null; }

    void OnGUI()
    {
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

        if (simulacion == null)
        {
            GUILayout.Label("Segunda ley de Newton");
            GUILayout.Label("Apunte la cámara a un marcador (M1, M2 o M3).");
            GUILayout.EndArea();
            return;
        }

        var s = simulacion;
        GUILayout.Label(s.nombreEscenario);

        // RF-03: deslizadores, activos solo antes de iniciar o en pausa; la variable principal va destacada
        GUI.enabled = !s.enEjecucion;
        float m = Deslizador($"Masa  m = {s.masa:F1} kg", s.masa, s.masaMin, s.masaMax,
                             s.variablePrincipal == SimulacionNewton.VariablePrincipal.Masa);
        float F = Deslizador($"Fuerza  F = {s.fuerza:F0} N", s.fuerza, s.fuerzaMin, s.fuerzaMax,
                             s.variablePrincipal == SimulacionNewton.VariablePrincipal.Fuerza);
        float u = Deslizador($"Fricción  μ = {s.mu:F2}", s.mu, s.muMin, s.muMax,
                             s.variablePrincipal == SimulacionNewton.VariablePrincipal.Friccion);
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

        // RF-06
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Iniciar")) s.Iniciar();
        if (GUILayout.Button("Pausar")) s.Pausar();
        if (GUILayout.Button("Reiniciar")) s.Reiniciar();
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    float Deslizador(string etiqueta, float valor, float min, float max, bool principal)
    {
        var estilo = new GUIStyle(GUI.skin.label);
        if (principal) { estilo.fontStyle = FontStyle.Bold; estilo.normal.textColor = colorDestacado; }
        GUILayout.Label((principal ? "▶ " : "") + etiqueta, estilo);
        return GUILayout.HorizontalSlider(valor, min, max);
    }
}