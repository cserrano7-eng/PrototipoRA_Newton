using UnityEngine;

[RequireComponent(typeof(SimulacionNewton))]
public class VectoresFuerza : MonoBehaviour
{
    [Header("Escala de las flechas")]
    [Tooltip("Metros de flecha por cada newton de fuerza")]
    [SerializeField] private float metrosPorNewton = 0.004f;
    [SerializeField] private float largoMaximo = 0.3f;
    [SerializeField] private float grosor = 0.004f;

    [Header("Qué mostrar")]
    [SerializeField] private bool mostrarPesoYNormal = true;
    [SerializeField] private bool mostrarEtiquetas = true;
    [SerializeField] private int tamanioEtiqueta = 28;

    [Header("Colores")]
    [SerializeField] private Color colorAplicada = new Color(0.20f, 0.50f, 1.00f); // azul
    [SerializeField] private Color colorFriccion = new Color(1.00f, 0.25f, 0.20f); // rojo
    [SerializeField] private Color colorNeta     = new Color(0.20f, 0.85f, 0.30f); // verde
    [SerializeField] private Color colorPeso     = new Color(1.00f, 0.60f, 0.10f); // naranja
    [SerializeField] private Color colorNormal   = new Color(0.75f, 0.40f, 1.00f); // morado

    private SimulacionNewton sim;
    private Renderer rendCubo;
    private LineRenderer lrAplicada, lrFriccion, lrNeta, lrPeso, lrNormal;
    private GUIStyle estilo;

    // Magnitudes calculadas (N) y puntas de las flechas para las etiquetas
    private float fAplicada, fFriccion, fNeta, peso, normal;
    private Vector3 finAplicada, finFriccion, finNeta, finPeso, finNormal;

    void Awake()
    {
        sim = GetComponent<SimulacionNewton>();
        rendCubo = GetComponent<Renderer>();

        lrAplicada = CrearFlecha("Vector_F_aplicada", colorAplicada);
        lrFriccion = CrearFlecha("Vector_Friccion", colorFriccion);
        lrNeta     = CrearFlecha("Vector_F_neta", colorNeta);
        lrPeso     = CrearFlecha("Vector_Peso", colorPeso);
        lrNormal   = CrearFlecha("Vector_Normal", colorNormal);
    }

    LineRenderer CrearFlecha(string nombre, Color color)
    {
        GameObject go = new GameObject(nombre);
        go.transform.SetParent(transform, false);

        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;
        lr.numCapVertices = 2;
        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;

        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = color;
        lr.material = mat;
        lr.startColor = color;
        lr.endColor = color;
        return lr;
    }

    void LateUpdate()
    {
        // 1. Magnitudes según el modelo físico
        float m = sim.masa, F = sim.fuerza, mu = sim.mu;
        peso = ModeloFisico.Normal(m);          // P = m·g
        normal = peso;                          // N = m·g (superficie horizontal)
        float fMax = ModeloFisico.Friccion(mu, m);
        fAplicada = F;
        fFriccion = F > fMax ? fMax : F;        // si no se mueve, la fricción iguala a F
        fNeta = ModeloFisico.FuerzaNeta(F, m, mu);

        // 2. Ejes del marcador: derecha = dirección del movimiento, arriba = normal al marcador
        Transform marco = transform.parent != null ? transform.parent : transform;
        Vector3 derecha = marco.right;
        Vector3 arriba = marco.up;
        Vector3 c = transform.position;
        float medio = transform.lossyScale.y * 0.5f;

        // 3. Flechas
        finAplicada = Dibujar(lrAplicada, c + derecha * medio, derecha, fAplicada);
        finFriccion = Dibujar(lrFriccion, c - arriba * medio * 0.8f - derecha * medio, -derecha, fFriccion);
        finNeta     = Dibujar(lrNeta, c + arriba * medio + derecha * medio, derecha, fNeta);

        if (mostrarPesoYNormal)
        {
            finPeso   = Dibujar(lrPeso, c - arriba * medio, -arriba, peso);
            finNormal = Dibujar(lrNormal, c + arriba * medio, arriba, normal);
        }
        else
        {
            lrPeso.positionCount = 0;
            lrNormal.positionCount = 0;
        }
    }

    Vector3 Dibujar(LineRenderer lr, Vector3 origen, Vector3 dir, float magnitud)
    {
        float largo = Mathf.Min(magnitud * metrosPorNewton, largoMaximo);
        if (largo < 0.0005f) { lr.positionCount = 0; return origen; }

        Vector3 fin = origen + dir.normalized * largo;
        float largoPunta = Mathf.Min(grosor * 4f, largo * 0.5f);
        float t = 1f - largoPunta / largo;          // dónde empieza la punta (0..1)
        float anchoPunta = grosor * 3f;
        float pendiente = -anchoPunta / (1f - t);

        lr.positionCount = 4;
        lr.SetPosition(0, origen);
        lr.SetPosition(1, Vector3.Lerp(origen, fin, t - 0.001f));
        lr.SetPosition(2, Vector3.Lerp(origen, fin, t));
        lr.SetPosition(3, fin);
        lr.widthCurve = new AnimationCurve(
            new Keyframe(0f, grosor, 0f, 0f),
            new Keyframe(t - 0.001f, grosor, 0f, 0f),
            new Keyframe(t, anchoPunta, 0f, pendiente),
            new Keyframe(1f, 0f, pendiente, 0f));
        return fin;
    }

    void OnGUI()
    {
        if (!mostrarEtiquetas || rendCubo == null || !rendCubo.enabled) return; // marcador no visible
        Camera cam = Camera.main;
        if (cam == null) return;

        if (estilo == null)
        {
            estilo = new GUIStyle(GUI.skin.label) { fontSize = tamanioEtiqueta, fontStyle = FontStyle.Bold };
        }

        Etiqueta(cam, finAplicada, $"F = {fAplicada:F2} N", colorAplicada, lrAplicada);
        Etiqueta(cam, finFriccion, $"f = {fFriccion:F2} N", colorFriccion, lrFriccion);
        Etiqueta(cam, finNeta, $"F neta = {fNeta:F2} N", colorNeta, lrNeta);
        if (mostrarPesoYNormal)
        {
            Etiqueta(cam, finPeso, $"P = {peso:F2} N", colorPeso, lrPeso);
            Etiqueta(cam, finNormal, $"N = {normal:F2} N", colorNormal, lrNormal);
        }
    }

    void Etiqueta(Camera cam, Vector3 posMundo, string texto, Color color, LineRenderer lr)
    {
        if (lr.positionCount == 0) return;
        Vector3 p = cam.WorldToScreenPoint(posMundo);
        if (p.z < 0) return; // detrás de la cámara
        estilo.normal.textColor = color;
        GUI.Label(new Rect(p.x + 8, Screen.height - p.y - tamanioEtiqueta, 400, tamanioEtiqueta * 1.6f), texto, estilo);
    }
}