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
    // DESTINOS DISPONIBLES
    // =====================================================

    public enum DestinoPCVR
    {
        Equipo1,
        Equipo2,
        Lobby
    }


    // Este destino se usa SOLO cuando llamamos LanzarJuego()
    // sin parámetro.
    // Principalmente para regresar con Y desde los juegos.
    [Header("Destino por defecto")]
    public DestinoPCVR destino = DestinoPCVR.Lobby;


    // =====================================================
    // RUTAS
    // =====================================================

    private const string RUTA_EQUIPO1 =
        @"E:\Casitaselterrorportable\Equipos\EXEEQUIPO2\Equipo2.exe";


    private const string RUTA_EQUIPO2 =
        @"E:\Casitaselterrorportable\Equipos\EXEEQUIPO2\Equipo2.exe";


    private const string RUTA_LOBBY =
        @"E:\Casitaselterrorportable\LOBBYEXES\LobbyCasitasDelTerror.exe";


    // =====================================================
    // REGRESO CON CONTROL META QUEST
    // =====================================================

    [Header("Regreso con control Meta Quest")]

    [Tooltip(
        "Activalo dentro de los juegos. " +
        "Y del control izquierdo ejecutara el destino por defecto."
    )]
    public bool usarYControlIzquierdo = false;


    // =====================================================
    // TIEMPOS
    // =====================================================

    [Header("Tiempos")]

    public float tiempoLiberarXR = 0.8f;

    public float tiempoComprobarDestino = 1.5f;


    // =====================================================
    // VARIABLES PRIVADAS
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
        else
        {
            Debug.LogWarning(
                "NO SE DETECTO CONTROL IZQUIERDO"
            );
        }
    }


    // =====================================================
    // BOTON Y DEL CONTROL IZQUIERDO
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
                "Y IZQUIERDA PRESIONADA"
            );


            // IMPORTANTE:
            // aquí usamos el destino por defecto.
            // En Equipo1/Equipo2 debe ser Lobby.
            LanzarJuego();
        }


        yAnterior =
            yPresionado;
    }


    // =====================================================
    // LANZAMIENTO POR DEFECTO
    // =====================================================
    //
    // Se usa principalmente desde los juegos con Y.
    // Usa el valor "destino" del Inspector.
    // =====================================================

    public void LanzarJuego()
    {
        LanzarJuego(
            destino
        );
    }


    // =====================================================
    // LANZAMIENTO SOLICITADO POR UNA PUERTA
    // =====================================================
    //
    // ManoHorror usa ESTE método.
    // Recibe explícitamente Equipo1, Equipo2, etc.
    // =====================================================

    public void LanzarJuego(
        DestinoPCVR destinoSolicitado
    )
    {
        if (cambiandoAplicacion)
        {
            Debug.LogWarning(
                "YA HAY UN CAMBIO DE APLICACION EN CURSO"
            );

            return;
        }


        Debug.Log(
            "LANZADOR RECIBIO DESDE LA PUERTA: " +
            destinoSolicitado
        );


        StartCoroutine(
            CambiarAplicacion(
                destinoSolicitado
            )
        );
    }


    // =====================================================
    // CAMBIAR APLICACION
    // =====================================================

    IEnumerator CambiarAplicacion(
        DestinoPCVR destinoSolicitado
    )
    {
        cambiandoAplicacion = true;


        // =================================================
        // OBTENER LA RUTA DEL DESTINO RECIBIDO
        // =================================================

        string rutaDestino =
            ObtenerRutaDestino(
                destinoSolicitado
            );


        Debug.Log(
            "DESTINO SOLICITADO: " +
            destinoSolicitado +
            "\nRUTA:\n" +
            rutaDestino
        );


        // =================================================
        // 1. COMPROBAR QUE EXISTE EL EXE
        // =================================================

        if (string.IsNullOrEmpty(
                rutaDestino
            ) ||
            !File.Exists(
                rutaDestino
            ))
        {
            Debug.LogError(
                "NO EXISTE EL EXE:\n" +
                rutaDestino
            );


            cambiandoAplicacion =
                false;


            yield break;
        }


        // =================================================
        // 2. LIBERAR OPENXR
        // =================================================

        XRManagerSettings manager =
            null;


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
        // 3. CREAR EL NUEVO PROCESO
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


        bool abierto =
            CreateProcessW(
                rutaDestino,
                commandLine,
                System.IntPtr.Zero,
                System.IntPtr.Zero,
                false,
                0,
                System.IntPtr.Zero,
                Path.GetDirectoryName(
                    rutaDestino
                ),
                ref si,
                out pi
            );


        // =================================================
        // 4. ERROR AL ABRIR
        // =================================================

        if (!abierto)
        {
            int error =
                Marshal.GetLastWin32Error();


            Debug.LogError(
                "CreateProcessW FALLO. ERROR: " +
                error +
                "\nDESTINO: " +
                destinoSolicitado +
                "\nRUTA:\n" +
                rutaDestino
            );


            cambiandoAplicacion =
                false;


            yield return
                StartCoroutine(
                    RecuperarXR()
                );


            yield break;
        }


        // El thread handle ya no lo ocupamos.
        CloseHandle(
            pi.hThread
        );


        Debug.Log(
            destinoSolicitado +
            " LANZADO. PID: " +
            pi.dwProcessId
        );


        // =================================================
        // 5. ESPERAR QUE ARRANQUE
        // =================================================

        yield return
            new WaitForSecondsRealtime(
                tiempoComprobarDestino
            );


        // =================================================
        // 6. CONFIRMAR QUE SIGUE VIVO
        // =================================================

        bool sigueVivo =
            WaitForSingleObject(
                pi.hProcess,
                0
            ) == WAIT_TIMEOUT;


        if (!sigueVivo)
        {
            Debug.LogError(
                destinoSolicitado +
                " SE CERRO AL ARRANCAR. " +
                "NO CERRARE LA APP ACTUAL."
            );


            CloseHandle(
                pi.hProcess
            );


            cambiandoAplicacion =
                false;


            yield return
                StartCoroutine(
                    RecuperarXR()
                );


            yield break;
        }


        Debug.Log(
            destinoSolicitado +
            " CONFIRMADO."
        );


        CloseHandle(
            pi.hProcess
        );


        // =================================================
        // 7. CERRAR LA APLICACION ACTUAL
        // =================================================

        Debug.Log(
            "CAMBIO CORRECTO. CERRANDO APP ACTUAL."
        );


        MatarProcesoActual();
    }


    // =====================================================
    // ELEGIR RUTA
    // =====================================================
    //
    // IMPORTANTE:
    // usa destinoSolicitado.
    // NO usa la variable global "destino".
    // =====================================================

    string ObtenerRutaDestino(
        DestinoPCVR destinoSolicitado
    )
    {
        switch (destinoSolicitado)
        {
            case DestinoPCVR.Equipo1:

                return RUTA_EQUIPO1;


            case DestinoPCVR.Equipo2:

                return RUTA_EQUIPO2;


            case DestinoPCVR.Lobby:

                return RUTA_LOBBY;


            default:

                return "";
        }
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
                "MATANDO PROCESO ACTUAL PID: " +
                pid
            );


            bool terminado =
                TerminateProcess(
                    handle,
                    0
                );


            if (!terminado)
            {
                Debug.LogError(
                    "TerminateProcess FALLO"
                );


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
    // RECUPERAR XR SI EL DESTINO FALLA
    // =====================================================

    IEnumerator RecuperarXR()
    {
        Debug.Log(
            "RECUPERANDO XR..."
        );


        if (XRGeneralSettings.Instance ==
            null)
        {
            yield break;
        }


        XRManagerSettings manager =
            XRGeneralSettings
                .Instance
                .Manager;


        if (manager == null)
            yield break;


        yield return
            manager.InitializeLoader();


        if (manager.activeLoader ==
            null)
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