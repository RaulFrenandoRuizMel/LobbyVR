
using System.Collections;
using UnityEngine;

public class DestinoJuegoPuerta : MonoBehaviour
{
    [Header("LANZADOR")]
    public LanzadorJuegoPCVR lanzador;

    [Header("META QUEST APK")]
    public string packageName = "";
    public string activityName =
        "com.unity3d.player.UnityPlayerGameActivity";

    [Header("WINDOWS PCVR")]
    public string rutaPC = "";

    [Header("TIEMPO")]
    public float segundos = 2f;

    private bool iniciado = false;
    private Coroutine cuenta;

    // Llamar cuando la mano comienza a agarrar
    public void IniciarAgarre()
    {
        if (iniciado) return;

        if (lanzador == null)
        {
            Debug.LogError("Falta asignar Lanzador");
            return;
        }

        iniciado = true;

        Debug.Log("Agarre iniciado en: " + name);

        cuenta = StartCoroutine(EsperarYAbrir());
    }

    private IEnumerator EsperarYAbrir()
    {
        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, segundos));

        cuenta = null;

        Debug.Log("Abriendo juego de: " + name);

        lanzador.LanzarDestino(
            rutaPC,
            packageName,
            activityName
        );

        iniciado = false;
    }

    // Compatible con llamadas existentes
    public void EjecutarDestino()
    {
        IniciarAgarre();
    }

    public void CancelarAgarre()
    {
        if (cuenta != null)
            StopCoroutine(cuenta);

        cuenta = null;
        iniciado = false;
    }

    private void OnDisable()
    {
        CancelarAgarre();
    }
}
