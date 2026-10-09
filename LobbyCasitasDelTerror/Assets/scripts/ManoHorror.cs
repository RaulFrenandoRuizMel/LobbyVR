using System.Collections;
using UnityEngine;

public class ManoHorror : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject mano;
    public Transform puntoSalida;
    public Transform puntoArrastre;
    public Transform mainCamera;
    public Transform xrOrigin;

    [Header("Salida de la mano")]
    public float esperaAntesDeSalir = 0.5f;
    public float velocidadEstiramiento = 8f;
    public float distanciaDelVisor = 0.18f;

    [Header("Arrastre")]
    public float esperaAlAgarrar = 0.15f;
    public float velocidadArrastre = 3.5f;
    public float distanciaFinal = 0.25f;

    [Header("Forma de la mano")]
    public float largoMinimo = 0.15f;

    private bool secuenciaIniciada = false;

    private Vector3 escalaOriginal;


    void Start()
    {
        if (mano != null)
        {
            escalaOriginal = mano.transform.localScale;
            mano.SetActive(false);
        }
    }


    public void IniciarSecuencia()
    {
        if (secuenciaIniciada)
            return;

        secuenciaIniciada = true;

        StartCoroutine(SecuenciaHorror());
    }


    IEnumerator SecuenciaHorror()
    {
        yield return new WaitForSeconds(
            esperaAntesDeSalir
        );


        if (mano == null ||
            puntoSalida == null ||
            puntoArrastre == null ||
            mainCamera == null ||
            xrOrigin == null)
        {
            Debug.LogWarning(
                "Faltan referencias en ManoHorror."
            );

            yield break;
        }


        // -------------------------------------------------
        // 1. MANO APARECE EN LA PUERTA
        // -------------------------------------------------

        mano.SetActive(true);

        Vector3 puntaActual =
            puntoSalida.position;

        Debug.Log("MANO EMPEZANDO A ESTIRARSE");


        // -------------------------------------------------
        // 2. ESTIRAR HASTA EL VISOR
        // -------------------------------------------------

        while (true)
        {
            Vector3 objetivoVisor =
                mainCamera.position +
                mainCamera.forward *
                distanciaDelVisor;


            puntaActual =
                Vector3.MoveTowards(
                    puntaActual,
                    objetivoVisor,
                    velocidadEstiramiento *
                    Time.deltaTime
                );


            ActualizarMano(
                puntoSalida.position,
                puntaActual
            );


            float distancia =
                Vector3.Distance(
                    puntaActual,
                    objetivoVisor
                );


            if (distancia < 0.03f)
                break;


            yield return null;
        }


        Debug.Log("MANO AGARRO EL VISOR");


        yield return new WaitForSeconds(
            esperaAlAgarrar
        );


        // -------------------------------------------------
        // 3. ARRASTRAR JUGADOR HACIA LA PUERTA
        // -------------------------------------------------

        Debug.Log("ARRASTRANDO JUGADOR");


        while (true)
        {
            Vector3 cabeza =
                mainCamera.position;


            Vector3 destino =
                new Vector3(
                    puntoArrastre.position.x,
                    cabeza.y,
                    puntoArrastre.position.z
                );


            float distancia =
                Vector3.Distance(
                    cabeza,
                    destino
                );


            if (distancia <= distanciaFinal)
                break;


            Vector3 nuevaCabeza =
                Vector3.MoveTowards(
                    cabeza,
                    destino,
                    velocidadArrastre *
                    Time.deltaTime
                );


            xrOrigin.position +=
                nuevaCabeza - cabeza;


            // La punta sigue pegada al visor.
            // Como el jugador se acerca a la puerta,
            // la mano se va encogiendo automáticamente.
            Vector3 objetivoVisor =
                mainCamera.position +
                mainCamera.forward *
                distanciaDelVisor;


            ActualizarMano(
                puntoSalida.position,
                objetivoVisor
            );


            yield return null;
        }


        Debug.Log(
            "JUGADOR LLEGO A LA PUERTA"
        );
    }


    // -------------------------------------------------
    // ESTIRAR MANO ENTRE DOS PUNTOS
    // -------------------------------------------------

    void ActualizarMano(
        Vector3 inicio,
        Vector3 fin
    )
    {
        Vector3 direccion =
            fin - inicio;


        float distancia =
            direccion.magnitude;


        if (distancia < 0.001f)
            return;


        // Centro entre puerta y visor
        mano.transform.position =
            inicio +
            direccion * 0.5f;


        // La mano apunta hacia el visor
        mano.transform.rotation =
            Quaternion.LookRotation(
                direccion.normalized,
                Vector3.up
            );


        // Mantener grosor X/Y.
        // Solo estiramos Z.
        Vector3 escala =
            escalaOriginal;


        float escalaPadreZ = 1f;

        if (mano.transform.parent != null)
        {
            escalaPadreZ =
                Mathf.Abs(
                    mano.transform.parent.lossyScale.z
                );

            if (escalaPadreZ < 0.001f)
                escalaPadreZ = 1f;
        }


        escala.z =
            Mathf.Max(
                largoMinimo,
                distancia / escalaPadreZ
            );


        mano.transform.localScale =
            escala;
    }
}