using UnityEngine;

public class NumerosFluorecentes : MonoBehaviour
{
    MeshRenderer meshRenderer;
    Color color;
    Material material;
    float intensity = 2.749785f;

    float angulo;
    float centroIntensidad;
    float radioIntensidad;
    float contador;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        meshRenderer = this.transform.GetChild(0).GetChild(0).GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        color = material.GetColor("_EmissionColor");
        material.SetColor("_EmissionColor", color * intensity);

        angulo = 0;

        centroIntensidad = Mathf.Lerp(0, intensity, 0.5f);
        radioIntensidad = Mathf.Abs(0 - centroIntensidad);

        contador = 180;
    }

    // Update is called once per frame
    void Update()
    {
        contador += Time.deltaTime;
        if(contador > 4)
        {
            angulo += Time.deltaTime * 50;
            if (angulo > 180)
            {
                angulo -= 360;
                contador = 0;
            }

            material.SetColor("_EmissionColor", color * (centroIntensidad + Mathf.Cos(angulo * Mathf.Deg2Rad) * radioIntensidad));
        }

        
        
        
    }
}
