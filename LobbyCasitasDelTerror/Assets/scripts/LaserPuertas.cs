using UnityEngine;
using UnityEngine.XR;

public class LaserPuertas : MonoBehaviour
{
    [Header("Origen del laser")]
    public Transform origenLaser;

    [Header("Configuracion")]
    public float distancia = 20f;
    public LayerMask capaPuertas;

    private HoverPuerta hoverActual;

    private InputDevice controlDerecho;

    private bool botonALiberado = true;

    // Cuando ya elegimos una puerta,
    // esto queda en true para siempre.
    private bool seleccionBloqueada = false;


    void Start()
    {
        BuscarControlDerecho();
    }


    void Update()
    {
        if (!controlDerecho.isValid)
        {
            BuscarControlDerecho();
        }

        DetectarPuertaConLaser();

        LeerBotonA();
    }


    // ----------------------------------------------------
    // BUSCAR CONTROL DERECHO
    // ----------------------------------------------------

    void BuscarControlDerecho()
    {
        controlDerecho =
            InputDevices.GetDeviceAtXRNode(
                XRNode.RightHand
            );
    }


    // ----------------------------------------------------
    // LASER / HOVER
    // ----------------------------------------------------

    void DetectarPuertaConLaser()
    {
        // Si ya elegimos una puerta,
        // nunca más activamos hover.
        if (seleccionBloqueada)
        {
            ApagarHover();
            return;
        }


        if (origenLaser == null)
            return;


        RaycastHit hit;

        bool golpeo = Physics.Raycast(
            origenLaser.position,
            origenLaser.forward,
            out hit,
            distancia,
            capaPuertas,
            QueryTriggerInteraction.Collide
        );


        if (golpeo)
        {
            HoverPuerta nuevoHover =
                hit.collider.GetComponentInParent<HoverPuerta>();


            if (nuevoHover != hoverActual)
            {
                ApagarHover();

                hoverActual = nuevoHover;

                if (hoverActual != null)
                {
                    hoverActual.ActivarHover();

                    Debug.Log(
                        "HOVER ACTIVO -> " +
                        hoverActual.gameObject.name
                    );
                }
            }
        }
        else
        {
            ApagarHover();
        }
    }


    // ----------------------------------------------------
    // BOTON A
    // ----------------------------------------------------

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


        // Cuando soltamos A,
        // permitimos detectar otra pulsacion.
        if (!botonA)
        {
            botonALiberado = true;
            return;
        }


        if (botonA && botonALiberado)
        {
            botonALiberado = false;


            // Ya seleccionamos una puerta.
            if (seleccionBloqueada)
                return;


            // A solo funciona mientras existe hover.
            if (hoverActual == null)
                return;


            // Guardamos la puerta antes
            // de apagar el hover.
            HoverPuerta puertaConfirmada =
                hoverActual;


            Debug.Log(
                "PUERTA CONFIRMADA CON A -> " +
                puertaConfirmada.gameObject.name
            );


            // ---------------------------------------------
            // BLOQUEAR SELECCION
            // ---------------------------------------------

            seleccionBloqueada = true;


            // Quitamos inmediatamente el amarillo.
            puertaConfirmada.DesactivarHover();

            hoverActual = null;


            Debug.Log(
                "SELECCION DE PUERTAS BLOQUEADA"
            );


            // ---------------------------------------------
            // ABRIR PUERTA
            // ---------------------------------------------

            AbrirPuerta abrirPuerta =
                puertaConfirmada.GetComponent<AbrirPuerta>();

            if (abrirPuerta != null)
            {
                abrirPuerta.Abrir();
            }


            // ---------------------------------------------
            // MANO HORROR
            // ---------------------------------------------

            ManoHorror manoHorror =
                puertaConfirmada.GetComponent<ManoHorror>();

            if (manoHorror != null)
            {
                manoHorror.IniciarSecuencia();
            }
        }
    }


    // ----------------------------------------------------
    // APAGAR HOVER
    // ----------------------------------------------------

    void ApagarHover()
    {
        if (hoverActual != null)
        {
            hoverActual.DesactivarHover();

            hoverActual = null;
        }
    }
}