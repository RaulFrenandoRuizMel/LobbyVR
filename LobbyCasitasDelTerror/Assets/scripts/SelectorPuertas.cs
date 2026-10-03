using UnityEngine;
using UnityEngine.XR;

public class SelectorPuertas : MonoBehaviour
{
    [Header("Referencias")]
    public Transform xrOrigin;
    public Transform mainCamera;

    [Header("Puertas / Focus")]
    public Transform[] focos;

    [Header("Hover de Puertas")]
    public HoverPuerta[] hoverPuertas;

    [Header("Configuracion")]
    public float velocidadGiro = 120f;
    public float deadZone = 0.6f;


    private int indiceActual = 0;

    private bool joystickLiberado = true;
    private bool girando = false;

    private bool botonALiberado = true;

    private float anguloObjetivo;

    private InputDevice controlDerecho;


    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        BuscarControlDerecho();

        ActualizarHover();
    }


    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        if (!controlDerecho.isValid)
        {
            BuscarControlDerecho();
        }


        LeerJoystickDerecho();

        LeerBotonA();


        if (girando)
        {
            GirarHaciaObjetivo();
        }
    }


    // =====================================================
    // CONTROL DERECHO
    // =====================================================

    void BuscarControlDerecho()
    {
        controlDerecho =
            InputDevices.GetDeviceAtXRNode(
                XRNode.RightHand
            );


        if (controlDerecho.isValid)
        {
            Debug.Log(
                "Control derecho detectado: " +
                controlDerecho.name
            );
        }
        else
        {
            Debug.LogWarning(
                "No se detecto el control derecho."
            );
        }
    }


    // =====================================================
    // JOYSTICK DERECHO
    // =====================================================

    void LeerJoystickDerecho()
    {
        Vector2 joystick;


        bool recibido =
            controlDerecho.TryGetFeatureValue(
                CommonUsages.primary2DAxis,
                out joystick
            );


        if (!recibido)
            return;


        // Joystick vuelve al centro
        if (Mathf.Abs(joystick.x) < 0.2f)
        {
            joystickLiberado = true;
        }


        if (!joystickLiberado)
            return;


        // DERECHA
        if (joystick.x > deadZone)
        {
            SiguientePuerta();

            joystickLiberado = false;
        }


        // IZQUIERDA
        else if (joystick.x < -deadZone)
        {
            PuertaAnterior();

            joystickLiberado = false;
        }
    }


    // =====================================================
    // BOTON A
    // =====================================================

    void LeerBotonA()
    {
        bool botonA;


        bool recibido =
            controlDerecho.TryGetFeatureValue(
                CommonUsages.primaryButton,
                out botonA
            );


        if (!recibido)
            return;


        if (!botonA)
        {
            botonALiberado = true;
        }


        if (botonA && botonALiberado)
        {
            botonALiberado = false;

            ConfirmarPuerta();
        }
    }


    // =====================================================
    // SIGUIENTE PUERTA
    // =====================================================

    void SiguientePuerta()
    {
        if (focos == null ||
            focos.Length == 0)
        {
            return;
        }


        indiceActual++;


        if (indiceActual >= focos.Length)
        {
            indiceActual = 0;
        }


        // HOVER
        ActualizarHover();


        // GIRO
        PrepararGiroHacia(
            indiceActual
        );


        Debug.Log(
            "Puerta seleccionada: " +
            (indiceActual + 1)
        );
    }


    // =====================================================
    // PUERTA ANTERIOR
    // =====================================================

    void PuertaAnterior()
    {
        if (focos == null ||
            focos.Length == 0)
        {
            return;
        }


        indiceActual--;


        if (indiceActual < 0)
        {
            indiceActual =
                focos.Length - 1;
        }


        // HOVER
        ActualizarHover();


        // GIRO
        PrepararGiroHacia(
            indiceActual
        );


        Debug.Log(
            "Puerta seleccionada: " +
            (indiceActual + 1)
        );
    }


    // =====================================================
    // ACTUALIZAR HOVER
    // =====================================================

    void ActualizarHover()
    {
        if (hoverPuertas == null)
            return;


        // Apagar TODAS
        for (int i = 0;
             i < hoverPuertas.Length;
             i++)
        {
            if (hoverPuertas[i] != null)
            {
                hoverPuertas[i]
                    .DesactivarHover();
            }
        }


        // Encender SOLO la seleccionada
        if (
            indiceActual >= 0 &&
            indiceActual <
            hoverPuertas.Length &&
            hoverPuertas[indiceActual]
            != null
        )
        {
            hoverPuertas[indiceActual]
                .ActivarHover();
        }


        Debug.Log(
            "Hover aplicado a Puerta " +
            (indiceActual + 1)
        );
    }


    // =====================================================
    // CONFIRMAR CON A
    // =====================================================

    void ConfirmarPuerta()
    {
        Debug.Log(
            "PUERTA CONFIRMADA: " +
            (indiceActual + 1)
        );


        // DESPUES:
        // mano al visor
        // sonido
        // blackout
        // cargar escena
    }


    // =====================================================
    // PREPARAR GIRO
    // =====================================================

    void PrepararGiroHacia(
        int indice
    )
    {
        if (
            xrOrigin == null ||
            mainCamera == null ||
            focos == null ||
            focos.Length == 0 ||
            focos[indice] == null
        )
        {
            return;
        }


        Vector3 direccionPuerta =
            focos[indice].position -
            mainCamera.position;


        direccionPuerta.y = 0f;


        Vector3 direccionCamara =
            mainCamera.forward;


        direccionCamara.y = 0f;


        if (
            direccionPuerta
            .sqrMagnitude < 0.001f ||
            direccionCamara
            .sqrMagnitude < 0.001f
        )
        {
            return;
        }


        float diferencia =
            Vector3.SignedAngle(
                direccionCamara.normalized,
                direccionPuerta.normalized,
                Vector3.up
            );


        anguloObjetivo =
            xrOrigin.eulerAngles.y +
            diferencia;


        girando = true;
    }


    // =====================================================
    // GIRAR SIN MOVER AL JUGADOR
    // =====================================================

    void GirarHaciaObjetivo()
    {
        if (
            xrOrigin == null ||
            mainCamera == null
        )
        {
            girando = false;

            return;
        }


        // Guardar posicion exacta
        // del visor.
        Vector3 posicionCabezaAntes =
            mainCamera.position;


        float anguloActual =
            xrOrigin.eulerAngles.y;


        float nuevoAngulo =
            Mathf.MoveTowardsAngle(
                anguloActual,
                anguloObjetivo,
                velocidadGiro *
                Time.deltaTime
            );


        // Rotar solamente en Y
        xrOrigin.rotation =
            Quaternion.Euler(
                0f,
                nuevoAngulo,
                0f
            );


        // Compensar movimiento
        Vector3 compensacion =
            posicionCabezaAntes -
            mainCamera.position;


        xrOrigin.position +=
            compensacion;


        float restante =
            Mathf.Abs(
                Mathf.DeltaAngle(
                    xrOrigin.eulerAngles.y,
                    anguloObjetivo
                )
            );


        if (restante < 0.5f)
        {
            Vector3 cabezaAntesFinal =
                mainCamera.position;


            xrOrigin.rotation =
                Quaternion.Euler(
                    0f,
                    anguloObjetivo,
                    0f
                );


            xrOrigin.position +=
                cabezaAntesFinal -
                mainCamera.position;


            girando = false;
        }
    }
}