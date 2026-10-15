using UnityEngine;

[RequireComponent(typeof(Animator))]
public class MiradaMonstruo : MonoBehaviour
{
    [Header("Objetivo")]
    public Transform jugador;

    [Header("Control")]
    public bool mirar = false;
    public float velocidadMirada = 0.8f;

    private Animator animator;
    private float pesoActual = 0f;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float objetivo = mirar ? 1f : 0f;

        pesoActual = Mathf.MoveTowards(
            pesoActual,
            objetivo,
            velocidadMirada * Time.deltaTime
        );
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null || jugador == null)
            return;

        animator.SetLookAtPosition(jugador.position);

        animator.SetLookAtWeight(
            pesoActual,
            0f,     // CUERPO = NO GIRAR
            1f,     // CABEZA = GIRAR
            0f,     // OJOS
            0.35f   // límite para no romper el cuello
        );
    }
}