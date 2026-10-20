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


    public float escalaMaximaBrazo =
        1f;


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

    [Header("Destino de esta puerta")]

    [Tooltip(
        "Normalmente se detecta automáticamente " +
        "del hijo DestinoJuego."
    )]
    public DestinoJuegoPuerta destinoPuerta;


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
        // -------------------------------------------------
        // BUSCAR AUTOMÁTICAMENTE EL DESTINO HIJO
        // -------------------------------------------------

        if (destinoPuerta == null)
        {
            destinoPuerta =
                GetComponentInChildren
                <DestinoJuegoPuerta>(
                    true
                );
        }


        if (destinoPuerta != null)
        {
            Debug.Log(
                gameObject.name +
                " DETECTO SU DESTINO: " +
                destinoPuerta.gameObject.name
            );
        }
        else
        {
            Debug.LogError(
                gameObject.name +
                " NO TIENE DestinoJuegoPuerta COMO HIJO"
            );
        }


        // -------------------------------------------------
        // HUESO
        // -------------------------------------------------

        if (huesoEstirar != null)
        {
            escalaOriginalHueso =
                huesoEstirar.localScale;
        }


        // -------------------------------------------------
        // MANO
        // -------------------------------------------------

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
    // INICIAR SECUENCIA
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
        yield return
            new WaitForSeconds(
                esperaAntesDeSalir
            );


        // =================================================
        // VALIDAR
        // =================================================

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
        // 2. IR AL VISOR
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
        // 3. PEGAR PALMA
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
        // 4. ARRASTRAR
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
            // PALMA PEGADA AL VISOR
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
        // 5. TERMINÓ
        // =================================================

        Debug.Log(
            "JUGADOR LLEGO A LA PUERTA"
        );


        yield return
            new WaitForSeconds(
                esperaAntesDeAbrirJuego
            );


        // =================================================
        // 6. EJECUTAR EL DESTINO HIJO DE ESTA PUERTA
        // =================================================

        if (destinoPuerta != null)
        {
            Debug.Log(
                "EJECUTANDO DESTINO DE " +
                gameObject.name
            );


            destinoPuerta
                .EjecutarDestino();
        }
        else
        {
            Debug.LogError(
                gameObject.name +
                " NO TIENE DESTINO JUEGO"
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