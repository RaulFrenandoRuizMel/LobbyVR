using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;


// WINDOWS SOLAMENTE
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

using System.Runtime.InteropServices;
using System.Text;

#endif


public class LanzadorJuegoPCVR : MonoBehaviour
{
    // =====================================================
    // DESTINO DEL BOTON Y
    // =====================================================
    //
    // NO HAY CHECKBOX.
    //
    // Si asignas Destino Boton Y:
    //      Y ejecuta ese destino.
    //
    // Si queda en None:
    //      Y no hace nada.
    //
    // El mismo DestinoJuegoPuerta puede contener:
    //
    // PC:
    // ..\..\LOBBYEXES\LobbyCasitasDelTerror.exe
    //
    // QUEST:
    // com.tuproyecto.lobby
    //
    // =====================================================

    [Header("Regreso con boton Y")]

    [Tooltip(
        "Arrastra aqui el DestinoJuegoPuerta para regresar. " +
        "Si queda vacio, Y no hace nada."
    )]
    public DestinoJuegoPuerta destinoBotonY;


    // =====================================================
    // TIEMPOS
    // =====================================================

    [Header("Tiempos")]

    public float tiempoLiberarXR = 0.8f;

    public float tiempoComprobarDestino = 1.5f;


    // =====================================================
    // ESTADO
    // =====================================================

    private bool cambiandoAplicacion = false;

    private InputDevice controlIzquierdo;

    private bool yAnterior = false;


    // =====================================================
    // WINDOWS API
    // =====================================================

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFO
    {
        public int cb;

        public System.IntPtr lpReserved;
        public System.IntPtr lpDesktop;
        public System.IntPtr lpTitle;

        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;

        public short wShowWindow;
        public short cbReserved2;

        public System.IntPtr lpReserved2;
        public System.IntPtr hStdInput;
        public System.IntPtr hStdOutput;
        public System.IntPtr hStdError;
    }


    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public System.IntPtr hProcess;

        public System.IntPtr hThread;

        public int dwProcessId;

        public int dwThreadId;
    }


    [DllImport(
        "kernel32.dll",
        CharSet = CharSet.Unicode,
        SetLastError = true
    )]
    private static extern bool CreateProcessW(
        string lpApplicationName,
        StringBuilder lpCommandLine,
        System.IntPtr lpProcessAttributes,
        System.IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        System.IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation
    );


    [DllImport(
        "kernel32.dll",
        SetLastError = true
    )]
    private static extern uint WaitForSingleObject(
        System.IntPtr hHandle,
        uint dwMilliseconds
    );


    [DllImport(
        "kernel32.dll",
        SetLastError = true
    )]
    private static extern bool CloseHandle(
        System.IntPtr hObject
    );


    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentProcessId();


    [DllImport(
        "kernel32.dll",
        SetLastError = true
    )]
    private static extern System.IntPtr OpenProcess(
        uint dwDesiredAccess,
        bool bInheritHandle,
        uint dwProcessId
    );


    [DllImport(
        "kernel32.dll",
        SetLastError = true
    )]
    private static extern bool TerminateProcess(
        System.IntPtr hProcess,
        uint uExitCode
    );


    private const uint WAIT_TIMEOUT =
        0x00000102;


    private const uint PROCESS_TERMINATE =
        0x0001;

#endif


    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        Application.runInBackground = true;


        Debug.Log(
            "===================================="
        );


        Debug.Log(
            "PLATAFORMA DETECTADA: " +
            Application.platform
        );


#if UNITY_ANDROID && !UNITY_EDITOR

        Debug.Log(
            "MODO: META QUEST STANDALONE / APK"
        );

#elif UNITY_STANDALONE_WIN

        Debug.Log(
            "MODO: WINDOWS PCVR / QUEST LINK"
        );

#elif UNITY_EDITOR_WIN

        Debug.Log(
            "MODO: UNITY EDITOR WINDOWS"
        );

#else

        Debug.Log(
            "MODO: PLATAFORMA NO SOPORTADA"
        );

#endif


        Debug.Log(
            "===================================="
        );


        BuscarControlIzquierdo();
    }


    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        // ---------------------------------------------
        // Si no existe destino para Y,
        // no necesitamos leer el boton.
        // ---------------------------------------------

        if (destinoBotonY == null)
            return;


        if (!controlIzquierdo.isValid)
        {
            BuscarControlIzquierdo();
        }


        LeerBotonY();
    }


    // =====================================================
    // BUSCAR CONTROL IZQUIERDO
    // =====================================================

    void BuscarControlIzquierdo()
    {
        controlIzquierdo =
            InputDevices.GetDeviceAtXRNode(
                XRNode.LeftHand
            );


        if (controlIzquierdo.isValid)
        {
            Debug.Log(
                "CONTROL IZQUIERDO DETECTADO: " +
                controlIzquierdo.name
            );
        }
    }


    // =====================================================
    // BOTON Y
    // =====================================================

    void LeerBotonY()
    {
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


        // ---------------------------------------------
        // Solo cuando cambia de NO presionado
        // a presionado.
        // ---------------------------------------------

        if (yPresionado && !yAnterior)
        {
            Debug.Log(
                "BOTON Y IZQUIERDO PRESIONADO"
            );


            if (destinoBotonY != null)
            {
                destinoBotonY
                    .EjecutarDestino();
            }
        }


        yAnterior =
            yPresionado;
    }


    // =====================================================
    // LANZADOR UNIVERSAL
    // =====================================================
    //
    // ESTA ES LA FUNCION QUE DEBE USAR
    // DestinoJuegoPuerta.
    //
    // Recibe DOS posibles destinos:
    //
    // PCVR:
    //
    // ..\Equipos\EXEEQUIPO4\Equipo4.exe
    //
    //
    // META QUEST:
    //
    // com.equipo4.juego
    //
    //
    // El lanzador decide automaticamente
    // cual debe utilizar.
    // =====================================================

    public void LanzarDestino(
        string rutaRelativaPC,
        string paqueteQuest
    )
    {
        if (cambiandoAplicacion)
        {
            Debug.LogWarning(
                "YA HAY UN CAMBIO DE APLICACION EN CURSO"
            );

            return;
        }


        // =================================================
        // META QUEST STANDALONE
        // =================================================

#if UNITY_ANDROID && !UNITY_EDITOR

        Debug.Log(
            "DESTINO DETECTADO COMO QUEST APK"
        );


        if (string.IsNullOrWhiteSpace(
            paqueteQuest
        ))
        {
            Debug.LogError(
                "EL PACKAGE NAME DE QUEST ESTA VACIO"
            );

            return;
        }


        StartCoroutine(
            CambiarAplicacionQuest(
                paqueteQuest.Trim()
            )
        );


        // =================================================
        // WINDOWS / QUEST LINK
        // =================================================

#elif UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        Debug.Log(
            "DESTINO DETECTADO COMO PCVR / QUEST LINK"
        );


        LanzarRuta(
            rutaRelativaPC
        );


        // =================================================
        // OTRA PLATAFORMA
        // =================================================

#else

        Debug.LogError(
            "PLATAFORMA NO SOPORTADA: " +
            Application.platform
        );

#endif
    }


    // =====================================================
    // PCVR
    // =====================================================
    //
    // Conservamos LanzarRuta para que no rompamos
    // el sistema que ya teniamos.
    //
    // =====================================================

    public void LanzarRuta(
        string rutaRelativa
    )
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

        if (cambiandoAplicacion)
        {
            Debug.LogWarning(
                "YA HAY UN CAMBIO DE APLICACION EN CURSO"
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(
            rutaRelativa
        ))
        {
            Debug.LogError(
                "LA RUTA RELATIVA DE PC ESTA VACIA"
            );

            return;
        }


        rutaRelativa =
            rutaRelativa.Trim();


        // =================================================
        // CARPETA DEL EXE ACTUAL
        // =================================================

        string carpetaExeActual =
            Directory.GetParent(
                Application.dataPath
            ).FullName;


        // =================================================
        // RUTA RELATIVA -> RUTA ABSOLUTA
        // =================================================

        string rutaCompleta =
            Path.GetFullPath(
                Path.Combine(
                    carpetaExeActual,
                    rutaRelativa
                )
            );


        Debug.Log(
            "===================================="
        );


        Debug.Log(
            "MODO PCVR"
        );


        Debug.Log(
            "CARPETA DEL EXE ACTUAL:\n" +
            carpetaExeActual
        );


        Debug.Log(
            "RUTA RELATIVA:\n" +
            rutaRelativa
        );


        Debug.Log(
            "RUTA FINAL RESUELTA:\n" +
            rutaCompleta
        );


        Debug.Log(
            "===================================="
        );


        StartCoroutine(
            CambiarAplicacionWindows(
                rutaCompleta
            )
        );

#else

        Debug.LogError(
            "LanzarRuta solamente funciona " +
            "en Windows PCVR."
        );

#endif
    }


    // =====================================================
    // WINDOWS - CAMBIAR APLICACION
    // =====================================================

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

    IEnumerator CambiarAplicacionWindows(
        string rutaDestino
    )
    {
        cambiandoAplicacion = true;


        // =================================================
        // 1. VALIDAR EXE
        // =================================================

        if (!File.Exists(
            rutaDestino
        ))
        {
            Debug.LogError(
                "NO EXISTE EL EXE:\n" +
                rutaDestino
            );


            cambiandoAplicacion = false;

            yield break;
        }


        Debug.Log(
            "EXE ENCONTRADO:\n" +
            rutaDestino
        );


        // =================================================
        // 2. LIBERAR OPENXR
        // =================================================

        XRManagerSettings manager = null;


        if (XRGeneralSettings.Instance != null)
        {
            manager =
                XRGeneralSettings
                    .Instance
                    .Manager;
        }


        if (manager != null &&
            manager.isInitializationComplete)
        {
            Debug.Log(
                "LIBERANDO OPENXR..."
            );


            manager.StopSubsystems();

            manager.DeinitializeLoader();
        }


        yield return
            new WaitForSecondsRealtime(
                tiempoLiberarXR
            );


        // =================================================
        // 3. PREPARAR PROCESO
        // =================================================

        STARTUPINFO si =
            new STARTUPINFO();


        si.cb =
            Marshal.SizeOf(
                typeof(STARTUPINFO)
            );


        StringBuilder commandLine =
            new StringBuilder(
                "\"" +
                rutaDestino +
                "\""
            );


        PROCESS_INFORMATION pi;


        string carpetaTrabajo =
            Path.GetDirectoryName(
                rutaDestino
            );


        // =================================================
        // 4. LANZAR EXE
        // =================================================

        bool abierto =
            CreateProcessW(
                rutaDestino,
                commandLine,
                System.IntPtr.Zero,
                System.IntPtr.Zero,
                false,
                0,
                System.IntPtr.Zero,
                carpetaTrabajo,
                ref si,
                out pi
            );


        // =================================================
        // 5. ERROR
        // =================================================

        if (!abierto)
        {
            int error =
                Marshal.GetLastWin32Error();


            Debug.LogError(
                "CreateProcessW FALLO"
            );


            Debug.LogError(
                "ERROR WINDOWS: " +
                error
            );


            Debug.LogError(
                "RUTA:\n" +
                rutaDestino
            );


            cambiandoAplicacion = false;


            yield return
                StartCoroutine(
                    RecuperarXR()
                );


            yield break;
        }


        CloseHandle(
            pi.hThread
        );


        Debug.Log(
            "EXE LANZADO"
        );


        Debug.Log(
            "PID: " +
            pi.dwProcessId
        );


        Debug.Log(
            "RUTA REAL EJECUTADA:\n" +
            rutaDestino
        );


        // =================================================
        // 6. ESPERAR
        // =================================================

        yield return
            new WaitForSecondsRealtime(
                tiempoComprobarDestino
            );


        // =================================================
        // 7. COMPROBAR QUE SIGUE VIVO
        // =================================================

        bool sigueVivo =
            WaitForSingleObject(
                pi.hProcess,
                0
            ) == WAIT_TIMEOUT;


        if (!sigueVivo)
        {
            Debug.LogError(
                "EL JUEGO NUEVO SE CERRO AL ARRANCAR"
            );


            CloseHandle(
                pi.hProcess
            );


            cambiandoAplicacion = false;


            yield return
                StartCoroutine(
                    RecuperarXR()
                );


            yield break;
        }


        Debug.Log(
            "NUEVO JUEGO CONFIRMADO"
        );


        CloseHandle(
            pi.hProcess
        );


        // =================================================
        // 8. CERRAR APP ACTUAL
        // =================================================

        MatarProcesoActualWindows();
    }


    // =====================================================
    // MATAR PROCESO WINDOWS
    // =====================================================

    void MatarProcesoActualWindows()
    {
#if UNITY_EDITOR

        Debug.Log(
            "EDITOR: DETENIENDO PLAY MODE"
        );


        UnityEditor.EditorApplication.isPlaying =
            false;

#else

        uint pid =
            GetCurrentProcessId();


        System.IntPtr handle =
            OpenProcess(
                PROCESS_TERMINATE,
                false,
                pid
            );


        if (handle != System.IntPtr.Zero)
        {
            Debug.Log(
                "CERRANDO PROCESO ACTUAL PID: " +
                pid
            );


            bool terminado =
                TerminateProcess(
                    handle,
                    0
                );


            if (!terminado)
            {
                Application.Quit();
            }


            CloseHandle(
                handle
            );
        }
        else
        {
            Application.Quit();
        }

#endif
    }

#endif


    // =====================================================
    // META QUEST STANDALONE
    // =====================================================

#if UNITY_ANDROID && !UNITY_EDITOR

    IEnumerator CambiarAplicacionQuest(
        string paqueteQuest
    )
    {
        cambiandoAplicacion = true;


        if (string.IsNullOrWhiteSpace(
            paqueteQuest
        ))
        {
            Debug.LogError(
                "PACKAGE NAME VACIO"
            );


            cambiandoAplicacion = false;

            yield break;
        }


        Debug.Log(
            "===================================="
        );


        Debug.Log(
            "MODO META QUEST STANDALONE"
        );


        Debug.Log(
            "PACKAGE DESTINO:\n" +
            paqueteQuest
        );


        Debug.Log(
            "===================================="
        );


        // =================================================
        // 1. LIBERAR OPENXR
        // =================================================

        XRManagerSettings manager = null;


        if (XRGeneralSettings.Instance != null)
        {
            manager =
                XRGeneralSettings
                    .Instance
                    .Manager;
        }


        if (manager != null &&
            manager.isInitializationComplete)
        {
            Debug.Log(
                "LIBERANDO OPENXR..."
            );


            manager.StopSubsystems();

            manager.DeinitializeLoader();
        }


        yield return
            new WaitForSecondsRealtime(
                tiempoLiberarXR
            );


        // =================================================
        // 2. INTENTAR ABRIR APK
        // =================================================

        bool abierto =
            false;


        try
        {
            using (
                AndroidJavaClass unityPlayer =
                    new AndroidJavaClass(
                        "com.unity3d.player.UnityPlayer"
                    )
            )
            {
                AndroidJavaObject actividadActual =
                    unityPlayer
                        .GetStatic<AndroidJavaObject>(
                            "currentActivity"
                        );


                if (actividadActual == null)
                {
                    Debug.LogError(
                        "NO SE ENCONTRO currentActivity"
                    );
                }
                else
                {
                    AndroidJavaObject packageManager =
                        actividadActual
                            .Call<AndroidJavaObject>(
                                "getPackageManager"
                            );


                    AndroidJavaObject intent =
                        null;


                    // =====================================
                    // PRIMER INTENTO:
                    // Launch Intent oficial del paquete
                    // =====================================

                    if (packageManager != null)
                    {
                        intent =
                            packageManager
                                .Call<AndroidJavaObject>(
                                    "getLaunchIntentForPackage",
                                    paqueteQuest
                                );
                    }


                    // =====================================
                    // SEGUNDO INTENTO:
                    // ACTION_MAIN + CATEGORY_LAUNCHER
                    // limitado al package.
                    // =====================================

                    if (intent == null)
                    {
                        Debug.LogWarning(
                            "getLaunchIntentForPackage " +
                            "NO DEVOLVIO INTENT. " +
                            "PROBANDO INTENT DIRECTO."
                        );


                        intent =
                            new AndroidJavaObject(
                                "android.content.Intent",
                                "android.intent.action.MAIN"
                            );


                        intent.Call<AndroidJavaObject>(
                            "addCategory",
                            "android.intent.category.LAUNCHER"
                        );


                        intent.Call<AndroidJavaObject>(
                            "setPackage",
                            paqueteQuest
                        );
                    }


                    if (intent != null)
                    {
                        // FLAG_ACTIVITY_NEW_TASK
                        intent.Call<AndroidJavaObject>(
                            "addFlags",
                            0x10000000
                        );


                        Debug.Log(
                            "ABRIENDO APK:\n" +
                            paqueteQuest
                        );


                        actividadActual.Call(
                            "startActivity",
                            intent
                        );


                        abierto =
                            true;


                        Debug.Log(
                            "APK SOLICITADA CORRECTAMENTE"
                        );


                        // ---------------------------------
                        // Cerramos la actividad actual.
                        // El nuevo juego queda al frente.
                        // ---------------------------------

                        actividadActual.Call(
                            "finish"
                        );


                        intent.Dispose();
                    }


                    if (packageManager != null)
                    {
                        packageManager.Dispose();
                    }


                    actividadActual.Dispose();
                }
            }
        }
        catch (
            System.Exception error
        )
        {
            Debug.LogError(
                "ERROR ABRIENDO APK:\n" +
                paqueteQuest +
                "\n\n" +
                error
            );


            abierto =
                false;
        }


        // =================================================
        // 3. SI FALLO, RECUPERAR XR
        // =================================================

        if (!abierto)
        {
            Debug.LogError(
                "NO SE PUDO ABRIR LA APK:\n" +
                paqueteQuest
            );


            cambiandoAplicacion =
                false;


            yield return
                StartCoroutine(
                    RecuperarXR()
                );


            yield break;
        }


        // En Android ya se termino nuestra Activity.
    }

#endif


    // =====================================================
    // RECUPERAR XR
    // =====================================================

    IEnumerator RecuperarXR()
    {
        Debug.Log(
            "RECUPERANDO XR..."
        );


        if (XRGeneralSettings.Instance == null)
        {
            yield break;
        }


        XRManagerSettings manager =
            XRGeneralSettings
                .Instance
                .Manager;


        if (manager == null)
        {
            yield break;
        }


        yield return
            manager.InitializeLoader();


        if (manager.activeLoader == null)
        {
            Debug.LogError(
                "NO SE PUDO RECUPERAR XR"
            );


            yield break;
        }


        manager.StartSubsystems();


        Debug.Log(
            "XR RECUPERADO"
        );
    }
}