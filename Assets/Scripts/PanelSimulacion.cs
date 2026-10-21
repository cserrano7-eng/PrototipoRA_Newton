using UnityEngine;

public class PanelSimulacion : MonoBehaviour
{
    [SerializeField] private SimulacionNewton simulacion;
    [SerializeField] private int tamanioTexto = 18;
    [SerializeField] private int tamanioBoton = 22;
    private bool estiloListo = false;

    void OnGUI()
    {
        if (simulacion == null) return;

        if (!estiloListo)
        {
            GUI.skin.label.fontSize = tamanioTexto;
            GUI.skin.button.fontSize = tamanioBoton;
            estiloListo = true;
        }

        GUILayout.BeginArea(new Rect(20, 20, Screen.width - 40, Screen.height - 40));
        GUILayout.Label("Segunda ley de Newton");
        GUILayout.Label($"m = {simulacion.masa:F1} kg   F = {simulacion.fuerza:F1} N   μ = {simulacion.mu:F2}");
        GUILayout.Label($"a teórica: {simulacion.aTeorica:F3} m/s²");
        GUILayout.Label($"a simulada: {simulacion.aSimulada:F3} m/s²");
        GUILayout.Label($"Distancia: {simulacion.distanciaMedida:F3} m (teórica {simulacion.distanciaTeorica:F3} m)");
        GUILayout.Label($"Error distancia: {simulacion.errorDistancia:F2} %");

        if (GUILayout.Button("Iniciar")) simulacion.Iniciar();
        if (GUILayout.Button("Pausar")) simulacion.Pausar();
        if (GUILayout.Button("Reiniciar")) simulacion.Reiniciar();
        GUILayout.EndArea();
    }
}