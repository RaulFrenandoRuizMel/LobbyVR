using UnityEngine;

public class CuboFade : MonoBehaviour
{
    Transform camara;
    MeshRenderer meshRenderer;
    Color color;
    int Estado;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        camara = GameObject.FindGameObjectWithTag("MainCamera").transform;

        meshRenderer = this.GetComponent<MeshRenderer>();
        color = meshRenderer.material.color;
        
        Estado = 0;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        switch (Estado)
        {
            case 0: // De opaco a transparente
                color.a -= Time.deltaTime;
                if(color.a <= 0)
                {
                    color.a = 0;
                    Estado = 1;
                    meshRenderer.enabled = false;
                }
                meshRenderer.material.color = color;
                break;
        }


        this.transform.position = camara.position;
        this.transform.rotation = camara.rotation;
    }
}
