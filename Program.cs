using System.Runtime.InteropServices;

namespace myApp
{
    public unsafe class MyArray<T> : IDisposable 
        where T : unmanaged
    {
        public T this[int index] 
        {
            get => GetValue(index);
            set => SetValue(index, value);
        }
        public MyArray(int size)
        {
            Jump = sizeof(T);

            byteArr = new MyByteArray(size * Jump);
        }
        private void SetValue(int index, T val) 
        {
            var reference = __makeref(val);

            byte* ptr = *(byte**)&reference;

            for (int i = 0; i < Jump; i++, ptr++)
            {
                byteArr[index * Jump + i] = *ptr;
            }
        }
        private T GetValue(int index)
        {
            object obj = default(T);

            var tr = __makeref(obj);

            IntPtr ptr = **(IntPtr**)&tr;

            byte* tempArr = (byte*)(ptr + 8);

            for (int i = 0; i < Jump; i++, tempArr++)
            {
                *tempArr = byteArr[index * Jump + i];
            }

            return (T)obj;
        }
        public int Jump { get; init; }
        private MyByteArray byteArr { get; init; }
        public void Dispose() 
        {
            byteArr.Dispose();

            GC.SuppressFinalize(this);
        }
    }
    /// <summary>
    /// Simple unsafe in-memory byte array. U can use it with primitive types using transformation to your type
    /// </summary>
    internal unsafe class MyByteArray : IDisposable
    {
        public byte this[int index] 
        {
            get => GetValue(index);
            set => SetValue(index, value);
        }
        public MyByteArray(int size)
        {
            Size = size;

            IntPtr ptr = Marshal.AllocHGlobal(size);
            Handle = ptr;
        }
        public int Size;
        private void SetValue(int index, byte val) 
        {
            if (index < 0 || index >= Size) throw new IndexOutOfRangeException(index.ToString());

            byte* ptr = (byte*)(Handle + index);

            *ptr = val;
        }
        private byte GetValue(int index) 
        {
            if (index < 0 || index >= Size) throw new IndexOutOfRangeException(index.ToString());

            byte* ptr = (byte*)(Handle + index);

            return *ptr;
        }
        /// <summary>
        /// Gets a handle to the first element in heap allocated by the OS
        /// </summary>
        private IntPtr Handle { get; init; }
        public void Dispose() 
        {
            Marshal.FreeHGlobal(Handle);
            GC.SuppressFinalize(this);
        }
    }
}
