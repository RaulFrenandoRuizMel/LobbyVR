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

    // Evita que A se active muchas veces
    // mientras lo mantienes presionado.
    private bool botonALiberado = true;


    void Start()
    {
        BuscarControlDerecho();
    }


    void Update()
    {
        // Si pierde el control, lo busca otra vez.
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


        // Si estamos apuntando a un Focus
        if (golpeo)
        {
            HoverPuerta nuevoHover =
                hit.collider.GetComponentInParent<HoverPuerta>();


            // Si cambiamos de puerta
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

        // Si ya no apuntamos a ninguna puerta
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


        // Soltaste A
        if (!botonA)
        {
            botonALiberado = true;
            return;
        }


        // Presionaste A
        if (botonA && botonALiberado)
        {
            botonALiberado = false;


            // IMPORTANTE:
            // A solamente funciona si existe un hover activo.
            if (hoverActual != null)
            {
                Debug.Log(
                    "PUERTA CONFIRMADA CON A -> " +
                    hoverActual.gameObject.name
                );

                AbrirPuerta abrirPuerta =
                    hoverActual.GetComponent<AbrirPuerta>();

                if (abrirPuerta != null)
                {
                    abrirPuerta.Abrir();
                }
                else
                {
                    Debug.LogWarning(
                        "La puerta no tiene el script AbrirPuerta."
                    );
                }
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