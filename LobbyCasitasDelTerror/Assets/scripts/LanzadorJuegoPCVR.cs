using System.Collections;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Management;


public class LanzadorJuegoPCVR : MonoBehaviour
{
    // =====================================================
    // META QUEST - BOTON Y
    // =====================================================

    [Header("Regreso con control Meta Quest")]

    [Tooltip(
        "OFF en el Lobby. " +
        "ON dentro de los juegos si quieres regresar con Y."
    )]
    public bool usarYControlIzquierdo = false;


    [Tooltip(
        "Destino externo que se ejecutará al presionar Y. " +
        "Por ejemplo un DestinoJuegoPuerta cuya ruta sea el Lobby."
    )]
    public DestinoJuegoPuerta destinoBotonY;


    // =====================================================
    // TIEMPOS
    // =====================================================

    [Header("Tiempos")]

    public float tiempoLiberarXR = 0.8f;

    public float tiempoComprobarDestino = 1.5f;


    // =====================================================
    // PRIVADAS
    // =====================================================

    private bool cambiandoAplicacion = false;

    private InputDevice controlIzquierdo;

    private bool yAnterior = false;


    // =====================================================
    // WINDOWS API
    // =====================================================

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


    // =====================================================
    // START
    // =====================================================

    void Start()
    {
        Application.runInBackground = true;


        if (usarYControlIzquierdo)
        {
            BuscarControlIzquierdo();
        }
    }


    // =====================================================
    // UPDATE
    // =====================================================

    void Update()
    {
        if (!usarYControlIzquierdo)
            return;


        if (!controlIzquierdo.isValid)
        {
            BuscarControlIzquierdo();
        }


        LeerBotonY();
    }


    // =====================================================
    // CONTROL IZQUIERDO
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
        bool yPresionado = false;


        bool recibido =
            controlIzquierdo.TryGetFeatureValue(
                CommonUsages.secondaryButton,
                out yPresionado
            );


        if (!recibido)
            return;


        if (yPresionado && !yAnterior)
        {
            Debug.Log(
                "BOTON Y IZQUIERDO PRESIONADO"
            );


            if (destinoBotonY != null)
            {
                destinoBotonY.EjecutarDestino();
            }
            else
            {
                Debug.LogError(
                    "NO HAY DESTINO ASIGNADO AL BOTON Y"
                );
            }
        }


        yAnterior =
            yPresionado;
    }


    // =====================================================
    // ÚNICA ENTRADA DE RUTA
    // =====================================================
    //
    // Este Lanzador NO conoce Equipo1, Equipo2, etc.
    //
    // Solo recibe:
    //
    // E:\Carpeta\Juego.exe
    //
    // desde DestinoJuegoPuerta.
    // =====================================================

    public void LanzarRuta(string rutaRelativa)
    {
        if (cambiandoAplicacion)
        {
            Debug.LogWarning(
                "YA HAY UN CAMBIO DE APLICACION EN CURSO"
            );

            return;
        }


        if (string.IsNullOrWhiteSpace(rutaRelativa))
        {
            Debug.LogError(
                "LA RUTA RELATIVA ESTA VACIA"
            );

            return;
        }


        // =====================================================
        // OBTENER CARPETA DEL EXE QUE ESTA CORRIENDO
        // =====================================================

        string carpetaExeActual =
            Directory.GetParent(
                Application.dataPath
            ).FullName;


        // =====================================================
        // CONVERTIR RUTA RELATIVA EN RUTA REAL
        // =====================================================

        string rutaCompleta =
            Path.GetFullPath(
                Path.Combine(
                    carpetaExeActual,
                    rutaRelativa
                )
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


        StartCoroutine(
            CambiarAplicacion(
                rutaCompleta
            )
        );
    }


    // =====================================================
    // CAMBIAR APLICACION
    // =====================================================

    IEnumerator CambiarAplicacion(
        string rutaDestino
    )
    {
        cambiandoAplicacion = true;


        // =================================================
        // 1. VALIDAR RUTA
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
        // 7. CONFIRMAR NUEVO PROCESO
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

        MatarProcesoActual();
    }


    // =====================================================
    // MATAR PROCESO ACTUAL
    // =====================================================

    void MatarProcesoActual()
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