
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.SceneManagement;

using Debug = UnityEngine.Debug;

public class LanzadorJuegoPCVR : MonoBehaviour
{
    // ============================================
    // ESTADO
    // ============================================

    private bool abriendo = false;
    private bool salioHaciaJuego = false;
    private bool lobbyPausado = false;
    private bool reiniciandoLobby = false;

    // ============================================
    // CONTROL IZQUIERDO - BOTON Y
    // ============================================

    private InputDevice controlIzquierdo;
    private bool yAnterior = false;

    private static bool esperarSoltarY = false;

    // ============================================
    // START
    // ============================================

    private void Start()
    {
        Application.runInBackground = true;
        BuscarControlIzquierdo();

        Debug.Log("LOBBY INICIADO");
    }

    // ============================================
    // DETECTAR BOTON Y
    // ============================================

    private void Update()
    {
        if (reiniciandoLobby || abriendo)
            return;

        if (!controlIzquierdo.isValid)
        {
            BuscarControlIzquierdo();
            yAnterior = false;
        }

        if (!controlIzquierdo.isValid)
            return;

        bool yPresionado = false;

        bool recibido =
            controlIzquierdo.TryGetFeatureValue(
                CommonUsages.secondaryButton,
                out yPresionado
            );

        if (!recibido)
            return;

        // Evitar reinicios repetidos si Y sigue
        // presionado despues de recargar la escena.

        if (esperarSoltarY)
        {
            if (!yPresionado)
            {
                esperarSoltarY = false;
            }

            yAnterior = yPresionado;
            return;
        }

        if (yPresionado && !yAnterior)
        {
            Debug.Log(
                "BOTON Y PRESIONADO - REINICIANDO LOBBY"
            );

            esperarSoltarY = true;
            ReiniciarLobby();
            return;
        }

        yAnterior = yPresionado;
    }

    private void BuscarControlIzquierdo()
    {
        controlIzquierdo =
            InputDevices.GetDeviceAtXRNode(
                XRNode.LeftHand
            );
    }

    // ============================================
    // DETECTAR REGRESO DESDE OTRO JUEGO
    // ============================================

    private void OnApplicationPause(bool pausado)
    {
        if (!salioHaciaJuego || reiniciandoLobby)
            return;

        if (pausado)
        {
            lobbyPausado = true;

            Debug.Log(
                "LOBBY PAUSADO - JUEGO EXTERNO"
            );
        }
        else if (lobbyPausado)
        {
            ReiniciarLobby();
        }
    }

    // ============================================
    // RESPALDO AL RECUPERAR EL FOCO
    // ============================================

    private void OnApplicationFocus(bool tieneFoco)
    {
        if (!salioHaciaJuego || reiniciandoLobby)
            return;

        if (!tieneFoco)
        {
            lobbyPausado = true;
        }
        else if (lobbyPausado)
        {
            ReiniciarLobby();
        }
    }

    // ============================================
    // REINICIAR LOBBY
    // ============================================

    public void ReiniciarLobby()
    {
        if (reiniciandoLobby)
            return;

        reiniciandoLobby = true;

        salioHaciaJuego = false;
        lobbyPausado = false;
        abriendo = false;

        Debug.Log(
            "REINICIANDO ESCENA DEL LOBBY"
        );

        Scene escena =
            SceneManager.GetActiveScene();

        SceneManager.LoadScene(
            escena.buildIndex
        );
    }

    // ============================================
    // COMPATIBILIDAD CON DOS PARAMETROS
    // ============================================

    public void LanzarDestino(
        string rutaPC,
        string packageName)
    {
        LanzarDestino(
            rutaPC,
            packageName,
            "com.unity3d.player.UnityPlayerGameActivity"
        );
    }

    // ============================================
    // LANZADOR UNIVERSAL
    // ============================================

    public void LanzarDestino(
        string rutaPC,
        string packageName,
        string activityName)
    {
        if (abriendo || reiniciandoLobby)
            return;

#if UNITY_ANDROID && !UNITY_EDITOR

        // ========================================
        // META QUEST STANDALONE
        // ========================================

        if (string.IsNullOrWhiteSpace(packageName) ||
            string.IsNullOrWhiteSpace(activityName))
        {
            Debug.LogError(
                "FALTA PACKAGE NAME O ACTIVITY NAME"
            );
            return;
        }

        abriendo = true;

        bool solicitudEnviada = false;

        try
        {
            using (
                AndroidJavaClass unityPlayer =
                    new AndroidJavaClass(
                        "com.unity3d.player.UnityPlayer"
                    )
            )
            using (
                AndroidJavaObject activity =
                    unityPlayer.GetStatic<AndroidJavaObject>(
                        "currentActivity"
                    )
            )
            using (
                AndroidJavaObject intent =
                    new AndroidJavaObject(
                        "android.content.Intent"
                    )
            )
            {
                if (activity == null)
                {
                    throw new Exception(
                        "NO SE ENCONTRO currentActivity"
                    );
                }

                // ====================================
                // PACKAGE NAME + ACTIVITY NAME
                // ====================================

                intent.Call<AndroidJavaObject>(
                    "setClassName",
                    packageName.Trim(),
                    activityName.Trim()
                );

                // FLAG_ACTIVITY_NEW_TASK

                intent.Call<AndroidJavaObject>(
                    "addFlags",
                    0x10000000
                );

                Debug.Log(
                    "ABRIENDO APK: " + packageName
                );

                // Preparar deteccion de regreso

                salioHaciaJuego = true;
                lobbyPausado = false;

                // ====================================
                // ABRIR APK
                // ====================================

                activity.Call(
                    "startActivity",
                    intent
                );

                solicitudEnviada = true;

                Debug.Log(
                    "SOLICITUD DE APERTURA ENVIADA: " +
                    packageName
                );

                // No llamar finish ni Application.Quit.
                // Queremos que Android pueda conservar
                // y reanudar el lobby.
            }
        }
        catch (Exception error)
        {
            Debug.LogError(
                "ERROR ABRIENDO APK:\n" + error
            );

            salioHaciaJuego = false;
            lobbyPausado = false;
        }

        if (!solicitudEnviada)
        {
            Debug.LogWarning(
                "NO SE PUDO SOLICITAR LA APERTURA."
            );
        }

        abriendo = false;

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        // ========================================
        // WINDOWS PCVR / QUEST LINK
        // ========================================

        if (string.IsNullOrWhiteSpace(rutaPC))
        {
            Debug.LogError(
                "FALTA RUTA DEL EXE"
            );
            return;
        }

        StartCoroutine(
            AbrirJuegoWindows(rutaPC)
        );

#else

        Debug.LogError(
            "PLATAFORMA NO SOPORTADA: " +
            Application.platform
        );

#endif
    }

    // ============================================
    // ABRIR JUEGO EN WINDOWS
    // ============================================

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    private IEnumerator AbrirJuegoWindows(string rutaPC)
    {
        abriendo = true;

        string rutaCompleta = "";

        try
        {
            string carpeta =
                Directory.GetParent(
                    Application.dataPath
                ).FullName;

            rutaCompleta =
                Path.GetFullPath(
                    Path.Combine(
                        carpeta,
                        rutaPC.Trim()
                    )
                );
        }
        catch (Exception error)
        {
            Debug.LogError(
                "ERROR EN RUTA PC:\n" + error
            );
        }

        if (string.IsNullOrEmpty(rutaCompleta) ||
            !File.Exists(rutaCompleta))
        {
            Debug.LogError(
                "NO EXISTE EXE: " + rutaCompleta
            );

            abriendo = false;
            yield break;
        }

        // ========================================
        // LIBERAR XR EN WINDOWS
        // ========================================

        XRManagerSettings xr = null;

        if (XRGeneralSettings.Instance != null)
        {
            xr =
                XRGeneralSettings.Instance.Manager;
        }

        bool liberarXR =
            xr != null && xr.isInitializationComplete;

        if (liberarXR)
        {
            xr.StopSubsystems();
            xr.DeinitializeLoader();

            yield return
                new WaitForSecondsRealtime(0.8f);
        }

        // ========================================
        // ABRIR EXE
        // ========================================

        Process proceso = null;

        try
        {
            ProcessStartInfo info =
                new ProcessStartInfo();

            info.FileName = rutaCompleta;

            info.WorkingDirectory =
                Path.GetDirectoryName(rutaCompleta);

            info.UseShellExecute = false;

            proceso = Process.Start(info);

            Debug.Log(
                "EXE SOLICITADO: " + rutaCompleta
            );
        }
        catch (Exception error)
        {
            Debug.LogError(
                "ERROR ABRIENDO EXE:\n" + error
            );
        }

        if (proceso == null)
        {
            if (liberarXR)
            {
                yield return StartCoroutine(
                    RecuperarXR(xr)
                );
            }

            abriendo = false;
            yield break;
        }

        yield return
            new WaitForSecondsRealtime(1.5f);

        bool sigueAbierto = false;

        try
        {
            sigueAbierto = !proceso.HasExited;
        }
        catch (Exception error)
        {
            Debug.LogWarning(
                "NO SE PUDO COMPROBAR EXE: " +
                error.Message
            );
        }

        proceso.Dispose();

        if (!sigueAbierto)
        {
            Debug.LogError(
                "EL EXE SE CERRO AL ARRANCAR"
            );

            if (liberarXR)
            {
                yield return StartCoroutine(
                    RecuperarXR(xr)
                );
            }

            abriendo = false;
            yield break;
        }

        Debug.Log(
            "JUEGO ABIERTO. CERRANDO LOBBY PC."
        );

#if UNITY_EDITOR

        UnityEditor.EditorApplication.isPlaying = false;

#else

        Application.Quit();

#endif
    }

    // ============================================
    // RECUPERAR XR EN WINDOWS
    // ============================================

    private IEnumerator RecuperarXR(
        XRManagerSettings xr)
    {
        if (xr == null)
            yield break;

        if (xr.activeLoader == null)
        {
            yield return xr.InitializeLoader();
        }

        if (xr.activeLoader != null)
        {
            xr.StartSubsystems();

            Debug.Log(
                "XR RECUPERADO"
            );
        }
        else
        {
            Debug.LogError(
                "NO SE PUDO RECUPERAR XR"
            );
        }
    }

#endif
}
