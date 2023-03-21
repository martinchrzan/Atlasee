using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using VisFileManager.FileSystemHelpers;
using VisFileManager.Shared;
using VisFileManager.Validators;

namespace VisFileManager.Helpers
{
    [Export(typeof(IInvokeHelper))]
    public class InvokeHelper : IInvokeHelper
    {
        private readonly IGlobalFileManager _globalFileManager;

        [ImportingConstructor]
        public InvokeHelper(IGlobalFileManager globalFileManager)
        {
            _globalFileManager = globalFileManager;
        }

        [Flags]
        public enum ASSOC_FILTER
        {
            ASSOC_FILTER_NONE = 0x00000000,
            ASSOC_FILTER_RECOMMENDED = 0x00000001
        }

        [DllImport("Shell32", EntryPoint = "SHAssocEnumHandlers", PreserveSig = false)]
        internal extern static void SHAssocEnumHandlers([MarshalAs(UnmanagedType.LPWStr)] string pszExtra, ASSOC_FILTER afFilter, [Out] out IntPtr ppEnumHandler);

        // IEnumAssocHandlers
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        private delegate int FuncNext(IntPtr refer, int celt, [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.Interface, SizeParamIndex = 1)] IntPtr[] rgelt, [Out] out int pceltFetched);

        // IAssocHandler
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        private delegate int FuncGetName(IntPtr refer, out IntPtr ppsz);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        private delegate int FuncGetUiName(IntPtr refer, out IntPtr ppsz);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        private delegate int FuncGetIconLocation(IntPtr refer, out IntPtr ppszPath, out IntPtr index);

        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Unicode)]
        private delegate int FuncInvoke(IntPtr refer, IDataObject pdo);


        [Guid("43826d1e-e718-42ee-bc55-a1e261c37bfe"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IShellItem
        {
            // here we only need this member
            IDataObject BindToHandler(IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid bhid, [MarshalAs(UnmanagedType.LPStruct)] Guid riid);
        }

        [DllImport("shell32", CharSet = CharSet.Unicode)]
        private extern static int SHCreateItemFromParsingName(string pszPath, IBindCtx pbc, [MarshalAs(UnmanagedType.LPStruct)] Guid riid, out IShellItem ppv);

        public class OpenWithInfo
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string IconPath { get; set; }
            public int IconIndex { get; set; }
        }

        private static void EnumerateItems(Action<IntPtr> innerAction, string extensionWithDot)
        {
            IntPtr pEnumAssocHandlers;
            SHAssocEnumHandlers(extensionWithDot, ASSOC_FILTER.ASSOC_FILTER_RECOMMENDED, out pEnumAssocHandlers);

            IntPtr pFuncNext = Marshal.ReadIntPtr(Marshal.ReadIntPtr(pEnumAssocHandlers) + 3 * Marshal.SizeOf(typeof(IntPtr)));
            FuncNext next = (FuncNext)Marshal.GetDelegateForFunctionPointer(pFuncNext, typeof(FuncNext));

            IntPtr[] pArrayAssocHandlers = new IntPtr[255];
            int num;

            int resNext = next(pEnumAssocHandlers, 255, pArrayAssocHandlers, out num);
            if (resNext == 0)
            {
                for (int i = 0; i < num; i++)
                {
                    IntPtr pAssocHandler = pArrayAssocHandlers[i];
                    innerAction.Invoke(pAssocHandler);

                    Marshal.Release(pArrayAssocHandlers[i]);
                }
            }
            Marshal.Release(pEnumAssocHandlers);
        }

        public static List<OpenWithInfo> GetOpenWithInfo(string extensionWithDot)
        {
            List<OpenWithInfo> openAsInfo = new List<OpenWithInfo>();
            EnumerateItems((pAssocHandler) =>
            {
                // 3. (or any number used in marshaling) indicates that that function is on the 3rd position in a given iterface, please refer to this question 
                // https://stackoverflow.com/a/48449462/613299
                IntPtr pFuncGetName = Marshal.ReadIntPtr(Marshal.ReadIntPtr(pAssocHandler) + 3 * Marshal.SizeOf(typeof(IntPtr)));
                FuncGetName getName = (FuncGetName)Marshal.GetDelegateForFunctionPointer(pFuncGetName, typeof(FuncGetName));
                int resGetName = getName(pAssocHandler, out IntPtr pName);

                IntPtr pFuncGetUiName = Marshal.ReadIntPtr(Marshal.ReadIntPtr(pAssocHandler) + 4 * Marshal.SizeOf(typeof(IntPtr)));
                FuncGetUiName getUiName = (FuncGetUiName)Marshal.GetDelegateForFunctionPointer(pFuncGetUiName, typeof(FuncGetUiName));
                int resGetUiName = getUiName(pAssocHandler, out IntPtr pUiName);

                IntPtr pFuncGetIconLocation = Marshal.ReadIntPtr(Marshal.ReadIntPtr(pAssocHandler) + 5 * Marshal.SizeOf(typeof(IntPtr)));
                FuncGetIconLocation geIconLocation = (FuncGetIconLocation)Marshal.GetDelegateForFunctionPointer(pFuncGetIconLocation, typeof(FuncGetIconLocation));
                int resGetIconLocation = geIconLocation(pAssocHandler, out IntPtr pIconLocation, out IntPtr pIndex);

                if (IntPtr.Zero != pUiName && IntPtr.Zero != pName && IntPtr.Zero != pIconLocation)
                {
                    openAsInfo.Add(
                    new OpenWithInfo()
                    {
                        Name = Marshal.PtrToStringUni(pUiName),
                        Path = Marshal.PtrToStringUni(pName),
                        IconPath = Marshal.PtrToStringUni(pIconLocation),
                        IconIndex = pIndex.ToInt32(),
                    });
                }
            },
            extensionWithDot);

            return openAsInfo;
        }

        public static void InvokeOpenAsInfo(OpenWithInfo openAsInfo, string pathToExecute)
        {
            EnumerateItems((pAssocHandler) =>
            {
                IntPtr pFuncGetName = Marshal.ReadIntPtr(Marshal.ReadIntPtr(pAssocHandler) + 3 * Marshal.SizeOf(typeof(IntPtr)));
                FuncGetName getName = (FuncGetName)Marshal.GetDelegateForFunctionPointer(pFuncGetName, typeof(FuncGetName));
                IntPtr pName;
                int resGetName = getName(pAssocHandler, out pName);

                if (Marshal.PtrToStringUni(pName) == openAsInfo.Path)
                {
                    int hr = SHCreateItemFromParsingName(pathToExecute, null, typeof(IShellItem).GUID, out IShellItem item);
                    if (hr != 0)
                        throw new Win32Exception(hr);

                    var BHID_DataObject = new Guid("b8c0bd9f-ed24-455c-83e6-d5390c4fe8c4");
                    var dao = item.BindToHandler(null, BHID_DataObject, typeof(IDataObject).GUID);


                    IntPtr pFuncInvoke = Marshal.ReadIntPtr(Marshal.ReadIntPtr(pAssocHandler) + 8 * Marshal.SizeOf(typeof(IntPtr)));
                    FuncInvoke geInvoke = (FuncInvoke)Marshal.GetDelegateForFunctionPointer(pFuncInvoke, typeof(FuncInvoke));

                    geInvoke(pAssocHandler, dao);
                }
            }, Path.GetExtension(pathToExecute));
        }

        public void Invoke(FormattedPath path)
        {
            if (path.PathType == PathValidator.PathType.Directory || path.PathType == PathValidator.PathType.Drive)
            {
                _globalFileManager.SetCurrentPath(path);
            }
            else
            {
                try
                {
                    Process.Start(path.Path);
                }
                catch (Exception ex)
                {
                    Logger.LogError("Failed to start path " + path?.Path + Environment.NewLine + ex.Message);
                }
            }
        }

        public static bool StartProcess(string fileName, bool asAdmin, string param)
        {
            try
            {
                Process proc = new Process();
                proc.StartInfo.FileName = fileName;
                proc.StartInfo.UseShellExecute = true;
                if (asAdmin)
                {
                    proc.StartInfo.Verb = "runas";
                }
                proc.StartInfo.Arguments = param;
                return proc.Start();
            }
            catch(Exception ex)
            {
                Logger.LogError("Failed to start process, filename: " + fileName + " As admin: " + asAdmin + " Param: " + param + Environment.NewLine + ex.Message);
            }

            return false;
        }
    }
}
