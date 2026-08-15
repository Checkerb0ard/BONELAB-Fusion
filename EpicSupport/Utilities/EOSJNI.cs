using LabFusion.Data;
using JNISharp.NativeInterface;
using System.Reflection;
using System.Runtime.InteropServices;

namespace MarrowFusion.Epic.Utilities;

internal static class EOSJNI
{
    private static readonly BindingFlags _allBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.SetField | BindingFlags.GetProperty | BindingFlags.SetProperty;
    
    private static IntPtr _javaVM = IntPtr.Zero;

    private static JClass _eosSdkClass;
    private static JClass _unityPlayerClass;

    private const string EOS_LIBRARY_PATH = "/data/data/com.StressLevelZero.BONELAB/libEOSSDK.so";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate int JNI_OnLoadDelegate(IntPtr javaVM, IntPtr reserved);

    internal static void Initialize()
    {
        if (IsInitialized())
            return;
        
        if (!ExtractDexFiles())
            return;
        
        if (!InjectDex(PersistentData.GetPath("classes.dex")))
            return;

        if (!InjectDex(PersistentData.GetPath("classes2.dex")))
            return;
        
        if (!InjectDex(PersistentData.GetPath("classes3.dex")))
            return;
        
        if (!InjectDex(PersistentData.GetPath("classes4.dex")))
            return;
        
        if (!InjectDex(PersistentData.GetPath("classes5.dex")))
            return;
        
        if (!ValidatePatch())
            return;

        if (!LoadJNI())
            return;

        if (!InitializeEOS())
            return;
    }

    private static bool IsInitialized()
    {
        return _javaVM != IntPtr.Zero;
    }
    
    private static bool ValidatePatch()
    {
        _eosSdkClass = JNI.FindClass("com/epicgames/mobile/eossdk/EOSSDK");

        if (_eosSdkClass.Valid())
            return true;

        EpicModule.Logger.Error("EOSSDK class not found!");
        EpicModule.Logger.Error("Did dex injection succeed?");
        return false;
    }
    
    private static bool LoadJNI()
    {
        if (!ResolveJavaVM())
            return false;

        if (!InvokeNativeJNIOnLoad())
            return false;
        
        if (!LoadEOSNativeLibrary())
            return false;

        return true;
    }

    private static bool ResolveJavaVM()
    {
        var jniType = Type.GetType("JNISharp.NativeInterface.JNI, JNISharp");
        if (jniType == null)
        {
            EpicModule.Logger.Error("JNISharp.NativeInterface.JNI type not found!");
            return false;
        }

        var lastVmPtrField = jniType.GetField("lastVmPtr", _allBindingFlags);
        if (lastVmPtrField == null)
        {
            EpicModule.Logger.Error("JNISharp lastVmPtr field not found!");
            return false;
        }

        _javaVM = (IntPtr)lastVmPtrField.GetValue(null);
        if (_javaVM == IntPtr.Zero)
        {
            EpicModule.Logger.Error("Failed to retrieve JavaVM pointer from JNISharp!");
            return false;
        }

        return true;
    }

    private static bool InvokeNativeJNIOnLoad()
    {
        IntPtr onLoadPtr = MelonLoader.NativeLibrary.GetExport(EOSSDKLoader.LibraryPtr, "JNI_OnLoad");

        if (onLoadPtr == IntPtr.Zero)
        {
            EpicModule.Logger.Error("JNI_OnLoad export not found in EOS SDK!");
            return false;
        }

        var onLoad = Marshal.GetDelegateForFunctionPointer<JNI_OnLoadDelegate>(onLoadPtr);
        int result = onLoad(_javaVM, IntPtr.Zero);

#if DEBUG
        // JNI 1.6 = 0x00010006
        EpicModule.Logger.Log($"JNI_OnLoad returned: 0x{result:X}");
#endif

        return true;
    }

    private static bool LoadEOSNativeLibrary()
    {
        JClass systemClass = JNI.FindClass("java/lang/System");
        JMethodID loadMethod = JNI.GetStaticMethodID(systemClass, "load", "(Ljava/lang/String;)V");

        JObject libPath = JNI.NewString(EOS_LIBRARY_PATH);

        JNI.CallStaticVoidMethod(systemClass, loadMethod, libPath);

        if (!JNI.ExceptionCheck())
            return true;

        JNI.ExceptionDescribe();
        EpicModule.Logger.Error("Failed to load libEOSSDK.so via System.load!");
        return false;
    }
    
    private static bool InitializeEOS()
    {
        _unityPlayerClass = JNI.FindClass("com/unity3d/player/UnityPlayer");

        JFieldID activityField = JNI.GetStaticFieldID(_unityPlayerClass, "currentActivity", "Landroid/app/Activity;");
        JObject activity = JNI.GetStaticObjectField<JObject>(_unityPlayerClass, activityField);
        if (!activity.Valid())
        {
            EpicModule.Logger.Error("Failed to retrieve UnityPlayer.currentActivity!");
            return false;
        }

        JMethodID initMethod = _eosSdkClass.GetStaticMethodID("init", "(Landroid/app/Activity;)V");
        JNI.CallStaticVoidMethod(_eosSdkClass, initMethod, activity);

        if (!JNI.ExceptionCheck())
            return true;

        JNI.ExceptionDescribe();
        EpicModule.Logger.Error("EOS SDK initialization failed!");
        return false;
    }

    private static bool ExtractDexFiles()
    {
        const string classesResourcePath = "EpicSupport.dependencies.resources.dex.classes.dex";
        const string classes2ResourcePath = "EpicSupport.dependencies.resources.dex.classes2.dex";
        const string classes3ResourcePath = "EpicSupport.dependencies.resources.dex.classes3.dex";
        const string classes4ResourcePath = "EpicSupport.dependencies.resources.dex.classes4.dex";
        const string classes5ResourcePath = "EpicSupport.dependencies.resources.dex.classes5.dex";
        
        string classesPath = PersistentData.GetPath("classes.dex");
        string classes2Path = PersistentData.GetPath("classes2.dex");
        string classes3Path = PersistentData.GetPath("classes3.dex");
        string classes4Path = PersistentData.GetPath("classes4.dex");
        string classes5Path = PersistentData.GetPath("classes5.dex");
        
        File.WriteAllBytes(classesPath, EmbeddedResource.LoadBytesFromAssembly(EpicModule.ModuleAssembly, classesResourcePath));
        File.WriteAllBytes(classes2Path, EmbeddedResource.LoadBytesFromAssembly(EpicModule.ModuleAssembly, classes2ResourcePath));
        File.WriteAllBytes(classes3Path, EmbeddedResource.LoadBytesFromAssembly(EpicModule.ModuleAssembly, classes3ResourcePath));
        File.WriteAllBytes(classes4Path, EmbeddedResource.LoadBytesFromAssembly(EpicModule.ModuleAssembly, classes4ResourcePath));
        File.WriteAllBytes(classes5Path, EmbeddedResource.LoadBytesFromAssembly(EpicModule.ModuleAssembly, classes5ResourcePath));
        
        if (!File.Exists(classesPath) || !File.Exists(classes2Path) || !File.Exists(classes3Path) || !File.Exists(classes4Path) || !File.Exists(classes5Path))
        {
            EpicModule.Logger.Error("Failed to extract dex files!");
            return false;
        }

#if DEBUG
        EpicModule.Logger.Log("Extracted dex files!");
#endif
        
        return true;
    }
    
    private static bool InjectDex(string dexPath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dexPath))
            {
                EpicModule.Logger.Log("DEX path is null or empty");
                return false;
            }

            if (!File.Exists(dexPath))
            {
                EpicModule.Logger.Log($"DEX file not found: {dexPath}");
                return false;
            }

            var extension = Path.GetExtension(dexPath).ToLowerInvariant();
            if (extension != ".dex")
            {
                EpicModule.Logger.Log($"Invalid file extension: {extension}. Expected .dex");
                return false;
            }
            
            var threadClass = JNI.FindClass("java/lang/Thread");
            var currentThreadMethod = threadClass.GetStaticMethodID("currentThread", "()Ljava/lang/Thread;");
            var currentThread = JNI.CallStaticObjectMethod<JObject>(threadClass, currentThreadMethod);
            
            var getContextClassLoaderMethod = threadClass.GetMethodID("getContextClassLoader", "()Ljava/lang/ClassLoader;");
            var classLoader = JNI.CallObjectMethod<JObject>(currentThread, getContextClassLoaderMethod);      
            
            var fileClass = JNI.FindClass("java/io/File");
            var fileConstructor = fileClass.GetMethodID("<init>", "(Ljava/lang/String;)V");
            var dexPathStr = JNI.NewString(dexPath);
            
            var dexFile = JNI.NewObject<JObject>(fileClass, fileConstructor, new JValue(dexPathStr));
            if (!dexFile.Valid())
            {
                EpicModule.Logger.Log("Failed to create File object for DEX path");
                return false;
            }
            
            var baseDexClassLoaderClass = JNI.FindClass("dalvik/system/BaseDexClassLoader");
            var pathListField = baseDexClassLoaderClass.GetFieldID("pathList", "Ldalvik/system/DexPathList;");
            var pathList = JNI.GetObjectField<JObject>(classLoader, pathListField);
            var dexPathListClass = JNI.FindClass("dalvik/system/DexPathList");
            var addDexPathMethod = dexPathListClass.GetMethodID("addDexPath", "(Ljava/lang/String;Ljava/io/File;)V");
            
            JNI.CallVoidMethod(pathList, addDexPathMethod, new JValue(dexPathStr), new JValue(dexFile));
            
            JNI.CheckExceptionAndThrow();
#if DEBUG
            FusionLogger.Log($"Successfully loaded DEX file: {dexPath}");
#endif
            return true;
        }
        catch (JThrowableException jex)
        {
            EpicModule.Logger.Error($"Java exception while loading DEX file: {jex.Throwable}");
            try
            {
                string message = jex.Throwable.GetMessage();
                EpicModule.Logger.Error($"Exception message: {message}");
            }
            catch
            {
                EpicModule.Logger.Error("Could not retrieve exception message");
            }
            return false;
        }
        catch (JNIResultException jniEx)
        {
            EpicModule.Logger.Error($"JNI error while loading DEX file: {jniEx.Result}");
            return false;
        }
        catch (Exception ex)
        {
            EpicModule.Logger.LogException("loading DEX file", ex);
            return false;
        }
    }
}