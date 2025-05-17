// Copyright (c) ktsu.dev
// All rights reserved.
// Licensed under the MIT license.

internal class RingBuffer<T>
{
	public static object bufferLock = new();

	public T[] buffer;
	public int back;
	public int front;
	public int capacity;
	public int length;

	public RingBuffer(int _length)
	{
		lock (bufferLock)
		{
			length = _length;
			capacity = (int)RingBuffer<T>.NextPower2((uint)_length);
			buffer = new T[capacity];
			back = capacity - 1;
			front = back - length;
			while (front < 0)
			{
				front += capacity;
			}
		}
	}

	public void PushBack(T o)
	{
		lock (bufferLock)
		{
			back = (back + 1) & (capacity - 1);
			front = (front + 1) & (capacity - 1);
			buffer[back] = o;
		}
	}

	public T At(int index)
	{
		lock (bufferLock)
		{
			var idx = (front + index) & (capacity - 1);
			return buffer[idx];
		}
	}

	public T Front()
	{
		lock (bufferLock)
		{
			return buffer[front];
		}
	}

	public T Back()
	{
		lock (bufferLock)
		{
			return buffer[back];
		}
	}

	private static uint NextPower2(uint v)
	{
		v--;
		v |= v >> 1;
		v |= v >> 2;
		v |= v >> 4;
		v |= v >> 8;
		v |= v >> 16;
		v++;
		return v;
	}

	public void Resize(int _length)
	{
		lock (bufferLock)
		{
			capacity = (int)RingBuffer<T>.NextPower2((uint)_length);
			length = _length;
			if (buffer.Length != capacity)
			{
				var newBuffer = new T[capacity];
				float resample = 0;
				if (buffer.Length > 0)
				{
					resample = newBuffer.Length / (float)buffer.Length;
					for (var i = 0; i < newBuffer.Length; ++i)
					{
						var oldIndex = (int)Math.Round(i / resample);
						oldIndex = Math.Min(oldIndex, buffer.Length - 1);
						newBuffer[i] = buffer[oldIndex];
					}
				}

				buffer = newBuffer;
				back = (int)(back * resample);
				front = back - length;
				while (front < 0)
				{
					front += capacity;
				}
			}
		}
	}
}
