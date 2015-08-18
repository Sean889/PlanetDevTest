/*
    Copyright (c) 2015 Sean Lynch

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
 */

using System;
using System.Runtime.InteropServices;
using System.Reflection;
using OpenTK.Graphics.OpenGL;

namespace PlanetDevTest
{
    unsafe class Program
    {
        [DllImport("opengl32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "wglGetProcAddress")]
        static extern IntPtr wglGetProcAddress(string name);
        [DllImport("opengl32.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "glDrawElements")]
        static extern void glDrawElements(int mode, int count, int type, void* indices);
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "GetProcAddress")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi, EntryPoint = "GetModuleHandle")]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        delegate void ImitDel(int mode, int count, int type, void* indices);

        //static ImitDel glDrawElements;

        static unsafe void glDrawElementsImit(int mode, int count, int type, void* indices)
        {
            glDrawElements(mode, count, type, indices);
        }

        static void InjectImitator(Delegate Act)
        {
            IntPtr ip = Marshal.GetFunctionPointerForDelegate(Act);
            IntPtr prev;

            FieldInfo Info = typeof(GL).GetField("EntryPoints", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            IntPtr[] Array = (IntPtr[])Info.GetValue(null);
            prev = Array[0x1b6];
            Array[0x1b6] = ip;

            IntPtr FuncPtr = wglGetProcAddress("glDrawElements");
            IntPtr NewPtr = GetProcAddress(GetModuleHandle("opengl32.dll"), "glDrawElements");
        }

        static void Main(string[] args)
        {
            using(Window Win = new Window())
            {
                unsafe
                {
                    //InjectImitator(new ImitDel(glDrawElementsImit));
                }
                Win.Run(30, 60);
            }
        }
    }
}
