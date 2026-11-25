using UnityEngine;
using DG.Tweening;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class MoverCamara : MonoBehaviour
{
    [SerializeField] Transform punto1;
    [SerializeField] Transform punto2;
    [SerializeField] Transform punto3;
    [SerializeField] Transform punto4;
    [SerializeField] Transform punto5;
    [SerializeField] Transform punto6;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.transform.position = punto1.position;
        this.transform.rotation = punto1.rotation;

        //this.transform.LookAt(punto1.position + Vector3.up * 1.5f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            
            this.transform.DOMove(punto2.position, 4).SetEase(Ease.InOutCubic);
            this.transform.DORotate(punto2.rotation.eulerAngles, 4).SetEase(Ease.InOutCubic);
            

            //StartCoroutine(Animacion());
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    IEnumerator Animacion()
    {

        yield return this.transform.DOLookAt(punto2.position + Vector3.up * 1.5f, 2).SetEase(Ease.InOutQuad).WaitForCompletion();
        yield return this.transform.DOLookAt(punto3.position + Vector3.up * 1.5f, 2).SetEase(Ease.InOutQuad).WaitForCompletion();
        yield return this.transform.DOLookAt(punto4.position + Vector3.up * 1.5f, 2).SetEase(Ease.InOutQuad).WaitForCompletion();
        yield return this.transform.DOLookAt(punto5.position + Vector3.up * 1.5f, 2).SetEase(Ease.InOutQuad).WaitForCompletion();
        yield return this.transform.DOLookAt(punto6.position + Vector3.up * 1.5f, 2).SetEase(Ease.InOutQuad).WaitForCompletion();
    }

}
