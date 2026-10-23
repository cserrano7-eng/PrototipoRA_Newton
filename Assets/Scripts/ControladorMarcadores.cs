using UnityEngine;
using Vuforia;

public class ControladorMarcadores : MonoBehaviour
{
    [SerializeField] private ObserverBehaviour marcador;
    [SerializeField] private SimulacionNewton simulacion;
    [SerializeField] private PanelSimulacion panel;

    private void OnEnable()
    {
        if (marcador != null) marcador.OnTargetStatusChanged += AlCambiarEstado;
    }

    private void OnDisable()
    {
        if (marcador != null) marcador.OnTargetStatusChanged -= AlCambiarEstado;
    }

    private void AlCambiarEstado(ObserverBehaviour ob, TargetStatus estado)
    {
        // Solo cuenta como detectado cuando la cámara ve el marcador directamente
        bool detectado = estado.Status == Status.TRACKED;

        if (detectado)
        {
            if (panel != null) panel.SetSimulacion(simulacion); // el panel controla este escenario
        }
        else
        {
            if (simulacion != null) simulacion.Pausar();         // RF-02: se pausa al perder el marcador
            if (panel != null) panel.Liberar(simulacion);
        }
    }
}