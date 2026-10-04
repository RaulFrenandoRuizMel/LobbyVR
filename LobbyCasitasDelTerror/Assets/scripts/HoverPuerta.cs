using UnityEngine;

public class HoverPuerta : MonoBehaviour
{
    [Header("Modelo amarillo al seleccionar")]
    public GameObject modeloSeleccionado;

    void Awake()
    {
        if (modeloSeleccionado != null)
        {
            modeloSeleccionado.SetActive(false);
        }
    }

    public void ActivarHover()
    {
        if (modeloSeleccionado == null)
            return;

        modeloSeleccionado.SetActive(true);

        Debug.Log(
            gameObject.name +
            " >>> HOVER ACTIVADO"
        );
    }

    public void DesactivarHover()
    {
        if (modeloSeleccionado == null)
            return;

        modeloSeleccionado.SetActive(false);

        Debug.Log(
            gameObject.name +
            " >>> HOVER DESACTIVADO"
        );
    }
}