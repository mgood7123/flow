using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public class ScriptManager : IDisposable {

    static public class Debug {
        static DateTime past = DateTime.Now;
        static DateTime now = DateTime.Now;
        static readonly object LOCK = new();
        public static event Action<string> OnMessageEmitted = _ => {};
        static bool in_emit = false;
        static public void Log(string msg) {
            lock (LOCK) {
                if (in_emit) {
                    throw new Exception("Debug.Log cannot be called inside of OnMessageEmitted");
                }
                past = now;
                now = DateTime.Now;
                TimeSpan diff = now - past;
                string message = "[ now : " + now.Hour + ":" + now.Minute + ":" + now.Second + ":" + now.Millisecond + "] [ since last : " + diff.Hours + ":" + diff.Minutes + ":" + diff.Seconds + ":" + diff.Milliseconds + " ] " + msg;
                UnityEngine.Debug.Log(message);
                File.AppendAllText("./log.txt", message + "\n");
                in_emit = true;
                OnMessageEmitted(message);
                in_emit = false;
            }
        }
    }

    /*
        using System;

        public class Program
        {
            public static void Main()
            {
                int args = 30;
                Console.WriteLine("    public static class RunnableUtils {");
                for (int i = 0; i < args+1; i++) {
                    Console.Write("        public delegate void Runnable");
                    if (i != 0) {
                        Console.Write("<");
                        for (int i1 = 0; i1 < i; i1++) {
                            if (i1 != 0) {
                                Console.Write($", ");
                            }
                            Console.Write($"T{i1+1}");
                        }
                        Console.Write(">");
                    }
                    Console.Write("(");
                    if (i != 0) {
                        for (int i1 = 0; i1 < i; i1++) {
                            if (i1 != 0) {
                                Console.Write($", ");
                            }
                            Console.Write($"T{i1+1} arg{i1+1}");
                        }
                    }
                    Console.WriteLine(");");
                }
                for (int i = 0; i < args+1; i++) {
                    Console.Write("        public delegate R RunnableWithReturn");
                    Console.Write("<");
                    for (int i1 = 0; i1 < i; i1++) {
                        if (i1 != 0) {
                            Console.Write($", ");
                        }
                        Console.Write($"T{i1+1}");
                    }
                    if (i != 0) {
                        Console.Write($", ");
                    }
                    Console.Write($"R");
                    Console.Write(">");
                    Console.Write("(");
                    for (int i1 = 0; i1 < i; i1++) {
                        if (i1 != 0) {
                            Console.Write($", ");
                        }
                        Console.Write($"T{i1+1} arg{i1+1}");
                    }
                    Console.WriteLine(");");
                }
                Console.WriteLine("    }");
            }
        }
*/

    public static class RunnableUtils {
        public delegate void Runnable();
        public delegate void Runnable<T1>(T1 arg1);
        public delegate void Runnable<T1, T2>(T1 arg1, T2 arg2);
        public delegate void Runnable<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3);
        public delegate void Runnable<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        public delegate void Runnable<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27, T28 arg28);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27, T28 arg28, T29 arg29);
        public delegate void Runnable<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27, T28 arg28, T29 arg29, T30 arg30);
        public delegate R RunnableWithReturn<R>();
        public delegate R RunnableWithReturn<T1, R>(T1 arg1);
        public delegate R RunnableWithReturn<T1, T2, R>(T1 arg1, T2 arg2);
        public delegate R RunnableWithReturn<T1, T2, T3, R>(T1 arg1, T2 arg2, T3 arg3);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27, T28 arg28);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27, T28 arg28, T29 arg29);
        public delegate R RunnableWithReturn<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, T17 arg17, T18 arg18, T19 arg19, T20 arg20, T21 arg21, T22 arg22, T23 arg23, T24 arg24, T25 arg25, T26 arg26, T27 arg27, T28 arg28, T29 arg29, T30 arg30);
    }

    BufferingFileSystemWatcher fs;
    private bool disposedValue;

    public class CompileInfo {
        internal readonly object Lock = new object();
        internal string path;

        internal CancellationTokenSource compilation_token;

        internal Task current_task;
        internal Assembly current_assembly;

        internal Task pending_task;
        internal Assembly pending_assembly;

        internal Action<CompileInfo> OnCompilationComplete;

        internal Dictionary<string, ClassInfo> classList = new();
        internal Dictionary<string, ClassInfo> classListStatic = new();
        internal bool compiled;

        public string Path => path;

        public Assembly CurrentAssembly => current_assembly;

        public CompileInfo SetTask(Task task) {
            this.pending_task = task;
            return this;
        }

        public CompileInfo SetCompilationToken(CancellationTokenSource token) {
            compilation_token = token;
            return this;
        }

        public CompileInfo SetCompletionCallback(Action<CompileInfo> value) {
            OnCompilationComplete = value;
            return this;
        }

        internal CompileInfo SetPath(string path) {
            this.path = path;
            return this;
        }

        public class ClassInfo {
            internal object instance;
            internal Type type;
            internal DynamicCall call;
            internal CompileInfo info;

            public ClassInfo(CompileInfo info) {
                this.info = info;
            }

            public ClassInfo(CompileInfo info, Type type, DynamicCall call) {
                this.info = info;
                this.type = type;
                this.call = call;
            }

            public ClassInfo(CompileInfo info, object instance, DynamicCall call) {
                this.info = info;
                this.instance = instance;
                this.call = call;
            }
        }
    };

    public class DynamicCall {
        Dictionary<Type, object> call_helper_map = new();
        public TDelegate Get<TDelegate>(string path, object instance, string methodName) where TDelegate : Delegate => Get(path, instance, methodName, out TDelegate del) ? del : null;
        public TDelegate Get<TDelegate>(string path, Type type, string methodName) where TDelegate : Delegate => Get(path, type, methodName, out TDelegate del) ? del : null;
        public bool Get<TDelegate>(string path, object instance, string methodName, out TDelegate del) where TDelegate : Delegate {
            if (call_helper_map.TryGetValue(typeof(TDelegate), out object map)) {
                return ((CallHelper<TDelegate>)map).GetDelegate(path, instance, methodName, out del);
            } else {
                map = new CallHelper<TDelegate>();
                call_helper_map.Add(typeof(TDelegate), map);
                return ((CallHelper<TDelegate>)map).GetDelegate(path, instance, methodName, out del);
            }
        }

        public bool Get<TDelegate>(string path, Type type, string methodName, out TDelegate del) where TDelegate : Delegate {
            if (!call_helper_map.TryGetValue(typeof(TDelegate), out object map)) {
                map = new CallHelper<TDelegate>();
                call_helper_map.Add(typeof(TDelegate), map);
            }
            return ((CallHelper<TDelegate>)map).GetDelegate(path, type, methodName, out del);
        }

        internal class CallHelper<TDelegate> where TDelegate : Delegate {
            private readonly Dictionary<string, TDelegate> lookup = new();
            private readonly Dictionary<string, TDelegate> lookupStatic = new();
            private readonly Type[] typesStatic = Array.ConvertAll(typeof(TDelegate).GetMethod("Invoke").GetParameters(), o => o.ParameterType);
            internal bool GetDelegate(string path, object instance, string methodName, out TDelegate del) {
                Type instance_type = instance.GetType();

                string key = methodName;
                if (lookup.TryGetValue(key, out del)) {
                    return true;
                }

                Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <obtaining methods in {instance_type} with signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                var instanceAttemptList = instance_type.GetMethods();
                if (instanceAttemptList.Length == 0) {
                    Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <no methods found on {instance_type} with signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                    return false;
                }
                for (int i2 = 0; i2 < instanceAttemptList.Length; i2++) {
                    MethodInfo i = instanceAttemptList[i2];
                    if (i.Name == methodName) {
                        Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <found candidate: Method {methodName} with signature {i} on {instance_type} with signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                        if (i.IsStatic) {
                            Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <must not be static: Method {methodName} with signature {i} on {instance_type} with signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                            continue;
                        }
                        ParameterInfo[] array = i.GetParameters();
                        int array_expected_length = typesStatic.Length - 1;
                        if (array.Length != array_expected_length) {
                            Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <argument length mismatch (argument length [{array.Length}], expected length [{array_expected_length}]) : Method {methodName} with signature {i} on {instance_type} with signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                            continue;
                        } else {
                            bool matches = true;
                            for (int i1 = 0; i1 < array.Length; i1++) {
                                if (array[i1].ParameterType != typesStatic[i1 + 1]) {
                                    Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <argument type mismatch (expected type [{typesStatic[i1 + 1]}], actual type [{array[i1].ParameterType}]) : Method {methodName} with signature {i} expects signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                                    matches = false;
                                    break;
                                }
                            }
                            if (!matches) {
                                continue;
                            }
                            try {
                                DynamicMethod m = new(key, typeof(TDelegate).GetMethod("Invoke").ReturnType, typesStatic);
                                var ilGen = m.GetILGenerator();

                                if (instance_type.IsValueType) {
                                    ilGen.Emit(OpCodes.Ldarga_S, 0);
                                } else {
                                    ilGen.Emit(OpCodes.Ldarg_0);
                                }
                                for (int i3 = 0; i3 < array_expected_length; i3++) {
                                    ilGen.Emit(OpCodes.Ldarg, i3 + 1);
                                }
                                ilGen.EmitCall(OpCodes.Callvirt, i, null);
                                ilGen.Emit(OpCodes.Ret);
                                del = (TDelegate)m.CreateDelegate(typeof(TDelegate));
                                Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <created delegate '{typeof(TDelegate)}' from the method '{methodName}' with signature '{(i.IsPublic ? "public " : i.IsPrivate ? "private " : "")}{(i.IsVirtual ? "/* virtual */ " : "")}{i}' from the class '{instance_type}'>");
                            } catch (Exception e) {
                                Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <an error occured while creating a delegate '{typeof(TDelegate)}' from the method '{methodName}' with signature '{(i.IsPublic ? "public " : i.IsPrivate ? "private " : "")}{(i.IsVirtual ? "/* virtual */ " : "")}{i}' from the class '{instance_type}'> : {e}");
                                return false;
                            }
                            lookup[key] = del;
                            return true;
                        }
                    }
                }
                Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <Path : {path}> <Method {methodName} not found on {instance_type} with signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                return false;
            }
            internal bool GetDelegate(string path, Type instance_type, string methodName, out TDelegate del) {

                string key = methodName;
                if (lookupStatic.TryGetValue(key, out del)) {
                    return true;
                }

                Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <obtaining methods in {instance_type} with signature {typeof(TDelegate)}>");
                var instanceAttemptList = instance_type.GetMethods();
                if (instanceAttemptList.Length == 0) {
                    Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <no methods found on {instance_type} with signature {typeof(TDelegate)}>");
                    return false;
                }
                for (int i2 = 0; i2 < instanceAttemptList.Length; i2++) {
                    MethodInfo i = instanceAttemptList[i2];
                    if (i.Name == methodName) {
                        Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <found candidate: Method {methodName} with signature {i} on {instance_type} with signature {typeof(TDelegate)}>");
                        if (!i.IsStatic) {
                            Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <must be static: Method {methodName} with signature {i} on {instance_type} with signature {typeof(TDelegate)}>");
                            continue;
                        }
                        ParameterInfo[] array = i.GetParameters();
                        int array_expected_length = typesStatic.Length;
                        if (array.Length != array_expected_length) {
                            Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <argument length mismatch (argument length [{array.Length}], expected length [{array_expected_length}]) : Method {methodName} with signature {i} on {instance_type} with signature {typeof(TDelegate)}>");
                            continue;
                        } else {
                            bool matches = true;
                            for (int i1 = 0; i1 < array.Length; i1++) {
                                if (array[i1].ParameterType != typesStatic[i1]) {
                                    Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <argument type mismatch (expected type [{typesStatic[i1]}], actual type [{array[i1].ParameterType}]) : Method {methodName} with signature {i} expects signature (the initial object is hidden 'this') {typeof(TDelegate)}>");
                                    matches = false;
                                    break;
                                }
                            }
                            if (!matches) {
                                continue;
                            }
                            try {
                                del = (TDelegate)i.CreateDelegate(typeof(TDelegate));
                                Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <created delegate '{typeof(TDelegate)}' from the method '{methodName}' with signature 'static {(i.IsPublic ? "public " : i.IsPrivate ? "private " : "")}{(i.IsVirtual ? "/* virtual */ " : "")}{i}' from the class '{instance_type}'>");
                            } catch (Exception e) {
                                Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <an error occured while creating a delegate '{typeof(TDelegate)}' from the method '{methodName}' with signature 'static {(i.IsPublic ? "public " : i.IsPrivate ? "private " : "")}{(i.IsVirtual ? "/* virtual */ " : "")}{i}' from the class '{instance_type}'> : {e}");
                                return false;
                            }
                            lookupStatic[key] = del;
                            return true;
                        }
                    }
                }
                Debug.Log($"COMPILER: STATIC INVOKE <INTERNAL> <Path : {path}> <Method {methodName} not found on {instance_type} with signature {typeof(TDelegate)}>");
                return false;
            }
        }
    }

    Dictionary<string, CompileInfo> compile_queue = new();

    DirectoryInfo tmp_dir;

    void append_string(List<byte> input, string content) {
        for (int i = 0; i < content.Length; i++) {
            input.Add((byte)content[i]);
        }
    }

    void append_char(List<byte> input, char content) {
        input.Add((byte)content);
    }

    class ArraySplit {
        ArraySegment<byte> left;
        ArraySegment<byte> middle;
        ArraySegment<byte> right;

        public ArraySplit(byte[] bytes, int offset, int len) {
            left = new ArraySegment<byte>(bytes, 0, offset);
            middle = new ArraySegment<byte>(bytes, offset, len);
            right = new ArraySegment<byte>(bytes, offset + len, bytes.Length - (offset + len));
        }

        public string left_to_utf8 => Encoding.UTF8.GetString(left.ToArray());
        public string middle_to_utf8 => Encoding.UTF8.GetString(middle.ToArray());
        public string right_to_utf8 => Encoding.UTF8.GetString(right.ToArray());

        public string left_to_base64 => Convert.ToBase64String(left.ToArray());
        public string middle_to_base64 => Convert.ToBase64String(middle.ToArray());
        public string right_to_base64 => Convert.ToBase64String(right.ToArray());

        public string left_to_hex => string.Concat(Array.ConvertAll(left.ToArray(), b => b.ToString("X2")));
        public string middle_to_hex => string.Concat(Array.ConvertAll(middle.ToArray(), b => b.ToString("X2")));
        public string right_to_hex => string.Concat(Array.ConvertAll(right.ToArray(), b => b.ToString("X2")));
    }

    DirectoryInfo TmpDir(string dir, string prefix, string suffix) {
        var rng = RandomNumberGenerator.Create();
        List<byte> s = new List<byte>();
        append_string(s, dir);
        append_char(s, '/');
        append_string(s, prefix);
        int offset = s.Count;
        append_string(s, "xxx");
        append_string(s, suffix);
        byte[] bytes = s.ToArray();
        while (true) {
            rng.GetBytes(bytes, offset, 3); // X -> XX
            ArraySplit a = new ArraySplit(bytes, offset, 3);
            string path = new StringBuilder().Append(a.left_to_utf8).Append(a.middle_to_hex).Append(a.right_to_utf8).ToString();
            Debug.Log("attempting to create directory: " + path);
            try {
                return Directory.CreateDirectory(path);
            } catch (Exception e) {
                if (e is IOException && Directory.Exists(path)) {
                    // directory exists, try another
                    continue;
                }
                throw e;
            }
        }
    }

    FileStream TmpFile(string dir, string prefix, string suffix) {
        var rng = RandomNumberGenerator.Create();
        List<byte> s = new List<byte>();
        append_string(s, dir);
        append_char(s, '/');
        append_string(s, prefix);
        int offset = s.Count;
        append_string(s, "xxx");
        append_string(s, suffix);
        byte[] bytes = s.ToArray();
        while (true) {
            rng.GetBytes(bytes, offset, 3); // X -> XX
            ArraySplit a = new ArraySplit(bytes, offset, 3);
            string path = new StringBuilder().Append(a.left_to_utf8).Append(a.middle_to_hex).Append(a.right_to_utf8).ToString();
            Debug.Log("attempting to create file: " + path);
            try {
                FileStream tmp = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.DeleteOnClose);
                new FileInfo(tmp.Name).Attributes = FileAttributes.Temporary;
                return tmp;
            } catch (Exception e) {
                if (e is IOException && File.Exists(path)) {
                    // file exists, try another
                    continue;
                }
                throw e;
            }
        }
    }

    // serialize  https://github.com/6bee/aqua-core

    public static bool instantiate_class(CompileInfo compileInfo, string @class, out CompileInfo.ClassInfo instance) {
        lock (compileInfo.classList) {
            return instantiate_class_locked(compileInfo, @class, out instance);
        }
    }

    private static bool instantiate_class_base(CompileInfo compileInfo, string @class, out CompileInfo.ClassInfo instance) {
        try {
            instance = new CompileInfo.ClassInfo(compileInfo, compileInfo.current_assembly.CreateInstance(@class), new DynamicCall());
        } catch (Exception e) {
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <an error occured while instantiating the class '" + @class + "'> : " + e);
            instance = null;
            return false;
        }
        if (instance == null) {
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <an error occured while instantiating the class '" + @class + "'> : ClassInfo returned null");
            return false;
        }

        if (instance.instance == null) {
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <the class '" + @class + "' was not found>");
            return false;
        }
        Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <class '" + @class + "' instantiated from assembly: '" + compileInfo.Path + "'>");
        return true;
    }

    private static bool get_class_type(CompileInfo compileInfo, string @class, out CompileInfo.ClassInfo type) {
        try {
            type = new CompileInfo.ClassInfo(compileInfo, compileInfo.current_assembly.GetType(@class), new DynamicCall());
        } catch (Exception e) {
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <an error occured while instantiating the class '" + @class + "'> : " + e);
            type = null;
            return false;
        }
        if (type == null || type.type == null) {
            return false;
        }
        Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <class '" + @class + "' instantiated from assembly: '" + compileInfo.Path + "'>");
        return true;
    }

    private static bool instantiate_class_locked(CompileInfo compileInfo, string @class, out CompileInfo.ClassInfo instance) {
        if (compileInfo.classList.TryGetValue(@class, out instance)) {
            return true;
        } else {
            if (!instantiate_class_base(compileInfo, @class, out var m)) {
                instance = null;
                return false;
            } else {
                instance = m;
                compileInfo.classList.Add(@class, instance);
                return true;
            }
        }
    }

    private static bool get_type_class_locked(CompileInfo compileInfo, string @class, out CompileInfo.ClassInfo type) {
        if (compileInfo.classListStatic.TryGetValue(@class, out type)) {
            return true;
        } else {
            if (!get_class_type(compileInfo, @class, out var m)) {
                type = null;
                return false;
            } else {
                type = m;
                compileInfo.classListStatic.Add(@class, type);
                return true;
            }
        }
    }

    private static bool obtain_method_from_instance_locked<TDelegate>(CompileInfo.ClassInfo instance, string method_name, out TDelegate del) where TDelegate : Delegate {
        bool v = instance.call.Get(instance.info.Path, instance.instance, method_name, out del);
        return v;
    }

    public static bool obtain_method_from_instance<TDelegate>(CompileInfo.ClassInfo instance, string method_name, out TDelegate del) where TDelegate : Delegate {
        bool v = instance.call.Get(instance.info.Path, instance.instance, method_name, out del);
        return v;
    }

    private static bool obtain_method_from_static_locked<TDelegate>(CompileInfo.ClassInfo type, string method_name, out TDelegate del) where TDelegate : Delegate
        => type.call.Get(type.info.Path, type.type, method_name, out del);

    private static bool obtain_method_locked<TDelegate>(CompileInfo compileInfo, string @class, string method_name, out object instance, out TDelegate del) where TDelegate : Delegate {
        if (!instantiate_class_locked(compileInfo, @class, out CompileInfo.ClassInfo instance_)) {
            del = null;
            instance = null;
            return false;
        } else {
            instance = instance_.instance;
            return obtain_method_from_instance_locked(instance_, method_name, out del);
        }
    }

    private static bool obtain_static_method_locked<TDelegate>(CompileInfo compileInfo, string @class, string method_name, out TDelegate del) where TDelegate : Delegate {
        if (!get_type_class_locked(compileInfo, @class, out CompileInfo.ClassInfo instance)) {
            del = null;
            return false;
        } else {
            return obtain_method_from_static_locked(instance, method_name, out del);
        }
    }

    public static bool obtain_method<TDelegate>(CompileInfo compileInfo, string @class, string method_name, out object instance, out TDelegate del) where TDelegate : Delegate {
        lock (compileInfo.classList) {
            return obtain_method_locked(compileInfo, @class, method_name, out instance, out del);
        }
    }

    public static bool obtain_static_method<TDelegate>(CompileInfo compileInfo, string @class, string method_name, out TDelegate del) where TDelegate : Delegate {
        lock (compileInfo.classListStatic) {
            return obtain_static_method_locked(compileInfo, @class, method_name, out del);
        }
    }

    public delegate void ex(Dictionary<string, CompileInfo> compile_queue, string @class, string method, params object[] @params);
    public delegate void exd(CompileInfo compileInfo, string @class, string method, params object[] @params);
    public delegate void exp(CompileInfo compileInfo, string @class, string method);

    private static class MiniCompile<TDelegate> where TDelegate : Delegate {
        private static Assembly assembly;

        private static ex assembly_delegate;
        private static ex assembly_delegate_static;
        private static exd assembly_delegate_direct;

        public static void compile_and_execute(MetadataReference[] refs, Dictionary<string, CompileInfo> compile_queue, string @class, string method, params object[] @params) {
            if (assembly_delegate != null) {
                assembly_delegate(compile_queue, @class, method, @params);
            } else {
                if (assembly == null) {
                    if (!loadAssembly(refs)) {
                        return;
                    }
                }

                try {
                    Type loop = assembly.GetType("loop");
                    try {
                        MethodInfo execute = loop.GetMethod("execute", BindingFlags.Static | BindingFlags.Public);
                        if (execute != null) {
                            try {
                                assembly_delegate = (ex)execute.CreateDelegate(typeof(ex));
                            } catch (Exception e) {
                                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while creating a delegate from the method 'execute' from the class 'loop'> : " + e);
                                return;
                            }
                        } else {
                            Debug.Log("COMPILER: COMPILE <INTERNAL> <failed to find the method 'execute' from the class 'loop'>");
                            return;
                        }
                    } catch (Exception e) {
                        Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the method 'execute' from the class 'loop'> : " + e);
                        return;
                    }
                } catch (Exception e) {
                    Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the type 'loop'> : " + e);
                    return;
                }
                assembly_delegate(compile_queue, @class, method, @params);
            }
        }

        public static void compile_and_execute_static(MetadataReference[] refs, Dictionary<string, CompileInfo> compile_queue, string @class, string method, params object[] @params) {
            if (assembly_delegate_static != null) {
                assembly_delegate_static(compile_queue, @class, method, @params);
            } else {
                if (assembly == null) {
                    if (!loadAssembly(refs)) {
                        return;
                    }
                }

                try {
                    Type loop = assembly.GetType("loop");
                    try {
                        MethodInfo execute = loop.GetMethod("execute_static", BindingFlags.Static | BindingFlags.Public);
                        if (execute != null) {
                            try {
                                assembly_delegate_static = (ex)execute.CreateDelegate(typeof(ex));
                            } catch (Exception e) {
                                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while creating a delegate from the method 'execute_static' from the class 'loop'> : " + e);
                                return;
                            }
                        } else {
                            Debug.Log("COMPILER: COMPILE <INTERNAL> <failed to find the method 'execute_static' from the class 'loop'>");
                            return;
                        }
                    } catch (Exception e) {
                        Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the method 'execute_static' from the class 'loop'> : " + e);
                        return;
                    }
                } catch (Exception e) {
                    Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the type 'loop'> : " + e);
                    return;
                }
                assembly_delegate_static(compile_queue, @class, method, @params);
            }
        }

        public static void compile_and_execute_direct(MetadataReference[] refs, CompileInfo compileInfo, string @class, string method, params object[] @params) {
            if (assembly_delegate_direct != null) {
                assembly_delegate_direct(compileInfo, @class, method, @params);
            } else {
                if (assembly == null) {
                    if (!loadAssembly(refs)) {
                        return;
                    }
                }

                try {
                    Type loop = assembly.GetType("loop");
                    try {
                        MethodInfo execute = loop.GetMethod("execute_direct", BindingFlags.Static | BindingFlags.Public);
                        if (execute != null) {
                            try {
                                assembly_delegate_direct = (exd)execute.CreateDelegate(typeof(exd));
                            } catch (Exception e) {
                                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while creating a delegate from the method 'execute_direct' from the class 'loop'> : " + e);
                                return;
                            }
                        } else {
                            Debug.Log("COMPILER: COMPILE <INTERNAL> <failed to find the method 'execute_direct' from the class 'loop'>");
                            return;
                        }
                    } catch (Exception e) {
                        Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the method 'execute_direct' from the class 'loop'> : " + e);
                        return;
                    }
                } catch (Exception e) {
                    Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the type 'loop'> : " + e);
                    return;
                }
                assembly_delegate_direct(compileInfo, @class, method, @params);
            }
        }

        public static bool compile_and_obtain_direct(MetadataReference[] refs, out exd del) {
            if (assembly_delegate_direct != null) {
                del = assembly_delegate_direct;
                return true;
            } else {
                if (assembly == null) {
                    if (!loadAssembly(refs)) {
                        del = null;
                        return false;
                    }
                }

                try {
                    Type loop = assembly.GetType("loop");
                    try {
                        MethodInfo execute = loop.GetMethod("execute_direct", BindingFlags.Static | BindingFlags.Public);
                        if (execute != null) {
                            try {
                                assembly_delegate_direct = (exd)execute.CreateDelegate(typeof(exd));
                            } catch (Exception e) {
                                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while creating a delegate from the method 'execute_direct' from the class 'loop'> : " + e);
                                del = null;
                                return false;
                            }
                        } else {
                            Debug.Log("COMPILER: COMPILE <INTERNAL> <failed to find the method 'execute_direct' from the class 'loop'>");
                            del = null;
                            return false;
                        }
                    } catch (Exception e) {
                        Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the method 'execute_direct' from the class 'loop'> : " + e);
                        del = null;
                        return false;
                    }
                } catch (Exception e) {
                    Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the type 'loop'> : " + e);
                    del = null;
                    return false;
                }
                del = assembly_delegate_direct;
                return true;
            }
        }

        public static bool precompile(MetadataReference[] refs, CompileInfo compileInfo, string @class, string method, out exd del) {
            if (assembly_delegate_direct != null) {
                del = assembly_delegate_direct;
                return true;
            } else {
                if (assembly == null) {
                    if (!loadAssembly(refs)) {
                        del = null;
                        return false;
                    }
                }

                try {
                    Type loop = assembly.GetType("loop");
                    try {
                        MethodInfo execute = loop.GetMethod("precompile", BindingFlags.Static | BindingFlags.Public);
                        if (execute != null) {
                            try {
                                var deleg = (exp)execute.CreateDelegate(typeof(exp));
                                deleg(compileInfo, @class, method);
                                MethodInfo execute_ = loop.GetMethod("execute_direct", BindingFlags.Static | BindingFlags.Public);
                                if (execute_ != null) {
                                    try {
                                        assembly_delegate_direct = (exd)execute_.CreateDelegate(typeof(exd));
                                    } catch (Exception e) {
                                        Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while creating a delegate from the method 'execute_direct' from the class 'loop'> : " + e);
                                        del = null;
                                        return false;
                                    }
                                } else {
                                    Debug.Log("COMPILER: COMPILE <INTERNAL> <failed to find the method 'execute_direct' from the class 'loop'>");
                                    del = null;
                                    return false;
                                }
                            } catch (Exception e) {
                                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while creating a delegate from the method 'precompile' from the class 'loop'> : " + e);
                                del = null;
                                return false;
                            }
                        } else {
                            Debug.Log("COMPILER: COMPILE <INTERNAL> <failed to find the method 'precompile' from the class 'loop'>");
                            del = null;
                            return false;
                        }
                    } catch (Exception e) {
                        Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the method 'precompile' from the class 'loop'> : " + e);
                        del = null;
                        return false;
                    }
                } catch (Exception e) {
                    Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while obtaining the type 'loop'> : " + e);
                    del = null;
                    return false;
                }
                del = assembly_delegate_direct;
                return true;
            }
        }

        private static string GetCode(ParameterInfo[] parameterInfos, ref string k) {
            Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <generating code>");
            StringBuilder b = new StringBuilder();
            b.Append("using System.Collections.Generic;\n");
            b.Append("using static ScriptManager;\n");
            b.Append("public static class loop {\n");
            b.Append("    public static void execute_static(Dictionary<string, CompileInfo> compile_queue, string @class, string method, params object[] @params) {\n");
            b.Append("        foreach (var item in compile_queue) {\n");
            b.Append("            CompileInfo compileInfo = item.Value;\n");
            b.Append("            if (compileInfo.CurrentAssembly != null) {\n");
            b.Append($"                if (obtain_static_method<{k}>(compileInfo, @class, method, out {k} del))\n");
            if (parameterInfos.Length == 0) {
                b.Append("                    del();\n");
            } else {
                b.Append("                    del(");
                for (int i = 0; i < parameterInfos.Length; i++) {
                    ParameterInfo p = parameterInfos[i];
                    string k2 = Regex.Replace(p.ParameterType.ToString(), "`\\d+", "").Replace('[', '<').Replace(']', '>').Replace('+', '.');
                    if (k2[k2.Length - 1] == '&') {
                        k2 = k2.Substring(0, k2.Length - 1);
                    }

                    if (i != 0) {
                        b.Append($", ");
                    }

                    b.Append($"({k2}) @params[{i}]");
                }
                b.Append(");\n");
            }
            b.Append("            }\n");
            b.Append("        }\n");
            b.Append("    }\n");
            b.Append("\n");
            b.Append("    public static void execute(Dictionary<string, CompileInfo> compile_queue, string @class, string method, params object[] @params) {\n");
            b.Append("        foreach (var item in compile_queue) {\n");
            b.Append("            execute_direct(item.Value, @class, method, @params);\n");
            b.Append("        }\n");
            b.Append("    }\n");
            b.Append("\n");
            if (k.StartsWith("ScriptManager.RunnableUtils.RunnableWithReturn")) {
                k = Regex.Replace(k, "^ScriptManager.RunnableUtils.RunnableWithReturn<", "ScriptManager.RunnableUtils.RunnableWithReturn<object, ");
            } else if (k.StartsWith("ScriptManager.RunnableUtils.Runnable<")) {
                k = Regex.Replace(k, "^ScriptManager.RunnableUtils.Runnable<", "ScriptManager.RunnableUtils.Runnable<object, ");
            } else {
                k = Regex.Replace(k, "^ScriptManager.RunnableUtils.Runnable", "ScriptManager.RunnableUtils.Runnable<object>");
            }
            b.Append("    public static void execute_direct(CompileInfo compileInfo, string @class, string method, params object[] @params) {\n");
            b.Append("        if (compileInfo.CurrentAssembly != null) {\n");
            b.Append($"            if (obtain_method<{k}>(compileInfo, @class, method, out object instance, out {k} del)) {{\n");
            if (parameterInfos.Length == 0) {
                b.Append("                del(instance);\n");
            } else {
                b.Append("                del(instance, ");
                for (int i = 0; i < parameterInfos.Length; i++) {
                    ParameterInfo p = parameterInfos[i];
                    string k2 = Regex.Replace(p.ParameterType.ToString(), "`\\d+", "").Replace('[', '<').Replace(']', '>').Replace('+', '.');
                    if (k2[k2.Length - 1] == '&') {
                        k2 = k2.Substring(0, k2.Length - 1);
                    }

                    if (i != 0) {
                        b.Append($", ");
                    }

                    b.Append($"({k2}) @params[{i}]");
                }
                b.Append(");\n");
            }
            b.Append("            }\n");
            b.Append("        }\n");
            b.Append("    }\n");
            b.Append("\n");
            b.Append("    public static void precompile(CompileInfo compileInfo, string @class, string method) {\n");
            b.Append("        if (compileInfo.CurrentAssembly != null) {\n");
            b.Append($"            obtain_method<{k}>(compileInfo, @class, method, out _, out _);\n");
            b.Append("        }\n");
            b.Append("    }\n");
            b.Append("}\n");
            string v = b.ToString();
            Debug.Log($"COMPILER: INSTANCE INVOKE <INTERNAL> <generated code>");
            return v;
        }

        static bool loadAssembly(MetadataReference[] refs) {
            ParameterInfo[] parameterInfos = typeof(TDelegate).GetMethod("Invoke").GetParameters();
            string k = Regex.Replace(typeof(TDelegate).ToString(), "`\\d+", "").Replace('[', '<').Replace(']', '>').Replace('+', '.');
            if (!k.StartsWith("ScriptManager.RunnableUtils.Runnable")) {
                throw new ArgumentException("error: delegate must be one of 'Runnable' or 'RunnableWithReturn' from the static class 'ScriptManager.RunnableUtils'\nexample: import static ScriptManager.RunnableUtils;\n// ...\n...<Runnable>\nexample: <ScriptManager.RunnableUtils.Runnable>\n\nexample: import static ScriptManager.RunnableUtils;\n// ...\n...<Runnable<int>>\nexample: <ScriptManager.RunnableUtils.Runnable<int>>");
            }

            string code = GetCode(parameterInfos, ref k);

            Debug.Log("COMPILER: COMPILE <INTERNAL> <compiling the following script>\n" + code);

            CSharpCompilation script;
            try {
                List<MetadataReference> refsL = new(refs.Length + 1);
                refsL.AddRange(refs);
                refsL.Add(MetadataReference.CreateFromFile(typeof(CompileInfo).Assembly.Location));
                script = CSharpCompilation.Create(
                    "C# Script", // assembly name
                    new SyntaxTree[] { CSharpSyntaxTree.ParseText(SourceText.From(code)) },
                    options: new CSharpCompilationOptions(
                        outputKind: OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release,
                        metadataImportOptions: MetadataImportOptions.Internal
                    ),
                    references: refsL.ToArray()
                );
            } catch (Exception e) {
                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while creating the script object> : " + e);
                return false;
            }
            if (script == null) {
                Debug.Log("COMPILER: COMPILE <INTERNAL> <script could not be loaded>");
                return false;
            }
            Debug.Log("COMPILER: COMPILE <INTERNAL> <script loaded>");

            using var pending_stream = new MemoryStream();
            EmitResult emitResult;
            try {
                Debug.Log("COMPILER: COMPILE <INTERNAL> <emitting DLL to memory>");
                emitResult = script.Emit(pending_stream);
            } catch (Exception e) {
                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while emitting DLL to memory> : " + e);
                return false;
            }
            Debug.Log("COMPILER: COMPILE <INTERNAL> <emitted DLL to memory>");
            if (!emitResult.Success) {
                var sb = new StringBuilder();
                foreach (var diag in emitResult.Diagnostics) {
                    sb.AppendLine(diag.ToString());
                }
                emitResult = null;
                Debug.Log("COMPILER: COMPILE <INTERNAL> <assembly could not be emitted for script> : " + sb);
                return false;
            }
            Debug.Log("COMPILER: COMPILE <INTERNAL> <emitted DLL to memory successfully>");
            // api visibility changes do not affect the byte stream output
            Debug.Log("COMPILER: COMPILE <INTERNAL> <loading assembly>");
            try {
                assembly = Assembly.Load(pending_stream.ToArray());
            } catch (Exception e) {
                Debug.Log("COMPILER: COMPILE <INTERNAL> <an error occured while loading the assembly> : " + e);
                return false;
            }
            if (assembly == null) {
                Debug.Log("COMPILER: COMPILE <INTERNAL> <failed to load assembly for script>");
                return false;
            }
            Debug.Log("COMPILER: COMPILE <INTERNAL> <loaded assembly>");
            return true;
        }
    }

    public void invoke_static_callback<TDelegate>(string @class, string method, params object[] @params) where TDelegate : Delegate {
        lock (compile_queue) {
            MiniCompile<TDelegate>.compile_and_execute_static(refs, compile_queue, @class, method, @params);
        }
    }

    public void invoke_callback<TDelegate>(string @class, string method, params object[] @params) where TDelegate : Delegate {
        lock (compile_queue) {
            MiniCompile<TDelegate>.compile_and_execute(refs, compile_queue, @class, method, @params);
        }
    }

    public bool obtain_callback_direct<TDelegate>(out exd del) where TDelegate : Delegate {
        lock (compile_queue) {
            return MiniCompile<TDelegate>.compile_and_obtain_direct(refs, out del);
        }
    }

    public bool precompile<TDelegate>(CompileInfo compileInfo, string @class, string method, out exd del) where TDelegate : Delegate {
        lock (compile_queue) {
            return MiniCompile<TDelegate>.precompile(refs, compileInfo, @class, method, out del);
        }
    }

    public void invoke_callback_direct<TDelegate>(CompileInfo compileInfo, string @class, string method, params object[] @params) where TDelegate : Delegate {
        lock (compile_queue) {
            MiniCompile<TDelegate>.compile_and_execute_direct(refs, compileInfo, @class, method, @params);
        }
    }

    CompileInfo unload(string path) {
        lock (compile_queue) {
            if (compile_queue.Remove(path, out CompileInfo compileInfo)) {
                Debug.Log("COMPILER: UNLOADING: " + compileInfo.Path);
                try {
                    compileInfo.compilation_token.Cancel();
                } catch (Exception e) {
                    Debug.Log("COMPILER: ERROR CANCELLING EXISTING COMPILATION " + compileInfo.Path + " <token cancel EXCEPTION> : " + e);
                }
                try {
                    compileInfo.pending_task.Wait();
                } catch (Exception e) {
                    Debug.Log("COMPILER: ERROR WAITING FOR EXISTING COMPILATION " + compileInfo.Path + " <task wait EXCEPTION> : " + e);
                }

                if (compileInfo.compilation_token != null) {
                    compileInfo.compilation_token.Dispose();
                    compileInfo.compilation_token = null;
                }
                if (compileInfo.pending_task != null) {
                    compileInfo.pending_task.Dispose();
                    compileInfo.pending_task = null;
                }

                // we cannot retain any method/class info since the assembly has been invalidated
                //   we have no idea what could have changed
                lock (compileInfo.classList) {
                    compileInfo.current_assembly = compileInfo.pending_assembly;
                    compileInfo.pending_assembly = null;
                    compileInfo.classList.Clear();
                    compileInfo.classListStatic.Clear();
                }

                Debug.Log("COMPILER: UNLOADED: " + compileInfo.Path);
            }
            return compileInfo;
        }
    }

    void reload(string path) {
        CompileInfo compileInfo = null;
        bool present = false;
        lock (compile_queue) {
            present = compile_queue.TryGetValue(path, out compileInfo);
            if (!present) {
                // we have no instance
                compileInfo = new CompileInfo().SetPath(path);
                compile_queue.Add(compileInfo.Path, compileInfo);
            }
        }
        if (present && compileInfo.compiled) {
            // we have an active instance
            Debug.Log("COMPILER: STOPPING EXISTING COMPILATION: " + path);
            try {
                compileInfo.compilation_token.Cancel();
            } catch (Exception e) {
                Debug.Log("COMPILER: ERROR CANCELLING EXISTING COMPILATION " + compileInfo.Path + " <token cancel EXCEPTION> : " + e);
            }
            try {
                compileInfo.pending_task.Wait();
            } catch (Exception e) {
                Debug.Log("COMPILER: ERROR WAITING FOR EXISTING COMPILATION " + compileInfo.Path + " <task wait EXCEPTION> : " + e);
            }
            compileInfo.compilation_token.Dispose();
            Debug.Log("COMPILER: RECOMPILING: " + path);
            compileInfo
                .SetCompilationToken(new CancellationTokenSource())
                .SetCompletionCallback(complete_after_recompile)
                .SetTask(Task.Run(() => compile(compileInfo)));
        } else {
            Debug.Log("COMPILER: COMPILING: " + path);
            compileInfo
                .SetCompilationToken(new CancellationTokenSource())
                .SetCompletionCallback(complete_after_compile)
                .SetTask(Task.Run(() => compile(compileInfo)));
        }
    }

    private void complete_after_recompile(CompileInfo compileInfo) {
        // pending assembly must be non-null here
        // reload

        Debug.Log("COMPILER: RELOADING: " + compileInfo.Path);

        var data = new List<object>();
        if (obtain_method<Action<object, List<object>>>(compileInfo, "Main", "reload_begin", out var i1, out var r_1)) {
            try {
                r_1(i1, data);
            } catch (Exception e) {
                Debug.Log("caught exception while executing reload_begin: " + e);
            }
        }

        // we cannot retain any method/class info since the assembly has been invalidated
        //   we have no idea what could have changed
        lock (compileInfo.classList) {
            compileInfo.current_assembly = compileInfo.pending_assembly;
            compileInfo.pending_assembly = null;
            compileInfo.classList.Clear();
            compileInfo.classListStatic.Clear();
        }

        if (obtain_method<Action<object, List<object>>>(compileInfo, "Main", "reload_finish", out var i2, out var r_2)) {
            try {
                r_2(i2, data);
            } catch (Exception e) {
                Debug.Log("caught exception while executing reload_finish: " + e);
            }
        }

        Debug.Log("COMPILER: RELOADED: " + compileInfo.Path);
    }

    private void complete_after_compile(CompileInfo compileInfo) {
        // pending_assembly must be non-null here

        // fresh load

        Debug.Log("COMPILER: LOADING: " + compileInfo.Path);

        compileInfo.current_assembly = compileInfo.pending_assembly;
        compileInfo.pending_assembly = null;
        compileInfo.compiled = true;
        if (onScriptLoaded != null) {
            onScriptLoaded(compileInfo);
        }
    }

    private void compile(CompileInfo compileInfo) {
        Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " STARTING COMPILATION JOB");
        try {
            if (compileInfo.compilation_token.IsCancellationRequested) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " COMPILATION JOB HAS BEEN CANCELLED");
                return;
            }
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <script loading ... >");
            CSharpCompilation script;
            try {
                using var t_f = new FileStream(compileInfo.Path, FileMode.Open, FileAccess.Read);
                script = CSharpCompilation.Create(
                    "C# Script", // assembly name
                    new SyntaxTree[] { CSharpSyntaxTree.ParseText(SourceText.From(t_f)) },
                    options: new CSharpCompilationOptions(
                        outputKind: OutputKind.DynamicallyLinkedLibrary,
                        optimizationLevel: OptimizationLevel.Release
                    ),
                    references: refs
                );
            } catch (Exception e) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <an error occured while creating the script object> : " + e);
                return;
            }
            if (compileInfo.compilation_token.IsCancellationRequested) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " COMPILATION JOB HAS BEEN CANCELLED");
                return;
            }
            if (script == null) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <script could not be loaded>");
                return;
            }
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <script loaded>");

            using var pending_stream = new MemoryStream();
            EmitResult emitResult;
            try {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <emitting DLL to memory>");
                emitResult = script.Emit(pending_stream);
            } catch (Exception e) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <an error occured while emitting DLL to memory> : " + e);
                return;
            }
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <emitted DLL to memory>");
            if (compileInfo.compilation_token.IsCancellationRequested) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " COMPILATION JOB HAS BEEN CANCELLED");
                return;
            }
            if (!emitResult.Success) {
                var sb = new StringBuilder();
                foreach (var diag in emitResult.Diagnostics) {
                    sb.AppendLine(diag.ToString());
                }
                emitResult = null;
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <assembly could not be emitted for script> : " + sb);
                return;
            }
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <emitted DLL to memory successfully>");
            if (compileInfo.compilation_token.IsCancellationRequested) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " COMPILATION JOB HAS BEEN CANCELLED");
                return;
            }
            script = null;
            // api visibility changes do not affect the byte stream output
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <loading assembly>");
            try {
                // each time an assembly is loaded, it gets loaded into memory
                //
                // if an assembly stream (byte array - byte[]) with the same assembly.GetName.name() is loaded twice
                // then it will not be cached and a duplicate assembly will exist in memory
                //
                // this means that if "foo" with code_1 and "foo" with code_2 are both loaded
                // then both code_1 and code_2 will exist in memory as unique assembly files
                //
                // asm_1 = foo code_1
                // asm_2 = foo code_2
                //
                // this means that additional memory will be used
                // if the same assembly name and the same code is loaded multiple times
                //
                // how much memory depends on the assembly size
                //
                // this also means any code currently executing will belong to the previous assembly
                // for code to be updated we must stop all code running in the old assembly
                // and continue in the new assembly
                //
                // eg
                //  asm_1.save(state).shutdown();
                //  asm_2.startup(state);
                //
                // save and shut down asm_1, all threads and code should stop in cooperation with the app
                // at this point the app should not be executing any more code from asm_1
                //
                // start asm_2 and try to resume from the saved state, this process should be as seamless as possible
                //
                //
                compileInfo.pending_assembly = Assembly.Load(pending_stream.ToArray());
            } catch (Exception e) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <an error occured while loading the assembly> : " + e);
                return;
            }
            if (compileInfo.pending_assembly == null) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <failed to load assembly for script>");
                return;
            }
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <loaded assembly>");
            if (compileInfo.compilation_token.IsCancellationRequested) {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " COMPILATION JOB HAS BEEN CANCELLED");
                return;
            }
            if (compileInfo.OnCompilationComplete != null) {
                compileInfo.OnCompilationComplete(compileInfo);
            } else {
                Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " compileInfo.OnCompilationComplete is null");
            }
        } catch (Exception e) {
            Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " <UNCAUGHT EXCEPTION> : " + e);
            return;
        }
        Debug.Log("COMPILER: COMPILE " + compileInfo.Path + " FINISHED COMPILATION JOB");
    }

    MetadataReference[] refs;

    Action<CompileInfo> onScriptLoaded;

    public ScriptManager(string dir, params Type[] types) => init(null, dir, types);
    public ScriptManager(Action<CompileInfo> onScriptLoaded, string dir, params Type[] types) => init(onScriptLoaded, dir, types);
    
    public void init(Action<CompileInfo> onScriptLoaded, string dir, params Type[] types) {
        try {
            Debug.Log("ScriptManager starting");
            this.onScriptLoaded = onScriptLoaded;
            List<MetadataReference> refsL = new List<MetadataReference>();

            Debug.Log("adding Net21/netstandard.dll");
            refsL.Add(MetadataReference.CreateFromFile("Net21/netstandard.dll"));
            Debug.Log("added Net21/netstandard.dll");

            for (int i = 0; i < types.Length; i++) {
                Debug.Log("adding " + types[i].Assembly.Location);
                refsL.Add(MetadataReference.CreateFromFile(types[i].Assembly.Location));
                Debug.Log("added " + types[i].Assembly.Location);
            }
            refs = refsL.ToArray();
            if (!File.Exists(Path.GetFullPath(dir))) {
                Debug.Log("Created directory: " + Path.GetFullPath(dir));
                Directory.CreateDirectory(Path.GetFullPath(dir));
            } else {
                Debug.Log("directory exists: " + Path.GetFullPath(dir));
            }

            string tmp_path = Path.GetFullPath(dir + "/TMP");
            if (Directory.Exists(tmp_path)) {
                Debug.Log("cleaning directory: " + tmp_path);
                Directory.Delete(tmp_path, true);
                Debug.Log("cleaned directory: " + tmp_path);
            }
            try {
                tmp_dir = TmpDir(Path.GetFullPath(dir), "TMP/", "");
            } catch (Exception e) {
                Debug.Log("failed to create temp directory in folder: " + Path.GetFullPath(dir) + " : " + e);
                return;
            }
            Debug.Log("temp directory created: " + tmp_dir.FullName);

            Debug.Log("file watcher started, Directory: " + Path.GetFullPath(dir));
            fs = new BufferingFileSystemWatcher(dir, "*.cs");
            fs.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fs.Existed += Fs_Existed;
            fs.Created += Fs_Created;
            fs.Changed += Fs_Changed;
            fs.Deleted += Fs_Deleted;
            fs.Renamed += Fs_Renamed;
            fs.Error += Fs_Error;
            fs.IncludeSubdirectories = true;
            fs.EnableRaisingEvents = true;
        } catch (Exception e) {
            Debug.Log("CAUGHT EXCEPTION while initializing ScriptManager : " + e);
        }
    }

    protected virtual void Dispose(bool disposing) {
        if (!disposedValue) {
            if (disposing) {
                // TODO: dispose managed state (managed objects)
                fs.Dispose();
                fs = null;
            }
            disposedValue = true;
        }
    }

    public void Dispose() {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Fs_Error(object sender, ErrorEventArgs e) {
        Debug.Log("fs error: " + e.GetException());
        if (e.GetException().GetType() == typeof(InternalBufferOverflowException)) {
            //  This can happen if Windows is reporting many file system events quickly
            //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
            //  rate of events. The InternalBufferOverflowException error informs the application
            //  that some of the file system events are being lost.
            Debug.Log("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message);
        }
    }

    private void Fs_Created(object sender, FileSystemEventArgs e) {
        Debug.Log("fs created: " + e.Name);
        reload(e.FullPath);
    }

    private void Fs_Existed(object sender, FileSystemEventArgs e) {
        Debug.Log("fs existed: " + e.Name);
        reload(e.FullPath);
    }

    private void Fs_Changed(object sender, FileSystemEventArgs e) {
        Debug.Log("fs changed: " + e.Name);
        reload(e.FullPath);
    }

    private void Fs_Renamed(object sender, RenamedEventArgs e) {
        Debug.Log("fs renamed: " + e.OldName + " -> " + e.Name);
        unload(e.OldFullPath);
        reload(e.FullPath);
    }

    private void Fs_Deleted(object sender, FileSystemEventArgs e) {
        Debug.Log("fs deleted: " + e.Name);
        unload(e.FullPath);
    }

    public class ThreadRunner {
        Queue<Action<object>> currentFrame = new();
        Queue<Action<object>> nextFrame = new();

        static readonly object LOCK = new object();

        Thread main;

        public ThreadRunner() {
            main = Thread.CurrentThread;
        }

        public void RunOnBackground(Action action) {
            Task.Run(action);
        }

        public void RunOnMain(Action<object> action) {
            if (Thread.CurrentThread == main) {
                action(action);
            } else {
                lock (LOCK) {
                    nextFrame.Enqueue(action);
                }
            }
        }

        public void RunOnMain(object action) => RunOnMain((Action<object>)action);
        public void RunOnMain(Action action) => RunOnMain(_ => action());

        public void RunMainLoop() {
            lock (LOCK) {
                // swap action buffers
                var tmp = nextFrame; // full
                nextFrame = currentFrame; // full = empty
                currentFrame = tmp; // empty = full
            }
            while (currentFrame.TryDequeue(out var action)) {
                action(action);
            }
        }
    }
}
