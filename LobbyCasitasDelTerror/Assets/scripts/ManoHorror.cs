
using System.Collections;
using UnityEngine;

public class ManoHorror : MonoBehaviour
{
    // ============================================
    // REFERENCIAS
    // ============================================

    [Header("Referencias principales")]
    public GameObject mano;
    public Transform puntoSalida;
    public Transform puntoArrastre;
    public Transform mainCamera;
    public Transform xrOrigin;

    [Header("Ruta del arrastre")]
    public Transform puntoEntradaSegura;

    [Header("Palma y visor")]
    public Transform puntoPalma;
    public Transform puntoAgarreVisor;

    // ============================================
    // CUBO AUXILIAR
    // ============================================

    [Header("Ocultar cubo auxiliar")]
    [Tooltip("Arrastra SOLO el MeshRenderer del cubo. No el modelo de la mano.")]
    public MeshRenderer rendererCubo;

    // ============================================
    // BRAZO
    // ============================================

    [Header("Estiramiento del brazo")]
    public Transform huesoEstirar;
    public float escalaMaximaBrazo = 1f;

    [Header("Rotacion de la mano")]
    public Vector3 offsetRotacionMano = Vector3.zero;

    // ============================================
    // ORIENTACION DEL JUGADOR
    // ============================================

    [Header("Orientacion del jugador")]
    public bool orientarJugadorHaciaMano = true;

    [Tooltip("Girar hacia la mano antes de que aparezca")]
    public float duracionGiro = 0.35f;

    // ============================================
    // TIEMPOS
    // ============================================

    [Header("Aparicion")]
    public float esperaAntesDeSalir = 0.5f;
    public float velocidadEstiramiento = 4f;

    [Header("Agarre")]
    public float esperaAlAgarrar = 0.3f;

    [Header("Arrastre")]
    public float velocidadArrastre = 2f;
    public float distanciaFinal = 0.08f;
    public float tiempoMaximoArrastre = 15f;

    // ============================================
    // COLISIONES
    // ============================================

    [Header("Character Controller")]
    public CharacterController controladorJugador;

    // ============================================
    // DESTINO
    // ============================================

    [Header("Juego de esta puerta")]
    public DestinoJuegoPuerta destinoPuerta;

    // ============================================
    // VARIABLES INTERNAS
    // ============================================

    private bool secuenciaIniciada;
    private Coroutine rutina;

    private Vector3 escalaOriginalHueso;
    private Quaternion rotacionOriginalMano;

    // ============================================
    // START
    // ============================================

    private void Start()
    {
        if (controladorJugador == null && xrOrigin != null)
        {
            controladorJugador =
                xrOrigin.GetComponent<CharacterController>();
        }

        if (destinoPuerta == null)
        {
            Transform raizBusqueda = transform.parent;

            if (raizBusqueda == null)
                raizBusqueda = transform;

            destinoPuerta =
                raizBusqueda.GetComponentInChildren<
                    DestinoJuegoPuerta
                >(true);
        }

        if (huesoEstirar != null)
        {
            escalaOriginalHueso =
                huesoEstirar.localScale;
        }

        if (mano != null)
        {
            rotacionOriginalMano =
                mano.transform.rotation;

            if (mano != gameObject)
                mano.SetActive(false);
        }

        OcultarCubo();

        Debug.Log("MANO HORROR PREPARADA");
    }

    // ============================================
    // OCULTAR SOLO EL CUBO
    // ============================================

    private void OcultarCubo()
    {
        if (rendererCubo != null)
        {
            rendererCubo.enabled = false;
        }
    }

    // ============================================
    // INICIAR
    // ============================================

    public void IniciarSecuencia()
    {
        if (secuenciaIniciada)
            return;

        if (!isActiveAndEnabled)
            return;

        secuenciaIniciada = true;

        rutina = StartCoroutine(
            SecuenciaHorror()
        );
    }

    // ============================================
    // SECUENCIA PRINCIPAL
    // ============================================

    private IEnumerator SecuenciaHorror()
    {
        if (!ValidarReferencias())
        {
            Finalizar();
            yield break;
        }

        // ========================================
        // 1. ORIENTAR AL JUGADOR
        // ========================================

        if (orientarJugadorHaciaMano)
        {
            yield return StartCoroutine(
                GirarJugadorHaciaMano()
            );
        }

        // ========================================
        // 2. ESPERAR SALIDA
        // ========================================

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, esperaAntesDeSalir)
        );

        // ========================================
        // 3. APARECER MANO
        // ========================================

        mano.transform.position =
            puntoSalida.position;

        mano.transform.rotation =
            rotacionOriginalMano;

        if (huesoEstirar != null)
        {
            huesoEstirar.localScale =
                escalaOriginalHueso;
        }

        mano.SetActive(true);

        // Desactivar solamente el cubo auxiliar
        OcultarCubo();

        Debug.Log("MANO APARECIO SIN CUBO");

        // ========================================
        // 4. ACERCAR MANO AL VISOR
        // ========================================

        float distanciaInicial =
            Vector3.Distance(
                puntoPalma.position,
                puntoAgarreVisor.position
            );

        float tiempoAcercamiento = 0f;
        bool agarro = false;

        while (tiempoAcercamiento < 10f)
        {
            Vector3 objetivo =
                puntoAgarreVisor.position;

            OrientarMano(objetivo);

            Vector3 nuevaPalma =
                Vector3.MoveTowards(
                    puntoPalma.position,
                    objetivo,
                    Mathf.Max(
                        0.01f,
                        velocidadEstiramiento
                    ) * Time.unscaledDeltaTime
                );

            mano.transform.position +=
                nuevaPalma - puntoPalma.position;

            float distancia =
                Vector3.Distance(
                    puntoPalma.position,
                    objetivo
                );

            float progreso =
                1f - distancia /
                Mathf.Max(distanciaInicial, 0.001f);

            EstirarBrazo(
                Mathf.Clamp01(progreso)
            );

            if (distancia <= 0.015f)
            {
                agarro = true;
                break;
            }

            tiempoAcercamiento +=
                Time.unscaledDeltaTime;

            yield return null;
        }

        if (!agarro)
        {
            Debug.LogWarning(
                "LA MANO NO ALCANZO EL VISOR"
            );

            Finalizar();
            yield break;
        }

        // ========================================
        // 5. AGARRAR VISOR
        // ========================================

        AjustarPalmaAlVisor();

        Debug.Log("LA MANO AGARRO EL VISOR");

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, esperaAlAgarrar)
        );

        // ========================================
        // 6. COMENZAR ARRASTRE
        // ========================================

        float inicioArrastre =
            Time.realtimeSinceStartup;

        bool llego = true;

        // Primer punto opcional
        if (puntoEntradaSegura != null)
        {
            yield return StartCoroutine(
                ArrastrarHacia(
                    puntoEntradaSegura,
                    inicioArrastre,
                    resultado => llego = resultado
                )
            );
        }

        // Punto final en el centro
        if (llego)
        {
            yield return StartCoroutine(
                ArrastrarHacia(
                    puntoArrastre,
                    inicioArrastre,
                    resultado => llego = resultado
                )
            );
        }

        // ========================================
        // 7. SOLTAR JUGADOR
        // ========================================

        mano.SetActive(false);

        Debug.Log("LA MANO SOLTO AL JUGADOR");

        if (!llego)
        {
            Debug.LogWarning(
                "ARRASTRE INTERRUMPIDO. " +
                "REVISA PUNTOS Y COLLIDERS."
            );

            Finalizar();
            yield break;
        }

        // ========================================
        // 8. ABRIR JUEGO TRAS LA ESPERA
        // ========================================

        Debug.Log(
            "JUGADOR EN PUNTO ARRASTRE"
        );

        if (destinoPuerta != null)
        {
            destinoPuerta.IniciarAgarre();

            Debug.Log(
                "CUENTA DEL JUEGO INICIADA"
            );
        }
        else
        {
            Debug.LogError(
                "FALTA DESTINO JUEGO PUERTA"
            );
        }

        Finalizar();
    }

    // ============================================
    // GIRAR AL JUGADOR HACIA LA MANO
    // ============================================

    private IEnumerator GirarJugadorHaciaMano()
    {
        // Calcular la direccion horizontal
        // desde el visor hacia PuntoSalidaMano.

        Vector3 direccion =
            puntoSalida.position -
            mainCamera.position;

        direccion.y = 0f;

        Vector3 adelante =
            mainCamera.forward;

        adelante.y = 0f;

        if (direccion.sqrMagnitude < 0.001f ||
            adelante.sqrMagnitude < 0.001f)
        {
            yield break;
        }

        direccion.Normalize();
        adelante.Normalize();

        float angulo =
            Vector3.SignedAngle(
                adelante,
                direccion,
                Vector3.up
            );

        float duracion =
            Mathf.Max(0f, duracionGiro);

        // Giro instantaneo si la duracion
        // es cero.

        if (duracion <= 0.001f)
        {
            GirarOrigenAlrededorDeCabeza(angulo);
            yield break;
        }

        float transcurrido = 0f;
        float anguloAplicado = 0f;

        while (transcurrido < duracion)
        {
            transcurrido +=
                Time.unscaledDeltaTime;

            float progreso =
                Mathf.Clamp01(
                    transcurrido / duracion
                );

            // Suavizado al principio y al final.
            float suavizado =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progreso
                );

            float anguloDeseado =
                angulo * suavizado;

            float giroEsteFrame =
                anguloDeseado - anguloAplicado;

            GirarOrigenAlrededorDeCabeza(
                giroEsteFrame
            );

            anguloAplicado =
                anguloDeseado;

            yield return null;
        }

        Debug.Log(
            "JUGADOR ORIENTADO HACIA LA MANO"
        );
    }

    // ============================================
    // GIRAR SIN DESPLAZAR LA CABEZA
    // ============================================

    private void GirarOrigenAlrededorDeCabeza(
        float grados
    )
    {
        if (xrOrigin == null ||
            mainCamera == null)
            return;

        Vector3 posicionCabezaAntes =
            mainCamera.position;

        // Giramos el origen, no Main Camera.
        xrOrigin.Rotate(
            Vector3.up,
            grados,
            Space.World
        );

        // Compensar el desplazamiento causado
        // por el pivote del XR Origin.

        Vector3 compensacion =
            posicionCabezaAntes -
            mainCamera.position;

        xrOrigin.position +=
            compensacion;
    }

    // ============================================
    // ARRASTRE HACIA DESTINO
    // ============================================

    private IEnumerator ArrastrarHacia(
        Transform punto,
        float inicioArrastre,
        System.Action<bool> resultado
    )
    {
        if (punto == null)
        {
            resultado(false);
            yield break;
        }

        while (true)
        {
            // Usar la cabeza real como referencia.
            Vector3 cabeza =
                mainCamera.position;

            Vector3 objetivo =
                new Vector3(
                    punto.position.x,
                    cabeza.y,
                    punto.position.z
                );

            Vector3 diferencia =
                objetivo - cabeza;

            diferencia.y = 0f;

            float distancia =
                diferencia.magnitude;

            if (distancia <=
                Mathf.Max(0.01f, distanciaFinal))
            {
                resultado(true);
                yield break;
            }

            // ====================================
            // MOVER AL JUGADOR
            // ====================================

            Vector3 movimiento =
                Vector3.ClampMagnitude(
                    diferencia,
                    Mathf.Max(
                        0.01f,
                        velocidadArrastre
                    ) * Time.deltaTime
                );

            // Character Controller sigue activo.
            // No se desactiva la colision del piso.

            if (controladorJugador != null &&
                controladorJugador.enabled)
            {
                controladorJugador.Move(
                    movimiento
                );
            }
            else
            {
                xrOrigin.position += movimiento;
            }

            // ====================================
            // MANTENER LA MANO EN EL VISOR
            // ====================================

            OrientarMano(
                puntoAgarreVisor.position
            );

            AjustarPalmaAlVisor();

            EstirarBrazo(1f);

            // ====================================
            // LIMITE DE TIEMPO
            // ====================================

            float tiempo =
                Time.realtimeSinceStartup -
                inicioArrastre;

            if (tiempo >=
                Mathf.Max(
                    0.1f,
                    tiempoMaximoArrastre
                ))
            {
                Debug.LogWarning(
                    "TIEMPO MAXIMO DEL ARRASTRE"
                );

                resultado(false);
                yield break;
            }

            yield return null;
        }
    }

    // ============================================
    // ALINEAR PALMA CON VISOR
    // ============================================

    private void AjustarPalmaAlVisor()
    {
        if (mano == null ||
            puntoPalma == null ||
            puntoAgarreVisor == null)
        {
            return;
        }

        mano.transform.position +=
            puntoAgarreVisor.position -
            puntoPalma.position;
    }

    // ============================================
    // ORIENTAR MANO HACIA VISOR
    // ============================================

    private void OrientarMano(
        Vector3 objetivo
    )
    {
        if (mano == null ||
            puntoSalida == null)
            return;

        Vector3 direccion =
            objetivo -
            puntoSalida.position;

        if (direccion.sqrMagnitude < 0.001f)
            return;

        Quaternion rotacion =
            Quaternion.LookRotation(
                direccion.normalized,
                Vector3.up
            );

        mano.transform.rotation =
            rotacion *
            Quaternion.Euler(
                offsetRotacionMano
            );
    }

    // ============================================
    // ESTIRAR BRAZO
    // ============================================

    private void EstirarBrazo(float progreso)
    {
        if (huesoEstirar == null)
            return;

        Vector3 escala =
            escalaOriginalHueso;

        float multiplicador =
            Mathf.Lerp(
                1f,
                escalaMaximaBrazo,
                Mathf.Clamp01(progreso)
            );

        escala.y =
            escalaOriginalHueso.y *
            multiplicador;

        huesoEstirar.localScale = escala;
    }

    // ============================================
    // VALIDACIONES
    // ============================================

    private bool ValidarReferencias()
    {
        bool correcto =
            mano != null &&
            puntoSalida != null &&
            puntoArrastre != null &&
            mainCamera != null &&
            xrOrigin != null &&
            puntoPalma != null &&
            puntoAgarreVisor != null;

        if (!correcto)
        {
            Debug.LogError(
                "FALTAN REFERENCIAS EN MANOHORROR"
            );
        }

        if (mano == gameObject)
        {
            Debug.LogError(
                "MANOHORROR DEBE ESTAR EN UN " +
                "OBJETO DISTINTO DEL MODELO " +
                "QUE SE DESACTIVA."
            );

            correcto = false;
        }

        return correcto;
    }

    // ============================================
    // FINALIZAR SECUENCIA
    // ============================================

    private void Finalizar()
    {
        if (mano != null &&
            mano != gameObject)
        {
            mano.SetActive(false);
        }

        secuenciaIniciada = false;
        rutina = null;
    }

    private void OnDisable()
    {
        if (rutina != null)
        {
            StopCoroutine(rutina);
            rutina = null;
        }

        Finalizar();
    }
}
