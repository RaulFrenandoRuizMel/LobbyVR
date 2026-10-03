using UnityEngine;
using UnityEngine.XR;

public class SelectorPuertas : MonoBehaviour
{
    [Header("Referencias")]
    public Transform xrOrigin;
    public Transform mainCamera;

    [Header("Puertas / Focus")]
    public Transform[] focos;

    [Header("Configuracion")]
    public float velocidadGiro = 120f;
    public float deadZone = 0.6f;

    private int indiceActual = 0;

    private bool joystickLiberado = true;
    private bool girando = false;

    private float anguloObjetivo;

    // Control derecho del Quest
    private InputDevice controlDerecho;


    void Start()
    {
        BuscarControlDerecho();
    }


    void Update()
    {
        // Si pierde conexión, intenta recuperarlo.
        if (!controlDerecho.isValid)
        {
            BuscarControlDerecho();
        }

        LeerJoystickDerecho();

        if (girando)
        {
            GirarHaciaObjetivo();
        }
    }


    // -------------------------------------------------------
    // BUSCAR CONTROL DERECHO
    // -------------------------------------------------------

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


    // -------------------------------------------------------
    // LEER JOYSTICK DERECHO
    // -------------------------------------------------------

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


        // Cuando regresa al centro permite
        // seleccionar otra puerta.
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


    // -------------------------------------------------------
    // SIGUIENTE PUERTA
    // -------------------------------------------------------

    void SiguientePuerta()
    {
        if (focos == null || focos.Length == 0)
            return;

        indiceActual++;

        if (indiceActual >= focos.Length)
        {
            indiceActual = 0;
        }

        PrepararGiroHacia(indiceActual);

        Debug.Log(
            "Puerta seleccionada: " +
            (indiceActual + 1)
        );
    }


    // -------------------------------------------------------
    // PUERTA ANTERIOR
    // -------------------------------------------------------

    void PuertaAnterior()
    {
        if (focos == null || focos.Length == 0)
            return;

        indiceActual--;

        if (indiceActual < 0)
        {
            indiceActual = focos.Length - 1;
        }

        PrepararGiroHacia(indiceActual);

        Debug.Log(
            "Puerta seleccionada: " +
            (indiceActual + 1)
        );
    }


    // -------------------------------------------------------
    // CALCULAR GIRO HACIA PUERTA
    // -------------------------------------------------------

    void PrepararGiroHacia(int indice)
    {
        if (xrOrigin == null ||
            mainCamera == null ||
            focos == null ||
            focos.Length == 0 ||
            focos[indice] == null)
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


        if (direccionPuerta.sqrMagnitude < 0.001f ||
            direccionCamara.sqrMagnitude < 0.001f)
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


    // -------------------------------------------------------
    // GIRAR SIN CAMBIAR POSICION DE LA CABEZA
    // -------------------------------------------------------

    void GirarHaciaObjetivo()
    {
        if (xrOrigin == null ||
            mainCamera == null)
        {
            girando = false;
            return;
        }


        // Guardamos exactamente dónde está
        // la cabeza en el mundo.
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


        // Solo rotamos en Y.
        xrOrigin.rotation =
            Quaternion.Euler(
                0f,
                nuevoAngulo,
                0f
            );


        // Compensamos cualquier desplazamiento
        // producido por la rotación del XR Origin.
        Vector3 compensacion =
            posicionCabezaAntes -
            mainCamera.position;


        xrOrigin.position += compensacion;


        float restante =
            Mathf.Abs(
                Mathf.DeltaAngle(
                    xrOrigin.eulerAngles.y,
                    anguloObjetivo
                )
            );


        if (restante < 0.5f)
        {
            // Corrección final.
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