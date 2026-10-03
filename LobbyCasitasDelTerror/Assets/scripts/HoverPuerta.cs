using UnityEngine;

public class HoverPuerta : MonoBehaviour
{
    [Header("Modelo de la puerta")]
    public GameObject puertaVisual;

    [Header("Hover")]
    public Color colorHover = new Color(1f, 0.15f, 0.02f, 1f);
    public float intensidadEmision = 5f;
    public float escalaHover = 1.03f;

    private Renderer[] renderers;
    private Material[][] materiales;

    private Color[][] coloresOriginales;
    private Color[][] emisionesOriginales;

    private Vector3 escalaOriginal;

    private bool preparado = false;


    void Start()
    {
        Preparar();
        
    }


    void Preparar()
    {
        if (puertaVisual == null)
        {
            Debug.LogWarning(
                name + ": falta asignar puertaVisual"
            );

            return;
        }

        escalaOriginal =
            puertaVisual.transform.localScale;

        renderers =
            puertaVisual.GetComponentsInChildren<Renderer>(true);

        materiales =
            new Material[renderers.Length][];

        coloresOriginales =
            new Color[renderers.Length][];

        emisionesOriginales =
            new Color[renderers.Length][];


        for (int r = 0; r < renderers.Length; r++)
        {
            materiales[r] =
                renderers[r].materials;

            coloresOriginales[r] =
                new Color[materiales[r].Length];

            emisionesOriginales[r] =
                new Color[materiales[r].Length];


            for (int m = 0; m < materiales[r].Length; m++)
            {
                Material mat =
                    materiales[r][m];


                if (mat.HasProperty("_BaseColor"))
                {
                    coloresOriginales[r][m] =
                        mat.GetColor("_BaseColor");
                }
                else if (mat.HasProperty("_Color"))
                {
                    coloresOriginales[r][m] =
                        mat.GetColor("_Color");
                }
                else
                {
                    coloresOriginales[r][m] =
                        Color.white;
                }


                if (mat.HasProperty("_EmissionColor"))
                {
                    emisionesOriginales[r][m] =
                        mat.GetColor("_EmissionColor");
                }
                else
                {
                    emisionesOriginales[r][m] =
                        Color.black;
                }
            }
        }


        preparado = true;

        Debug.Log(
            name + " HOVER PREPARADO"
        );
    }


    public void ActivarHover()
    {
        if (!preparado)
            Preparar();

        if (!preparado)
            return;


        puertaVisual.transform.localScale =
            escalaOriginal * escalaHover;


        for (int r = 0; r < materiales.Length; r++)
        {
            for (int m = 0; m < materiales[r].Length; m++)
            {
                Material mat =
                    materiales[r][m];


                // Color más brillante
                if (mat.HasProperty("_BaseColor"))
                {
                    Color nuevo =
                        Color.Lerp(
                            coloresOriginales[r][m],
                            colorHover,
                            0.65f
                        );

                    mat.SetColor(
                        "_BaseColor",
                        nuevo
                    );
                }
                else if (mat.HasProperty("_Color"))
                {
                    Color nuevo =
                        Color.Lerp(
                            coloresOriginales[r][m],
                            colorHover,
                            0.65f
                        );

                    mat.SetColor(
                        "_Color",
                        nuevo
                    );
                }


                // Emisión
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
        }


        Debug.Log(
            name + " >>> HOVER ACTIVADO"
        );
    }


    public void DesactivarHover()
    {
        if (!preparado)
            return;


        puertaVisual.transform.localScale =
            escalaOriginal;


        for (int r = 0; r < materiales.Length; r++)
        {
            for (int m = 0; m < materiales[r].Length; m++)
            {
                Material mat =
                    materiales[r][m];


                if (mat.HasProperty("_BaseColor"))
                {
                    mat.SetColor(
                        "_BaseColor",
                        coloresOriginales[r][m]
                    );
                }
                else if (mat.HasProperty("_Color"))
                {
                    mat.SetColor(
                        "_Color",
                        coloresOriginales[r][m]
                    );
                }


                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor(
                        "_EmissionColor",
                        emisionesOriginales[r][m]
                    );
                }
            }
        }
    }
}