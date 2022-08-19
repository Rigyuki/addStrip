using HybridCLR.Generators;
using HybridCLR.Generators.MethodBridge;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace HybridCLR
{
     [InitializeOnLoad]
    public class BuildPipelineStripDllHook
    {
        private static MethodHook _hook_StripAssembliesTo;

        struct BuildPostProcessArgs
        {
            public BuildTarget target;
            public int subTarget;
            public string stagingArea;
            public string stagingAreaData;
            public string stagingAreaDataManaged;
            public string playerPackage;
            public string installPath;
            public string companyName;
            public string productName;
            public Guid productGUID;
            public BuildOptions options;
            public UnityEditor.Build.Reporting.BuildReport report;
            internal /*RuntimeClassRegistry*/object usedClassRegistry;
        }

        static BuildPipelineStripDllHook()
        {
            InstallHook();
        }

        public static void InstallHook()
        {          
            do
            {
                Type type = Type.GetType("UnityEditorInternal.AssemblyStripper,UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
                if (type == null)
                {
                    Debug.LogError($"can not find type: UnityEditorInternal.AssemblyStripper");
                    break;
                }

                MethodInfo miTarget = type.GetMethod("StripAssembliesTo", BindingFlags.Static | BindingFlags.NonPublic);
                if (miTarget == null)
                {
                    Debug.LogError($"can not find method: UnityEditorInternal.AssemblyStripper.StripAssembliesTo");
                    break;
                }

                MethodInfo miReplace = typeof(BuildPipelineStripDllHook).GetMethod(nameof(StripAssembliesTo_Replace), BindingFlags.Static | BindingFlags.NonPublic);
                MethodInfo miProxy = typeof(BuildPipelineStripDllHook).GetMethod(nameof(StripAssembliesTo_Proxy), BindingFlags.Static | BindingFlags.NonPublic);

                _hook_StripAssembliesTo = new MethodHook(miTarget, miReplace, miProxy);
                _hook_StripAssembliesTo.Install();

                Debug.Log("Hook BuildPipeline_StripDll_HookTest.StripAssembliesTo installed");
            } while (false);
        }

        public static void UninstallHook()
        {           
            _hook_StripAssembliesTo?.Uninstall();
        }

        static void ReportBuildResults_Replace(object obj, /*BeeDriverResult*/ object result)
        {
            
            // TODO: 可以在这里把 Library\Bee\artifacts\WinPlayerBuildProgram\ManagedStripped 目录下的文件复制出来
            Debug.LogError("ReportBuildResults_Replace called");
            ReportBuildResults_Proxy(obj, result);
        }

        static bool StripAssembliesTo_Replace(string outputFolder, out string output, out string error, IEnumerable<string> linkXmlFiles, /*UnityLinkerRunInformation*/ object runInformation)
        {
                            
            string srcPath = $"{AppDomain.CurrentDomain.BaseDirectory}/Temp/StagingArea/Data/Managed/tempStrip";
            string destPath = $"{AppDomain.CurrentDomain.BaseDirectory}/HybridCLRData/AssembliesPostIl2CppStrip/iOS";
            if (Directory.Exists(srcPath) && Directory.Exists(destPath))
            {
                CopyDirectory(srcPath, destPath);
            }

            // TODO: 可以在这里把 Temp\StagingArea\Data\Managed\tempStrip 目录下的文件复制出来
            Debug.Log("StripAssembliesTo_Replace called");

            bool ret = StripAssembliesTo_Proxy(outputFolder, out output, out error, linkXmlFiles, runInformation);
            return ret;
        }

        public static void CopyDirectory(string srcPath, string destPath)
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);
            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();
            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)
                {
                    CopyDirectory(i.FullName, destPath + "\\" + i.Name);
                }
                else
                {
                    File.Copy(i.FullName, destPath + "\\" + i.Name, true);
                }
            }

        }

        #region Proxy Methods

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static void ReportBuildResults_Proxy(object obj, /*BeeDriverResult*/ object result)
        {
            // dummy code
            Debug.Log("something" + obj.ToString() + result.ToString() + 2);
        }

        [MethodImpl(MethodImplOptions.NoOptimization)]
        static bool StripAssembliesTo_Proxy(string outputFolder, out string output, out string error, IEnumerable<string> linkXmlFiles, /*UnityLinkerRunInformation*/ object runInformation)
        {
            Debug.Log("StripAssembliesTo_Proxy called");
            output = null;
            error = null;
            return true;
        }
        #endregion
    }
}
 
