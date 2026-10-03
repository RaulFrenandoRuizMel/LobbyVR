using UnityEngine;
using System.Collections.Generic;

public class EfectoPuertaTerror : MonoBehaviour
{
    [Header("Puerta")]
    public Transform puertaVisual;

    [Header("Hover")]
    public float escalaHover = 1.03f;
    public float velocidadHover = 6f;

    [Header("Color de seleccion")]
    public Color colorHover = new Color(0.75f, 0.12f, 0.05f, 1f);

    [Range(0f, 1f)]
    public float mezclaColor = 0.45f;

    [Header("Emision")]
    public Color colorEmision = new Color(1f, 0.08f, 0.02f, 1f);

    public float intensidadEmision = 3f;

    private Vector3 escalaOriginal;

    private bool seleccionado = false;

    private List<Material> materiales = new List<Material>();
    private List<Color> coloresOriginales = new List<Color>();
    private List<Color> emisionesOriginales = new List<Color>();


    // Awake ocurre antes del Start del SelectorPuertas.
    void Awake()
    {
        if (puertaVisual == null)
            return;

        escalaOriginal = puertaVisual.localScale;

        Renderer[] renderers =
            puertaVisual.GetComponentsInChildren<Renderer>(true);


        foreach (Renderer rend in renderers)
        {
            // .materials crea instancias para esta puerta,
            // así no modificamos las otras puertas.
            Material[] mats = rend.materials;

            foreach (Material mat in mats)
            {
                if (mat == null)
                    continue;

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


                // Guardar emisión original
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
    }


    void Update()
    {
        if (puertaVisual == null)
            return;


        // -----------------------------
        // ESCALA HOVER
        // -----------------------------

        Vector3 escalaObjetivo =
            seleccionado
            ? escalaOriginal * escalaHover
            : escalaOriginal;


        puertaVisual.localScale =
            Vector3.Lerp(
                puertaVisual.localScale,
                escalaObjetivo,
                velocidadHover * Time.deltaTime
            );


        // -----------------------------
        // PULSO DE EMISION
        // -----------------------------

        if (seleccionado)
        {
            float pulso =
                Mathf.Lerp(
                    intensidadEmision * 0.6f,
                    intensidadEmision,
                    (Mathf.Sin(Time.time * 5f) + 1f) * 0.5f
                );

            AplicarEmision(pulso);
        }
    }


    // ==========================================
    // SELECCIONAR
    // ==========================================

    public void Seleccionar()
    {
        seleccionado = true;

        AplicarColorHover();

        Debug.Log(
            gameObject.name +
            " -> HOVER ACTIVADO"
        );
    }


    // ==========================================
    // DESELECCIONAR
    // ==========================================

    public void Deseleccionar()
    {
        seleccionado = false;

        RestaurarMateriales();
    }


    // ==========================================
    // COLOR HOVER
    // ==========================================

    void AplicarColorHover()
    {
        for (int i = 0; i < materiales.Count; i++)
        {
            Material mat = materiales[i];

            Color nuevoColor =
                Color.Lerp(
                    coloresOriginales[i],
                    colorHover,
                    mezclaColor
                );


            if (mat.HasProperty("_BaseColor"))
            {
                mat.SetColor(
                    "_BaseColor",
                    nuevoColor
                );
            }
            else if (mat.HasProperty("_Color"))
            {
                mat.SetColor(
                    "_Color",
                    nuevoColor
                );
            }


            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
            }
        }
    }


    // ==========================================
    // EMISION
    // ==========================================

    void AplicarEmision(float intensidad)
    {
        for (int i = 0; i < materiales.Count; i++)
        {
            Material mat = materiales[i];

            if (!mat.HasProperty("_EmissionColor"))
                continue;


            mat.EnableKeyword("_EMISSION");

            mat.SetColor(
                "_EmissionColor",
                colorEmision * intensidad
            );
        }
    }


    // ==========================================
    // RESTAURAR
    // ==========================================

    void RestaurarMateriales()
    {
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
    }
}