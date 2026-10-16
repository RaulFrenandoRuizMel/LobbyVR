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

    [Header("Evento de mirada en B")]
    public Transform puntoMirada;
    public Animator animatorMonstruo;
    public MiradaMonstruo miradaMonstruo;

    public float tiempoMirando = 3f;
    public float tiempoRegresoCabeza = 1.2f;

    [Header("Sonido de cadenas")]
    public AudioSource audioCadenas;

    private int indiceActual = 0;
    private bool pausado = false;

    void Start()
    {
        if (puntos == null || puntos.Length == 0)
        {
            Debug.LogWarning("PatrullaMonstruo: No hay puntos asignados.");
            return;
        }

        // Empieza exactamente en A
        transform.position = puntos[0].position;

        // Primer destino = B
        indiceActual = 1;

        // Cabeza normal al iniciar
        if (miradaMonstruo != null)
            miradaMonstruo.mirar = false;

        // Cadenas sonando constantemente mientras patrulla
        if (audioCadenas != null)
        {
            audioCadenas.loop = true;

            if (!audioCadenas.isPlaying)
                audioCadenas.Play();
        }
    }

    void Update()
    {
        if (pausado)
            return;

        if (puntos == null || puntos.Length == 0)
            return;

        Transform destino = puntos[indiceActual];

        if (destino == null)
            return;

        // =========================
        // GIRAR HACIA EL DESTINO
        // =========================

        Vector3 direccion = destino.position - transform.position;
        direccion.y = 0f;

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

        // =========================
        // CAMINAR
        // =========================

        Vector3 posicionObjetivo = destino.position;
        posicionObjetivo.y = transform.position.y;

        transform.position = Vector3.MoveTowards(
            transform.position,
            posicionObjetivo,
            velocidad * Time.deltaTime
        );

        // =========================
        // LLEGADA AL PUNTO
        // =========================

        float distancia = Vector3.Distance(
            transform.position,
            posicionObjetivo
        );

        if (distancia <= distanciaLlegada)
        {
            // Cada vez que llega a B:
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

        // Detener animación
        if (animatorMonstruo != null)
            animatorMonstruo.speed = 0f;

        // SILENCIO cuando te mira
        if (audioCadenas != null)
            audioCadenas.Pause();

        // Solo cabeza hacia jugador
        if (miradaMonstruo != null)
            miradaMonstruo.mirar = true;

        // Te observa
        yield return new WaitForSeconds(tiempoMirando);

        // Cabeza regresa
        if (miradaMonstruo != null)
            miradaMonstruo.mirar = false;

        yield return new WaitForSeconds(tiempoRegresoCabeza);

        // Siguiente destino
        SiguientePunto();

        // Reanudar caminata
        if (animatorMonstruo != null)
            animatorMonstruo.speed = 1f;

        // Continúan las cadenas desde donde iban
        if (audioCadenas != null)
            audioCadenas.UnPause();

        pausado = false;
    }

    void SiguientePunto()
    {
        indiceActual++;

        if (indiceActual >= puntos.Length)
            indiceActual = 0;
    }
}