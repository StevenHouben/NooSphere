using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Security;
using System.Runtime.ConstrainedExecution;
using Microsoft.Win32.SafeHandles;
using System.Collections.Generic;
using System.ComponentModel;
using System.Management;
using System.Threading;


//inspired from http://www.eggheadcafe.com/community/csharp/2/10315145/enabledisable-comm-port-by-programming.aspx

namespace ABC.Infrastructure.Driver
{
    [Flags()]
    enum SetupDiGetClassDevsFlags
    {
        Default = 1,
        Present = 2,
        AllClasses = 4,
        Profile = 8,
        DeviceInterface = (int)0x10
    }

    enum DiFunction
    {
        SelectDevice = 1,
        InstallDevice = 2,
        AssignResources = 3,
        Properties = 4,
        Remove = 5,
        FirstTimeSetup = 6,
        FoundDevice = 7,
        SelectClassDrivers = 8,
        ValidateClassDrivers = 9,
        InstallClassDrivers = (int)0xa,
        CalcDiskSpace = (int)0xb,
        DestroyPrivateData = (int)0xc,
        ValidateDriver = (int)0xd,
        Detect = (int)0xf,
        InstallWizard = (int)0x10,
        DestroyWizardData = (int)0x11,
        PropertyChange = (int)0x12,
        EnableClass = (int)0x13,
        DetectVerify = (int)0x14,
        InstallDeviceFiles = (int)0x15,
        UnRemove = (int)0x16,
        SelectBestCompatDrv = (int)0x17,
        AllowInstall = (int)0x18,
        RegisterDevice = (int)0x19,
        NewDeviceWizardPreSelect = (int)0x1a,
        NewDeviceWizardSelect = (int)0x1b,
        NewDeviceWizardPreAnalyze = (int)0x1c,
        NewDeviceWizardPostAnalyze = (int)0x1d,
        NewDeviceWizardFinishInstall = (int)0x1e,
        Unused1 = (int)0x1f,
        InstallInterfaces = (int)0x20,
        DetectCancel = (int)0x21,
        RegisterCoInstallers = (int)0x22,
        AddPropertyPageAdvanced = (int)0x23,
        AddPropertyPageBasic = (int)0x24,
        Reserved1 = (int)0x25,
        Troubleshooter = (int)0x26,
        PowerMessageWake = (int)0x27,
        AddRemotePropertyPageAdvanced = (int)0x28,
        UpdateDriverUI = (int)0x29,
        Reserved2 = (int)0x30
    }

    enum StateChangeAction
    {
        Enable = 1,
        Disable = 2,
        PropChange = 3,
        Start = 4,
        Stop = 5
    }

    [Flags()]
    enum Scopes
    {
        Global = 1,
        ConfigSpecific = 2,
        ConfigGeneral = 4
    }

    enum SetupApiError
    {
        NoAssociatedClass = unchecked( (int)0xe0000200 ),
        ClassMismatch = unchecked( (int)0xe0000201 ),
        DuplicateFound = unchecked( (int)0xe0000202 ),
        NoDriverSelected = unchecked( (int)0xe0000203 ),
        KeyDoesNotExist = unchecked( (int)0xe0000204 ),
        InvalidDevinstName = unchecked( (int)0xe0000205 ),
        InvalidClass = unchecked( (int)0xe0000206 ),
        DevinstAlreadyExists = unchecked( (int)0xe0000207 ),
        DevinfoNotRegistered = unchecked( (int)0xe0000208 ),
        InvalidRegProperty = unchecked( (int)0xe0000209 ),
        NoInf = unchecked( (int)0xe000020a ),
        NoSuchHDevinst = unchecked( (int)0xe000020b ),
        CantLoadClassIcon = unchecked( (int)0xe000020c ),
        InvalidClassInstaller = unchecked( (int)0xe000020d ),
        DiDoDefault = unchecked( (int)0xe000020e ),
        DiNoFileCopy = unchecked( (int)0xe000020f ),
        InvalidHwProfile = unchecked( (int)0xe0000210 ),
        NoDeviceSelected = unchecked( (int)0xe0000211 ),
        DevinfolistLocked = unchecked( (int)0xe0000212 ),
        DevinfodataLocked = unchecked( (int)0xe0000213 ),
        DiBadPath = unchecked( (int)0xe0000214 ),
        NoClassInstallParams = unchecked( (int)0xe0000215 ),
        FileQueueLocked = unchecked( (int)0xe0000216 ),
        BadServiceInstallSect = unchecked( (int)0xe0000217 ),
        NoClassDriverList = unchecked( (int)0xe0000218 ),
        NoAssociatedService = unchecked( (int)0xe0000219 ),
        NoDefaultDeviceInterface = unchecked( (int)0xe000021a ),
        DeviceInterfaceActive = unchecked( (int)0xe000021b ),
        DeviceInterfaceRemoved = unchecked( (int)0xe000021c ),
        BadInterfaceInstallSect = unchecked( (int)0xe000021d ),
        NoSuchInterfaceClass = unchecked( (int)0xe000021e ),
        InvalidReferenceString = unchecked( (int)0xe000021f ),
        InvalidMachineName = unchecked( (int)0xe0000220 ),
        RemoteCommFailure = unchecked( (int)0xe0000221 ),
        MachineUnavailable = unchecked( (int)0xe0000222 ),
        NoConfigMgrServices = unchecked( (int)0xe0000223 ),
        InvalidPropPageProvider = unchecked( (int)0xe0000224 ),
        NoSuchDeviceInterface = unchecked( (int)0xe0000225 ),
        DiPostProcessingRequired = unchecked( (int)0xe0000226 ),
        InvalidCOInstaller = unchecked( (int)0xe0000227 ),
        NoCompatDrivers = unchecked( (int)0xe0000228 ),
        NoDeviceIcon = unchecked( (int)0xe0000229 ),
        InvalidInfLogConfig = unchecked( (int)0xe000022a ),
        DiDontInstall = unchecked( (int)0xe000022b ),
        InvalidFilterDriver = unchecked( (int)0xe000022c ),
        NonWindowsNTDriver = unchecked( (int)0xe000022d ),
        NonWindowsDriver = unchecked( (int)0xe000022e ),
        NoCatalogForOemInf = unchecked( (int)0xe000022f ),
        DevInstallQueueNonNative = unchecked( (int)0xe0000230 ),
        NotDisableable = unchecked( (int)0xe0000231 ),
        CantRemoveDevinst = unchecked( (int)0xe0000232 ),
        InvalidTarget = unchecked( (int)0xe0000233 ),
        DriverNonNative = unchecked( (int)0xe0000234 ),
        InWow64 = unchecked( (int)0xe0000235 ),
        SetSystemRestorePoint = unchecked( (int)0xe0000236 ),
        IncorrectlyCopiedInf = unchecked( (int)0xe0000237 ),
        SceDisabled = unchecked( (int)0xe0000238 ),
        UnknownException = unchecked( (int)0xe0000239 ),
        PnpRegistryError = unchecked( (int)0xe000023a ),
        RemoteRequestUnsupported = unchecked( (int)0xe000023b ),
        NotAnInstalledOemInf = unchecked( (int)0xe000023c ),
        InfInUseByDevices = unchecked( (int)0xe000023d ),
        DiFunctionObsolete = unchecked( (int)0xe000023e ),
        NoAuthenticodeCatalog = unchecked( (int)0xe000023f ),
        AuthenticodeDisallowed = unchecked( (int)0xe0000240 ),
        AuthenticodeTrustedPublisher = unchecked( (int)0xe0000241 ),
        AuthenticodeTrustNotEstablished = unchecked( (int)0xe0000242 ),
        AuthenticodePublisherNotTrusted = unchecked( (int)0xe0000243 ),
        SignatureOSAttributeMismatch = unchecked( (int)0xe0000244 ),
        OnlyValidateViaAuthenticode = unchecked( (int)0xe0000245 )
    }

    [StructLayout( LayoutKind.Sequential )]
    struct DeviceInfoData
    {
        public int Size;
        public Guid ClassGuid;
        public int DevInst;
        public IntPtr Reserved;
    }

    [StructLayout( LayoutKind.Sequential )]
    struct PropertyChangeParameters
    {
        public int Size;
        // part of header. It's flattened out into 1 structure.
        public DiFunction DiFunction;
        public StateChangeAction StateChange;
        public Scopes Scope;
        public int HwProfile;
    }

    class NativeMethods
    {
        const string setupapi = "setupapi.dll";

        NativeMethods() {}

        /// <summary>
        /// Retrieves the GUID(s) associated with the specified class name. 
        /// This list is built based on the classes currently installed on the system.
        /// </summary>
        /// <param name="ClassName">The name of the class for which to retrieve the class GUID.</param>
        /// <param name="ClassGuidArray1stItem">A pointer to an array to receive the list of 
        /// GUIDs associated with the specified class name.</param>
        /// <param name="ClassGuidArraySize">The number of GUIDs in the ClassGuidList array.</param>
        /// <param name="RequiredSize">Supplies a pointer to a variable that receives the 
        /// number of GUIDs associated with the class name. If this number is greater than the size of 
        /// the ClassGuidList buffer, the number indicates how large the array must be in order to store 
        /// all the GUIDs.</param>
        /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE 
        /// and the logged error can be retrieved by making a call to GetLastError.</returns>
        [DllImport( "setupapi.dll", SetLastError = true )]
        public static extern bool SetupDiClassGuidsFromName(
            string ClassName,
            ref Guid ClassGuidArray1stItem,
            UInt32 ClassGuidArraySize,
            out UInt32 RequiredSize );

        /// <summary>
        /// The SetupDiCallClassInstaller function calls the appropriate class installer, and any registered 
        /// co-installers, with the specified installation request (DIF code).
        /// </summary>
        /// <param name="installFunction">The device installation request (DIF request) to pass to the 
        /// co-installers and class installer. DIF codes have the format DIF_XXX and are defined in Setupapi.h.</param>
        /// <param name="deviceInfoSet">A handle to a device information set for the local computer. 
        /// This set contains a device installation element which represents the device for which to perform 
        /// the specified installation function.</param>
        /// <param name="deviceInfoData">A pointer to an SP_DEVINFO_DATA structure that specifies the device 
        /// information element in the DeviceInfoSet that represents the device for which to perform the 
        /// specified installation function. This parameter is optional and can be set to NULL.</param>
        /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the 
        /// logged error can be retrieved by making a call to GetLastError.</returns>
        [DllImport( setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetupDiCallClassInstaller(
            DiFunction installFunction,
            SafeDeviceInfoSetHandle deviceInfoSet,
            [In()] ref DeviceInfoData deviceInfoData );

        /// <summary>
        /// The SetupDiEnumDeviceInfo function returns a SP_DEVINFO_DATA structure that specifies a 
        /// device information element in a device information set. 
        /// </summary>
        /// <param name="deviceInfoSet">A handle to the device information set for which to return an 
        /// SP_DEVINFO_DATA structure that represents a device information element.</param>
        /// <param name="memberIndex">A zero-based index of the device information element to retrieve.</param>
        /// <param name="deviceInfoData">A pointer to an SP_DEVINFO_DATA structure to receive information 
        /// about an enumerated device information element. The caller must set DeviceInfoData.cbSize 
        /// to sizeof(SP_DEVINFO_DATA).</param>
        /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the 
        /// logged error can be retrieved with a call to GetLastError.</returns>
        [DllImport( setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetupDiEnumDeviceInfo(
            SafeDeviceInfoSetHandle deviceInfoSet,
            int memberIndex,
            ref DeviceInfoData deviceInfoData );

        /// <summary>
        /// The SetupDiGetClassDevs function returns a handle to a device information set that 
        /// contains requested device information elements for a local computer.
        /// </summary>
        /// <param name="classGuid">A pointer to the GUID for a device setup class or a device 
        /// interface class. This pointer is optional and can be NULL.</param>
        /// <param name="enumerator">A pointer to a NULL-terminated string that specifies:
        ///<list type="bullet">
        ///<item>
        ///<description>An identifier (ID) of a Plug and Play (PnP) enumerator. 
        ///This ID can either be the value's globally unique identifier (GUID) or symbolic name. 
        ///For example, "PCI" can be used to specify the PCI PnP value. 
        ///Other examples of symbolic names for PnP values include "USB," "PCMCIA," and "SCSI".</description>
        ///</item>
        ///<item>
        ///<description>A PnP device instance ID. When specifying a PnP device instance ID, 
        ///DIGCF_DEVICEINTERFACE must be set in the Flags parameter.</description>
        ///</item>
        ///</list>
        ///This pointer is optional and can be NULL. If an enumeration value is not used to select devices, set Enumerator to NULL</param>
        /// <param name="hwndParent">A handle to the top-level window to be used for a user interface that 
        /// is associated with installing a device instance in the device information set. 
        /// This handle is optional and can be NULL.</param>
        /// <param name="flags">A variable of type DWORD that specifies control options that filter the device information elements 
        /// that are added to the device information set.</param>
        /// <returns></returns>
        [DllImport( setupapi, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, SetLastError = true )]
        public static extern SafeDeviceInfoSetHandle SetupDiGetClassDevs(
            [In()] ref Guid classGuid,
            [MarshalAs( UnmanagedType.LPWStr )] string enumerator,
            IntPtr hwndParent,
            SetupDiGetClassDevsFlags flags );

        /// <summary>
        /// The SetupDiGetDeviceInstanceId function retrieves the device instance ID that is 
        /// associated with a device information element.
        /// </summary>
        /// <param name="DeviceInfoSet">A handle to the device information set that contains 
        /// the device information element that represents the device for which to retrieve a device instance ID.</param>
        /// <param name="did">A pointer to an SP_DEVINFO_DATA structure that specifies the device 
        /// information element in DeviceInfoSet.</param>
        /// <param name="DeviceInstanceId">A pointer to the character buffer that will receive the NULL-terminated device 
        /// instance ID for the specified device information element.</param>
        /// <param name="DeviceInstanceIdSize">The size, in characters, of the DeviceInstanceId buffer.</param>
        /// <param name="RequiredSize">A pointer to the variable that receives the number of characters required 
        /// to store the device instance ID.</param>
        /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the 
        /// logged error can be retrieved by making a call to GetLastError.</returns>
        [DllImport( "setupapi.dll", SetLastError = true, CharSet = CharSet.Auto )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetupDiGetDeviceInstanceId(
            IntPtr DeviceInfoSet,
            ref DeviceInfoData did,
            [MarshalAs( UnmanagedType.LPTStr )] StringBuilder DeviceInstanceId,
            int DeviceInstanceIdSize,
            out int RequiredSize );

        /// <summary>
        /// The SetupDiDestroyDeviceInfoList function deletes a device information set and frees all associated memory.
        /// </summary>
        /// <param name="deviceInfoSet">A handle to the device information set to delete.</param>
        /// <returns>The function returns TRUE if it is successful. Otherwise, it returns FALSE and the 
        /// logged error can be retrieved with a call to GetLastError.</returns>
        [SuppressUnmanagedCodeSecurity()]
        [ReliabilityContract( Consistency.WillNotCorruptState, Cer.Success )]
        [DllImport( setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetupDiDestroyDeviceInfoList( IntPtr deviceInfoSet );

        /// <summary>
        /// The SetupDiSetClassInstallParams function sets or clears class install parameters for a 
        /// device information set or a particular device information element.
        /// </summary>
        /// <param name="deviceInfoSet">A handle to the device information set for which to 
        /// set class install parameters.</param>
        /// <param name="deviceInfoData">A pointer to an SP_DEVINFO_DATA structure that represents the 
        /// device for which to set class install parameters. This parameter is optional and can be NULL.</param>
        /// <param name="classInstallParams">A pointer to a buffer that contains the new class install 
        /// parameters to use. The SP_CLASSINSTALL_HEADER structure at the beginning of this buffer 
        /// must have its cbSize field set to sizeof(SP_CLASSINSTALL_HEADER) and the InstallFunction 
        /// field must be set to the DI_FUNCTION code that reflects the type of parameters contained in 
        /// the rest of the buffer.</param>
        /// <param name="classInstallParamsSize">The size, in bytes, of the ClassInstallParams buffer. 
        /// If the buffer is not supplied (that is, the class install parameters are being cleared), 
        /// ClassInstallParamsSize must be 0.</param>
        /// <returns></returns>
        [DllImport( setupapi, CallingConvention = CallingConvention.Winapi, SetLastError = true )]
        [return: MarshalAs( UnmanagedType.Bool )]
        public static extern bool SetupDiSetClassInstallParams(
            SafeDeviceInfoSetHandle deviceInfoSet,
            [In()] ref DeviceInfoData deviceInfoData,
            [In()] ref PropertyChangeParameters classInstallParams,
            int classInstallParamsSize );
    }

    class SafeDeviceInfoSetHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        public SafeDeviceInfoSetHandle()
            : base( true ) {}

        protected override bool ReleaseHandle()
        {
            return NativeMethods.SetupDiDestroyDeviceInfoList( this.handle );
        }
    }

    /// <summary>
    /// Helper class for resetting ports and enabling/disabling devices 
    /// of the local system.
    /// </summary>
    public class PortHelper
    {
        PortHelper() {}

        /// <summary>
        /// Tries to reset (disable/enable) the port with the given instance Id.
        /// </summary>
        /// <param name="instanceId">The instance Id of the port to reset.</param>
        /// <returns>True, if the port was successfully resetted, otherwise false.</returns>
        public static bool TryResetPortByInstanceId( string instanceId )
        {
            SafeDeviceInfoSetHandle diSetHandle = null;

            if ( !String.IsNullOrEmpty( instanceId ) )
            {
                try
                {
                    Guid[] guidArray = GetGuidFromName( "Ports" );

                    //Get the handle to a device information set for all devices matching classGuid that are present on the 
                    //system.
                    diSetHandle = NativeMethods.SetupDiGetClassDevs(
                        ref guidArray[ 0 ], null, IntPtr.Zero, SetupDiGetClassDevsFlags.DeviceInterface );

                    //Get the device information data for each matching device.
                    DeviceInfoData[] diData = GetDeviceInfoData( diSetHandle );

                    //Try to find the object with the same instance Id.
                    foreach ( var infoData in diData )
                    {
                        var instanceIds = GetInstanceIdsFromClassGuid( infoData.ClassGuid );
                        foreach ( var id in instanceIds )
                        {
                            if ( id.Equals( instanceId ) )
                            {
                                //disable port
                                SetDeviceEnabled( infoData.ClassGuid, id, false );
                                //wait a moment
                                Thread.Sleep( 1000 );
                                //enable port
                                SetDeviceEnabled( infoData.ClassGuid, id, true );
                                return true;
                            }
                        }
                    }
                }
                catch ( Exception )
                {
                    return false;
                }
                finally
                {
                    if ( diSetHandle != null )
                    {
                        if ( diSetHandle.IsClosed == false )
                        {
                            diSetHandle.Close();
                        }
                        diSetHandle.Dispose();
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Tries to reset (disable/enable) the port with the given port name (e.g. COM1).
        /// </summary>
        /// <param name="portName">The port name of the port to reset.</param>
        /// <returns>True, if the port was successfully resetted, otherwise false.</returns>
        public static bool TryResetPortByName( string portName )
        {
            try
            {
                var instanceId = String.Empty;

                //get instance id for port from WMI
                ManagementObjectSearcher searcher = new ManagementObjectSearcher( "select * from Win32_SerialPort" );
                foreach ( ManagementObject port in searcher.Get() )
                {
                    if ( port[ "DeviceID" ].ToString().Equals( portName ) )
                    {
                        instanceId = port[ "PNPDeviceID" ].ToString();
                        break;
                    }
                }
                if ( !String.IsNullOrEmpty( instanceId ) )
                {
                    return TryResetPortByInstanceId( instanceId );
                }
                return false;
            }
            catch ( Exception )
            {
                return false;
            }
        }

        static Guid[] GetGuidFromName( string devName )
        {
            UInt32 RequiredSize = 0;
            Guid[] GuidArray = new Guid[1];
            // read Guids
            bool Status = NativeMethods.SetupDiClassGuidsFromName( devName, ref GuidArray[ 0 ], 1, out RequiredSize );
            if ( Status )
            {
                if ( RequiredSize > 1 )
                {
                    GuidArray = new Guid[RequiredSize];
                    NativeMethods.SetupDiClassGuidsFromName( devName, ref GuidArray[ 0 ], RequiredSize, out RequiredSize );
                }
            }
            else
            {
                throw new Win32Exception();
            }
            return GuidArray;
        }

        static List<string> GetInstanceIdsFromClassGuid( Guid classGuid )
        {
            SafeDeviceInfoSetHandle diSetHandle = null;
            List<string> resultList = new List<string>();
            try
            {
                // Get the handle to a device information set for all devices matching classGuid that are present on the 
                // system.
                diSetHandle = NativeMethods.SetupDiGetClassDevs( ref classGuid, null, IntPtr.Zero, SetupDiGetClassDevsFlags.Present );
                // Get the device information data for each matching device.
                DeviceInfoData[] diData = GetDeviceInfoData( diSetHandle );

                const int ERROR_INSUFFICIENT_BUFFER = 122;
                for ( int index = 0; index <= diData.Length - 1; index++ )
                {
                    StringBuilder sb = new StringBuilder( 1 );
                    int requiredSize = 0;
                    bool result = NativeMethods.SetupDiGetDeviceInstanceId( diSetHandle.DangerousGetHandle(), ref diData[ index ], sb, sb.Capacity, out requiredSize );
                    if ( result == false && Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER )
                    {
                        sb.Capacity = requiredSize;
                        result = NativeMethods.SetupDiGetDeviceInstanceId( diSetHandle.DangerousGetHandle(), ref diData[ index ], sb, sb.Capacity, out requiredSize );

                        resultList.Add( sb.ToString() );
                    }
                }
            }
            finally
            {
                if ( diSetHandle != null )
                {
                    if ( diSetHandle.IsClosed == false )
                    {
                        diSetHandle.Close();
                    }
                    diSetHandle.Dispose();
                }
            }
            return resultList;
        }

        /// <summary>
        /// Enable or disable a device.
        /// </summary>
        /// <param name="classGuid">The class guid of the device. Available in the device manager.</param>
        /// <param name="instanceId">The device instance id of the device. Available in the device manager.</param>
        /// <param name="enable">True to enable, False to disable.</param>
        /// <remarks>Will throw an exception if the device is not Disableable.</remarks>
        public static void SetDeviceEnabled( Guid classGuid, string instanceId, bool enable )
        {
            SafeDeviceInfoSetHandle diSetHandle = null;
            try
            {
                // Get the handle to a device information set for all devices matching classGuid that are present on the 
                // system.
                diSetHandle = NativeMethods.SetupDiGetClassDevs( ref classGuid, null, IntPtr.Zero, SetupDiGetClassDevsFlags.Present );
                // Get the device information data for each matching device.
                DeviceInfoData[] diData = GetDeviceInfoData( diSetHandle );
                // Find the index of our instance.
                int index = GetIndexOfInstance( diSetHandle, diData, instanceId );
                // Enable/Disable.
                EnableDevice( diSetHandle, diData[ index ], enable );
            }
            finally
            {
                if ( diSetHandle != null )
                {
                    if ( diSetHandle.IsClosed == false )
                    {
                        diSetHandle.Close();
                    }
                    diSetHandle.Dispose();
                }
            }
        }

        static DeviceInfoData[] GetDeviceInfoData( SafeDeviceInfoSetHandle handle )
        {
            List<DeviceInfoData> data = new List<DeviceInfoData>();
            DeviceInfoData did = new DeviceInfoData();
            int didSize = Marshal.SizeOf( did );
            did.Size = didSize;
            int index = 0;
            while ( NativeMethods.SetupDiEnumDeviceInfo( handle, index, ref did ) )
            {
                data.Add( did );
                index += 1;
                did = new DeviceInfoData();
                did.Size = didSize;
            }
            return data.ToArray();
        }

        /// <summary>
        /// Find the index of the particular DeviceInfoData for the instanceId.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="diData"></param>
        /// <param name="instanceId"></param>
        /// <returns></returns> 
        static int GetIndexOfInstance( SafeDeviceInfoSetHandle handle, DeviceInfoData[] diData, string instanceId )
        {
            const int ERROR_INSUFFICIENT_BUFFER = 122;
            for ( int index = 0; index <= diData.Length - 1; index++ )
            {
                StringBuilder sb = new StringBuilder( 1 );
                int requiredSize = 0;
                bool result = NativeMethods.SetupDiGetDeviceInstanceId( handle.DangerousGetHandle(), ref diData[ index ], sb, sb.Capacity, out requiredSize );
                if ( result == false && Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER )
                {
                    sb.Capacity = requiredSize;
                    result = NativeMethods.SetupDiGetDeviceInstanceId( handle.DangerousGetHandle(), ref diData[ index ], sb, sb.Capacity, out requiredSize );
                }
                if ( result == false )
                    throw new Win32Exception();
                if ( instanceId.Equals( sb.ToString() ) )
                {
                    return index;
                }
            }
            // not found
            return -1;
        }

        // enable/disable...
        static void EnableDevice( SafeDeviceInfoSetHandle handle, DeviceInfoData diData, bool enable )
        {
            PropertyChangeParameters @params = new PropertyChangeParameters();
            // The size is just the size of the header, but we've flattened the structure.
            // The header comprises the first two fields, both integer.
            @params.Size = 8;
            @params.DiFunction = DiFunction.PropertyChange;
            @params.Scope = Scopes.Global;
            if ( enable )
            {
                @params.StateChange = StateChangeAction.Enable;
            }
            else
            {
                @params.StateChange = StateChangeAction.Disable;
            }

            bool result = NativeMethods.SetupDiSetClassInstallParams( handle, ref diData, ref @params, Marshal.SizeOf( @params ) );
            if ( result == false )
            {
                throw new Win32Exception( Marshal.GetLastWin32Error() );
            }
            result = NativeMethods.SetupDiCallClassInstaller( DiFunction.PropertyChange, handle, ref diData );
            if ( result == false )
            {
                int err = Marshal.GetLastWin32Error();
                if ( err == (int)SetupApiError.NotDisableable )
                {
                    throw new ArgumentException( "Device can't be disabled (programmatically or in Device Manager)." );
                }
                else if ( err <= (int)SetupApiError.NoAssociatedClass && err >= (int)SetupApiError.OnlyValidateViaAuthenticode )
                {
                    throw new Win32Exception( "SetupAPI error: " + ( (SetupApiError)err ).ToString() );
                }
                else
                {
                    throw new Win32Exception( err );
                }
            }
        }
    }
}