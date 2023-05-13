using GameData.Domains.Character;
using GameData.Domains.Organization;
using GameData.Domains;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameData.GameDataBridge;
using GameData.GameDataBridge.VnPipe;
using System.Reflection;
using System.Runtime.InteropServices;

namespace HappyLife
{
    public partial class HappyLife
    {
        [HarmonyPatch(typeof(Master), nameof(Master.Read))]
        public class MasterReadPatch
        {

            [DllImport("vnpipe", CallingConvention = CallingConvention.Cdecl)]
            public unsafe static extern int master_read(IntPtr master, byte* buf, int len);
            //public static MethodInfo master_read = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName == "GameData.GameDataBridge.VnPipe").GetType("Bridge").GetMethod("master_read", BindingFlags.Static | BindingFlags.NonPublic);
            public static unsafe bool Prefix(ref Master __instance, byte[] buf, int off, int len, ref int __result)
            {
                try
                {
                    if (__instance.disposed)
                    {
                        throw new Exception("Pipe broken: trying to read data after disposed.");
                    }

                    if (off < 0 || buf.Length < off + len)
                    {
                        throw new ArgumentException($"Offset {off} should be in range of [0, {buf.Length}(buffer length) + {len}(data size)]");
                    }

                    int num;
                    var m_ptr = (IntPtr)typeof(Master).GetField("m_ptr", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                    fixed (byte* ptr = buf)
                    {
                        num = master_read(m_ptr, (ptr + off), len);
                    }

                    if (num <= 0)
                    {
                        throw new Exception($"Pipe broken: {num} received.");
                    }
                    __result = num;
                }
                catch(Exception ex)
                {
                    __result = -1;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Master), nameof(Master.Write))]
        public class MasterWritePatch
        {

            [DllImport("vnpipe", CallingConvention = CallingConvention.Cdecl)]
            public unsafe static extern int master_write(IntPtr master, byte* buf, int len);
            //public static MethodInfo master_read = AppDomain.CurrentDomain.GetAssemblies().First(a => a.FullName == "GameData.GameDataBridge.VnPipe").GetType("Bridge").GetMethod("master_read", BindingFlags.Static | BindingFlags.NonPublic);
            public static unsafe bool Prefix(ref Master __instance, byte[] buf, int off, int len, ref int __result)
            {
                try
                {
                    if (__instance.disposed)
                    {
                        __result = -1;
                    }

                    if (off < 0 || buf.Length < off + len)
                    {
                        throw new ArgumentException($"Offset {off} should be in range of [0, {buf.Length}(buffer length) + {len}(data size)]");
                    }

                    int num;
                    var m_ptr = (IntPtr)typeof(Master).GetField("m_ptr", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);
                    fixed (byte* ptr = buf)
                    {
                        num = master_write(m_ptr, ptr + off, len);
                    }

                    if (num < 0)
                    {
                        throw new Exception($"Pipe broken: {num} written.");
                    }

                    __result = num;
                }
                catch (Exception ex)
                {
                    __result = -1;
                }
                return false;
            }
        }
    }
}
