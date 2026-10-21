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
        bool detectado = estado.Status == Status.TRACKED || estado.Status == Status.EXTENDED_TRACKED;
        if (!detectado && simulacion != null)
        {
            simulacion.Pausar(); // si se pierde el marcador, la simulación se pausa
        }
    }
}