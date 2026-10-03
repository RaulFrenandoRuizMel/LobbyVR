using UnityEngine;
using System.Collections.Generic;

public class HoverPuerta : MonoBehaviour
{
    [Header("Objeto visual de la puerta")]
    public GameObject puertaVisual;

    [Header("Configuracion Hover")]
    public Color colorHover = new Color(1f, 0.15f, 0.02f, 1f);
    public float intensidadEmision = 4f;

    [Header("Escala")]
    public float escalaHover = 1.03f;
    public float velocidadEscala = 6f;

    private bool seleccionada = false;

    private Vector3 escalaOriginal;

    private List<Material> materiales = new List<Material>();
    private List<Color> coloresOriginales = new List<Color>();
    private List<Color> emisionesOriginales = new List<Color>();


    void Start()
    {
        if (puertaVisual == null)
        {
            Debug.LogWarning(
                gameObject.name +
                ": No tiene Puerta Visual asignada."
            );

            return;
        }


        escalaOriginal =
            puertaVisual.transform.localScale;


        Renderer[] renderers =
            puertaVisual.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer rend in renderers)
        {
            Material[] mats = rend.materials;

            foreach (Material mat in mats)
            {
                materiales.Add(mat);


                // Guardar color original
                if (mat.HasProperty("_BaseColor"))
                {
                    coloresOriginales.Add(
                        mat.GetColor("_BaseColor")
                    );
                }
                else if (mat.HasProperty("_Color"))
                {
                    coloresOriginales.Add(
                        mat.GetColor("_Color")
                    );
                }
                else
                {
                    coloresOriginales.Add(Color.white);
                }


                // Guardar emission original
                if (mat.HasProperty("_EmissionColor"))
                {
                    emisionesOriginales.Add(
                        mat.GetColor("_EmissionColor")
                    );
                }
                else
                {
                    emisionesOriginales.Add(Color.black);
                }
            }
        }


        Debug.Log(
            gameObject.name +
            ": HoverPuerta preparado con " +
            materiales.Count +
            " materiales."
        );
    }


    void Update()
    {
        if (puertaVisual == null)
            return;


        Vector3 objetivo;


        if (seleccionada)
        {
            objetivo =
                escalaOriginal *
                escalaHover;
        }
        else
        {
            objetivo =
                escalaOriginal;
        }


        puertaVisual.transform.localScale =
            Vector3.Lerp(
                puertaVisual.transform.localScale,
                objetivo,
                velocidadEscala *
                Time.deltaTime
            );
    }


    // ==========================================
    // ACTIVAR HOVER
    // ==========================================

    public void ActivarHover()
    {
        seleccionada = true;


        for (int i = 0; i < materiales.Count; i++)
        {
            Material mat = materiales[i];


            // Cambiar color base
            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor(
                    "_BaseColor",
                    Color.Lerp(
                        coloresOriginales[i],
                        colorHover,
                        0.6f
                    )
                );
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor(
                    "_Color",
                    Color.Lerp(
                        coloresOriginales[i],
                        colorHover,
                        0.6f
                    )
                );
            }


            // Emission
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");

                mat.SetColor(
                    "_EmissionColor",
                    colorHover *
                    intensidadEmision
                );
            }
        }


        Debug.Log(
            gameObject.name +
            " >>> HOVER ACTIVADO"
        );
    }


    // ==========================================
    // DESACTIVAR HOVER
    // ==========================================

    public void DesactivarHover()
    {
        seleccionada = false;


        for (int i = 0; i < materiales.Count; i++)
        {
            Material mat = materiales[i];


            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor(
                    "_BaseColor",
                    coloresOriginales[i]
                );
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor(
                    "_Color",
                    coloresOriginales[i]
                );
            }


            if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor(
                    "_EmissionColor",
                    emisionesOriginales[i]
                );
            }
        }


        Debug.Log(
            gameObject.name +
            " Hover desactivado"
        );
    }
}