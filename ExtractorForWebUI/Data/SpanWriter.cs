using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ExtractorForWebUI.Data;

public ref struct SpanWriter<T>
{
    Span<T> span;
    public int Count { get; private set; }

    public SpanWriter(Span<T> span)
    {
        this.span = span;
        Count = 0;
    }

    public void Write(T value)
    {
        this.span[Count] = value;
        Count++;
    }

    public void Write(ReadOnlySpan<T> values)
    {
        values.CopyTo(this.span[Count..]);
        Count += values.Length;
    }

    public void Write(List<T> values)
    {
        CollectionsMarshal.AsSpan(values).CopyTo(this.span[Count..]);
        Count += values.Count;
    }
}
