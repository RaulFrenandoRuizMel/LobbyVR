using UnityEngine;

public class AbrirPuerta : MonoBehaviour
{
    [Header("Punto de bisagra")]
    public Transform pivote;

    [Header("Partes que se mueven")]
    public Transform[] piezas;

    [Header("Apertura")]
    public float anguloApertura = -90f;
    public float velocidad = 90f;

    private Transform bisagraRuntime;

    private bool abriendo = false;
    private bool abierta = false;

    private float anguloInicialY;
    private float anguloObjetivoY;


    void Start()
    {
        if (pivote == null)
        {
            Debug.LogError("Falta asignar PivotePuerta.");
            return;
        }

        // Creamos una bisagra nueva exactamente
        // donde está PivotePuerta.
        GameObject objetoBisagra =
            new GameObject("BisagraRuntime");

        bisagraRuntime =
            objetoBisagra.transform;

        bisagraRuntime.position =
            pivote.position;

        // La dejamos alineada con el mundo.
        bisagraRuntime.rotation =
            Quaternion.identity;


        // Metemos las piezas dentro de la bisagra
        // SIN cambiar su posición visual.
        foreach (Transform pieza in piezas)
        {
            if (pieza != null)
            {
                pieza.SetParent(
                    bisagraRuntime,
                    true
                );
            }
        }

        anguloInicialY =
            bisagraRuntime.eulerAngles.y;

        anguloObjetivoY =
            anguloInicialY +
            anguloApertura;
    }


    void Update()
    {
        if (!abriendo || abierta)
            return;

        float nuevoAngulo =
            Mathf.MoveTowardsAngle(
                bisagraRuntime.eulerAngles.y,
                anguloObjetivoY,
                velocidad * Time.deltaTime
            );

        bisagraRuntime.rotation =
            Quaternion.Euler(
                0f,
                nuevoAngulo,
                0f
            );


        float restante =
            Mathf.Abs(
                Mathf.DeltaAngle(
                    bisagraRuntime.eulerAngles.y,
                    anguloObjetivoY
                )
            );

        if (restante < 0.5f)
        {
            bisagraRuntime.rotation =
                Quaternion.Euler(
                    0f,
                    anguloObjetivoY,
                    0f
                );

            abierta = true;
            abriendo = false;

            Debug.Log(
                gameObject.name +
                " >>> PUERTA ABIERTA"
            );
        }
    }


    public void Abrir()
    {
        if (abierta || abriendo)
            return;

        if (bisagraRuntime == null)
            return;

        abriendo = true;

        Debug.Log(
            gameObject.name +
            " >>> ABRIENDO PUERTA"
        );
    }
}