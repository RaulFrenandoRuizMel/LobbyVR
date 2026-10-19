using System.Collections;
using UnityEngine;


public class ManoHorror : MonoBehaviour
{
    // =====================================================
    // REFERENCIAS
    // =====================================================

    [Header("Referencias")]

    public GameObject mano;

    public Transform puntoSalida;

    public Transform puntoArrastre;

    public Transform mainCamera;

    public Transform xrOrigin;


    // =====================================================
    // PALMA / VISOR
    // =====================================================

    [Header("Contacto de la mano")]

    public Transform puntoPalma;


    [Header("Agarre al visor")]

    public Transform puntoAgarreVisor;


    // =====================================================
    // BRAZO
    // =====================================================

    [Header("Brazo")]

    public Transform huesoEstirar;


    [Tooltip(
        "1 = mano normal. " +
        "Mayor de 1 = brazo estirado."
    )]
    public float escalaMaximaBrazo = 1f;


    // =====================================================
    // ORIENTACION
    // =====================================================

    [Header("Orientacion")]

    public Vector3 offsetRotacionMano =
        Vector3.zero;


    // =====================================================
    // SALIDA
    // =====================================================

    [Header("Salida de la mano")]

    public float esperaAntesDeSalir =
        0.5f;


    public float velocidadEstiramiento =
        4f;


    // =====================================================
    // ARRASTRE
    // =====================================================

    [Header("Arrastre")]

    public float esperaAlAgarrar =
        0.3f;


    public float velocidadArrastre =
        3.5f;


    public float distanciaFinal =
        0.25f;


    // =====================================================
    // DESTINO DE ESTA PUERTA
    // =====================================================

    [Header("Juego al terminar")]

    [Tooltip(
        "Arrastra aqui el objeto global LanzadorJuegos."
    )]
    public LanzadorJuegoPCVR lanzadorJuego;


    [Tooltip(
        "TU eliges aqui que juego abre ESTA puerta."
    )]
    public LanzadorJuegoPCVR.DestinoPCVR destinoJuego =
        LanzadorJuegoPCVR.DestinoPCVR.Equipo1;


    public float esperaAntesDeAbrirJuego =
        0.5f;


    // =====================================================
    // PRIVADAS
    // =====================================================

    private bool secuenciaIniciada =
        false;


    private Vector3 escalaOriginalHueso;


    private MeshRenderer rendererCubito;


    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        if (huesoEstirar != null)
        {
            escalaOriginalHueso =
                huesoEstirar.localScale;
        }


        if (mano != null)
        {
            rendererCubito =
                mano.GetComponent<MeshRenderer>();


            if (rendererCubito != null)
            {
                rendererCubito.enabled =
                    false;
            }


            mano.SetActive(
                false
            );
        }
    }


    // =====================================================
    // INICIAR
    // =====================================================

    public void IniciarSecuencia()
    {
        if (secuenciaIniciada)
            return;


        secuenciaIniciada =
            true;


        StartCoroutine(
            SecuenciaHorror()
        );
    }


    // =====================================================
    // SECUENCIA
    // =====================================================

    IEnumerator SecuenciaHorror()
    {
        // -------------------------------------------------
        // ESPERA
        // -------------------------------------------------

        yield return
            new WaitForSeconds(
                esperaAntesDeSalir
            );


        // -------------------------------------------------
        // VALIDAR
        // -------------------------------------------------

        if (mano == null ||
            puntoSalida == null ||
            puntoArrastre == null ||
            mainCamera == null ||
            xrOrigin == null ||
            puntoPalma == null ||
            puntoAgarreVisor == null ||
            huesoEstirar == null)
        {
            Debug.LogWarning(
                "FALTAN REFERENCIAS EN ManoHorror"
            );


            yield break;
        }


        // =================================================
        // 1. APARECER
        // =================================================

        mano.transform.position =
            puntoSalida.position;


        mano.SetActive(
            true
        );


        if (rendererCubito != null)
        {
            rendererCubito.enabled =
                false;
        }


        huesoEstirar.localScale =
            escalaOriginalHueso;


        Debug.Log(
            "MANO APARECIO"
        );


        float distanciaInicial =
            Vector3.Distance(
                puntoSalida.position,
                puntoAgarreVisor.position
            );


        // =================================================
        // 2. IR HACIA EL VISOR
        // =================================================

        while (true)
        {
            Vector3 objetivoVisor =
                puntoAgarreVisor.position;


            OrientarMano(
                objetivoVisor
            );


            Vector3 siguientePosicionPalma =
                Vector3.MoveTowards(
                    puntoPalma.position,
                    objetivoVisor,
                    velocidadEstiramiento *
                    Time.deltaTime
                );


            Vector3 desplazamiento =
                siguientePosicionPalma -
                puntoPalma.position;


            mano.transform.position +=
                desplazamiento;


            // ---------------------------------------------
            // BRAZO
            // ---------------------------------------------

            float distanciaRecorrida =
                Vector3.Distance(
                    puntoSalida.position,
                    puntoPalma.position
                );


            float progreso =
                Mathf.Clamp01(
                    distanciaRecorrida /
                    Mathf.Max(
                        distanciaInicial,
                        0.001f
                    )
                );


            EstirarBrazo(
                progreso
            );


            // ---------------------------------------------
            // ¿LLEGO?
            // ---------------------------------------------

            float distanciaPalma =
                Vector3.Distance(
                    puntoPalma.position,
                    objetivoVisor
                );


            if (distanciaPalma <=
                0.015f)
            {
                break;
            }


            yield return null;
        }


        Debug.Log(
            "PALMA AGARRO EL VISOR"
        );


        // =================================================
        // 3. CLAVAR PALMA AL VISOR
        // =================================================

        mano.transform.position +=
            puntoAgarreVisor.position -
            puntoPalma.position;


        yield return
            new WaitForSeconds(
                esperaAlAgarrar
            );


        float distanciaBrazoAlAgarrar =
            Vector3.Distance(
                puntoSalida.position,
                puntoPalma.position
            );


        // =================================================
        // 4. JALAR JUGADOR
        // =================================================

        Debug.Log(
            "ARRASTRANDO JUGADOR"
        );


        while (true)
        {
            Vector3 cabeza =
                mainCamera.position;


            Vector3 destinoArrastre =
                new Vector3(
                    puntoArrastre.position.x,
                    cabeza.y,
                    puntoArrastre.position.z
                );


            float distanciaJugador =
                Vector3.Distance(
                    cabeza,
                    destinoArrastre
                );


            if (distanciaJugador <=
                distanciaFinal)
            {
                break;
            }


            // ---------------------------------------------
            // MOVER XR ORIGIN
            // ---------------------------------------------

            Vector3 nuevaCabeza =
                Vector3.MoveTowards(
                    cabeza,
                    destinoArrastre,
                    velocidadArrastre *
                    Time.deltaTime
                );


            xrOrigin.position +=
                nuevaCabeza -
                cabeza;


            // ---------------------------------------------
            // MANTENER PALMA EN EL VISOR
            // ---------------------------------------------

            Vector3 objetivoVisor =
                puntoAgarreVisor.position;


            Vector3 correccionPalma =
                objetivoVisor -
                puntoPalma.position;


            mano.transform.position +=
                correccionPalma;


            OrientarMano(
                objetivoVisor
            );


            // ---------------------------------------------
            // AJUSTAR BRAZO
            // ---------------------------------------------

            float distanciaBrazoActual =
                Vector3.Distance(
                    puntoSalida.position,
                    puntoPalma.position
                );


            float progresoBrazo =
                Mathf.Clamp01(
                    distanciaBrazoActual /
                    Mathf.Max(
                        distanciaBrazoAlAgarrar,
                        0.001f
                    )
                );


            EstirarBrazo(
                progresoBrazo
            );


            yield return null;
        }


        // =================================================
        // 5. TERMINO EL JALON
        // =================================================

        Debug.Log(
            "JUGADOR LLEGO A LA PUERTA"
        );


        Debug.Log(
            "MANOHORROR DE ESTA PUERTA PIDE: " +
            destinoJuego
        );


        yield return
            new WaitForSeconds(
                esperaAntesDeAbrirJuego
            );


        // =================================================
        // 6. LANZAR DESTINO DE ESTA PUERTA
        // =================================================

        if (lanzadorJuego != null)
        {
            Debug.Log(
                "ENVIANDO AL LANZADOR: " +
                destinoJuego
            );


            // *********************************************
            // ESTA ES LA LINEA IMPORTANTE
            // *********************************************

            lanzadorJuego.LanzarJuego(
                destinoJuego
            );
        }
        else
        {
            Debug.LogError(
                "NO HAY LanzadorJuegoPCVR " +
                "ASIGNADO EN ESTA PUERTA"
            );
        }
    }


    // =====================================================
    // ORIENTACION
    // =====================================================

    void OrientarMano(
        Vector3 objetivo
    )
    {
        Vector3 direccion =
            objetivo -
            puntoSalida.position;


        if (direccion.sqrMagnitude <
            0.001f)
        {
            return;
        }


        Quaternion rotacionObjetivo =
            Quaternion.LookRotation(
                direccion.normalized,
                Vector3.up
            );


        mano.transform.rotation =
            rotacionObjetivo *
            Quaternion.Euler(
                offsetRotacionMano
            );
    }


    // =====================================================
    // BRAZO
    // =====================================================

    void EstirarBrazo(
        float progreso
    )
    {
        if (huesoEstirar == null)
            return;


        Vector3 escala =
            escalaOriginalHueso;


        float multiplicador =
            Mathf.Lerp(
                1f,
                escalaMaximaBrazo,
                progreso
            );


        escala.y =
            escalaOriginalHueso.y *
            multiplicador;


        huesoEstirar.localScale =
            escala;
    }
}