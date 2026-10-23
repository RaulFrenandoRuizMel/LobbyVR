using UnityEngine;

public class DestinoJuegoPuerta : MonoBehaviour
{
    // =====================================================
    // DESTINO PCVR
    // =====================================================

    [Header("Destino PCVR - Quest Link")]

    [Tooltip("Ruta relativa al EXE del juego.")]
    public string rutaRelativaPC;


    // =====================================================
    // DESTINO META QUEST STANDALONE
    // =====================================================

    [Header("Destino Meta Quest APK")]

    [Tooltip(
        "Package Name del juego instalado en Meta Quest. " +
        "Ejemplo: com.equipo4.juego"
    )]
    public string paqueteQuest;


    // =====================================================
    // LANZADOR
    // =====================================================

    [Header("Lanzador")]

    public LanzadorJuegoPCVR lanzador;


    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        if (lanzador == null)
        {
            lanzador =
                FindFirstObjectByType<LanzadorJuegoPCVR>();
        }
    }


    // =====================================================
    // EJECUTAR DESTINO
    // =====================================================

    public void EjecutarDestino()
    {
        if (lanzador == null)
        {
            Debug.LogError(
                "No existe LanzadorJuegoPCVR."
            );

            return;
        }


        Debug.Log(
            "DESTINO SOLICITADO POR: " +
            gameObject.name
        );


        lanzador.LanzarDestino(
            rutaRelativaPC,
            paqueteQuest
        );
    }
}