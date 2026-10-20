using UnityEngine;

public class DestinoJuegoPuerta : MonoBehaviour
{
    [Header("Destino de ESTA puerta")]
    [Tooltip("Ruta relativa desde la carpeta del EXE actual.")]
    public string rutaRelativaExe;

    [Header("Lanzador")]
    public LanzadorJuegoPCVR lanzador;


    void Start()
    {
        if (lanzador == null)
        {
            lanzador =
                FindFirstObjectByType<LanzadorJuegoPCVR>();
        }
    }


    public void EjecutarDestino()
    {
        if (lanzador == null)
        {
            Debug.LogError(
                "No existe LanzadorJuegoPCVR."
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(rutaRelativaExe))
        {
            Debug.LogError(
                "La ruta relativa está vacía."
            );

            return;
        }


        Debug.Log(
            "RUTA RELATIVA SOLICITADA:\n" +
            rutaRelativaExe
        );


        lanzador.LanzarRuta(
            rutaRelativaExe
        );
    }
}