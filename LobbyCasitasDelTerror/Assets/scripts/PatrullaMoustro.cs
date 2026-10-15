using System.Collections;
using UnityEngine;

public class PatrullaMonstruo : MonoBehaviour
{
    [Header("Ruta")]
    public Transform[] puntos;

    [Header("Movimiento")]
    public float velocidad = 0.45f;
    public float velocidadGiro = 120f;
    public float distanciaLlegada = 0.15f;

    [Header("Evento en B")]
    public Transform puntoMirada;
    public Animator animatorMonstruo;
    public MiradaMonstruo miradaMonstruo;
    public float tiempoMirando = 3f;

    private int indiceActual = 0;
    private bool pausado = false;

    void Start()
    {
        if (puntos == null || puntos.Length == 0)
            return;

        transform.position = puntos[0].position;
        indiceActual = 1;
    }

    void Update()
    {
        if (pausado)
            return;

        if (puntos == null || puntos.Length == 0)
            return;

        Transform destino = puntos[indiceActual];

        Vector3 direccion = destino.position - transform.position;
        direccion.y = 0f;

        // Esto SÍ gira todo el cuerpo,
        // pero solamente mientras camina entre puntos.
        if (direccion.sqrMagnitude > 0.001f)
        {
            Quaternion rotacionObjetivo =
                Quaternion.LookRotation(direccion.normalized);

            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                rotacionObjetivo,
                velocidadGiro * Time.deltaTime
            );
        }

        Vector3 posicionObjetivo = destino.position;
        posicionObjetivo.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            posicionObjetivo,
            velocidad * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, posicionObjetivo)
            <= distanciaLlegada)
        {
            if (destino == puntoMirada)
            {
                StartCoroutine(EventoMirada());
            }
            else
            {
                SiguientePunto();
            }
        }
    }

    IEnumerator EventoMirada()
    {
        pausado = true;

        // Detenemos la caminata.
        if (animatorMonstruo != null)
            animatorMonstruo.speed = 0f;

        // SOLO la cabeza mira al jugador.
        if (miradaMonstruo != null)
            miradaMonstruo.mirar = true;

        yield return new WaitForSeconds(tiempoMirando);

        // La cabeza vuelve suavemente.
        if (miradaMonstruo != null)
            miradaMonstruo.mirar = false;

        // Le damos un poco de tiempo para regresar.
        yield return new WaitForSeconds(1.2f);

        if (animatorMonstruo != null)
            animatorMonstruo.speed = 1f;

        SiguientePunto();
        pausado = false;
    }

    void SiguientePunto()
    {
        indiceActual++;

        if (indiceActual >= puntos.Length)
            indiceActual = 0;
    }
}