using UnityEngine;
using Vuforia;

public class ControladorMarcadores : MonoBehaviour
{
    [SerializeField] private ObserverBehaviour marcador;
    [SerializeField] private SimulacionNewton simulacion;

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
        // Solo se considera detectado cuando la cámara ve el marcador directamente.
        // EXTENDED_TRACKED significa que Vuforia ya no lo ve y solo estima su posición,
        // por eso en ese estado también se pausa la simulación.
        bool detectado = estado.Status == Status.TRACKED;

        if (!detectado && simulacion != null)
        {
            simulacion.Pausar(); // si se pierde el marcador, la simulación se pausa
        }
    }
}