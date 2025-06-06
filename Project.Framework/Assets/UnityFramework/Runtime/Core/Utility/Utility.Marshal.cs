﻿using System;

namespace UnityFramework
{
    public static partial class Utility
    {
        /// <summary>
        /// Marshal 相关的实用函数。
        /// </summary>
        /// <remarks>非托管内存相关的实用函数。</remarks>
        public static class Marshal
        {
            private const int BLOCK_SIZE = 1024 * 4;
            private static IntPtr _cachedHGlobalPtr = IntPtr.Zero;
            private static int _cachedHGlobalSize = 0;

            /// <summary>
            /// 获取缓存的从进程的非托管内存中分配的内存的大小。
            /// </summary>
            public static int CachedHGlobalSize => _cachedHGlobalSize;

            /// <summary>
            /// 确保从进程的非托管内存中分配足够大小的内存并缓存。
            /// </summary>
            /// <param name="ensureSize">要确保从进程的非托管内存中分配内存的大小。</param>
            public static void EnsureCachedHGlobalSize(int ensureSize)
            {
                if (ensureSize < 0)
                {
                    throw new GameFrameworkException("Ensure size is invalid.");
                }

                if (_cachedHGlobalPtr == IntPtr.Zero || _cachedHGlobalSize < ensureSize)
                {
                    FreeCachedHGlobal();
                    int size = (ensureSize - 1 + BLOCK_SIZE) / BLOCK_SIZE * BLOCK_SIZE;
                    _cachedHGlobalPtr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
                    _cachedHGlobalSize = size;
                }
            }

            /// <summary>
            /// 释放缓存的从进程的非托管内存中分配的内存。
            /// </summary>
            public static void FreeCachedHGlobal()
            {
                if (_cachedHGlobalPtr != IntPtr.Zero)
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(_cachedHGlobalPtr);
                    _cachedHGlobalPtr = IntPtr.Zero;
                    _cachedHGlobalSize = 0;
                }
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <returns>存储转换结果的二进制流。</returns>
            public static byte[] StructureToBytes<T>(T structure)
            {
                return StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)));
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <returns>存储转换结果的二进制流。</returns>
            internal static byte[] StructureToBytes<T>(T structure, int structureSize)
            {
                if (structureSize < 0)
                {
                    throw new GameFrameworkException("Structure size is invalid.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.StructureToPtr(structure, _cachedHGlobalPtr, true);
                byte[] result = new byte[structureSize];
                System.Runtime.InteropServices.Marshal.Copy(_cachedHGlobalPtr, result, 0, structureSize);
                return result;
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            public static void StructureToBytes<T>(T structure, byte[] result)
            {
                StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), result, 0);
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            internal static void StructureToBytes<T>(T structure, int structureSize, byte[] result)
            {
                StructureToBytes(structure, structureSize, result, 0);
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            /// <param name="startIndex">写入存储转换结果的二进制流的起始位置。</param>
            public static void StructureToBytes<T>(T structure, byte[] result, int startIndex)
            {
                StructureToBytes(structure, System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), result, startIndex);
            }

            /// <summary>
            /// 将数据从对象转换为二进制流。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structure">要转换的对象。</param>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="result">存储转换结果的二进制流。</param>
            /// <param name="startIndex">写入存储转换结果的二进制流的起始位置。</param>
            internal static void StructureToBytes<T>(T structure, int structureSize, byte[] result, int startIndex)
            {
                if (structureSize < 0)
                {
                    throw new GameFrameworkException("Structure size is invalid.");
                }

                if (result == null)
                {
                    throw new GameFrameworkException("Result is invalid.");
                }

                if (startIndex < 0)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                if (startIndex + structureSize > result.Length)
                {
                    throw new GameFrameworkException("Result length is not enough.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.StructureToPtr(structure, _cachedHGlobalPtr, true);
                System.Runtime.InteropServices.Marshal.Copy(_cachedHGlobalPtr, result, startIndex, structureSize);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <returns>存储转换结果的对象。</returns>
            public static T BytesToStructure<T>(byte[] buffer)
            {
                return BytesToStructure<T>(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer, 0);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <param name="startIndex">读取要转换的二进制流的起始位置。</param>
            /// <returns>存储转换结果的对象。</returns>
            public static T BytesToStructure<T>(byte[] buffer, int startIndex)
            {
                return BytesToStructure<T>(System.Runtime.InteropServices.Marshal.SizeOf(typeof(T)), buffer, startIndex);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <returns>存储转换结果的对象。</returns>
            internal static T BytesToStructure<T>(int structureSize, byte[] buffer)
            {
                return BytesToStructure<T>(structureSize, buffer, 0);
            }

            /// <summary>
            /// 将数据从二进制流转换为对象。
            /// </summary>
            /// <typeparam name="T">要转换的对象的类型。</typeparam>
            /// <param name="structureSize">要转换的对象的大小。</param>
            /// <param name="buffer">要转换的二进制流。</param>
            /// <param name="startIndex">读取要转换的二进制流的起始位置。</param>
            /// <returns>存储转换结果的对象。</returns>
            internal static T BytesToStructure<T>(int structureSize, byte[] buffer, int startIndex)
            {
                if (structureSize < 0)
                {
                    throw new GameFrameworkException("Structure size is invalid.");
                }

                if (buffer == null)
                {
                    throw new GameFrameworkException("Buffer is invalid.");
                }

                if (startIndex < 0)
                {
                    throw new GameFrameworkException("Start index is invalid.");
                }

                if (startIndex + structureSize > buffer.Length)
                {
                    throw new GameFrameworkException("Buffer length is not enough.");
                }

                EnsureCachedHGlobalSize(structureSize);
                System.Runtime.InteropServices.Marshal.Copy(buffer, startIndex, _cachedHGlobalPtr, structureSize);
                return (T)System.Runtime.InteropServices.Marshal.PtrToStructure(_cachedHGlobalPtr, typeof(T));
            }
        }
    }
}
