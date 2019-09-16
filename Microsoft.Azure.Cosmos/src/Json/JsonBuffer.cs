﻿// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
// ------------------------------------------------------------
namespace Microsoft.Azure.Cosmos.Json
{
    using System;

    internal abstract class JsonBuffer
    {
        protected readonly ReadOnlyMemory<byte> buffer;
        protected int position;

        protected JsonBuffer(ReadOnlyMemory<byte> buffer)
        {
            this.buffer = buffer;
        }

        public bool IsEof
        {
            get { return this.position >= this.buffer.Length; }
        }

        public int Position
        {
            get
            {
                return this.position;
            }
        }

        public byte Read()
        {
            byte value = this.position < this.buffer.Length ? (byte)this.buffer.Span[this.position] : (byte)0;
            this.position++;
            return value;
        }

        public byte Peek()
        {
            byte value = this.position < this.buffer.Length ? (byte)this.buffer.Span[this.position] : (byte)0;
            return value;
        }

        public ReadOnlySpan<byte> GetBufferedRawJsonToken()
        {
            return this.buffer.Span;
        }

        public ReadOnlySpan<byte> GetBufferedRawJsonToken(int startPosition)
        {
            return this.buffer.Slice(startPosition).Span;
        }

        public ReadOnlySpan<byte> GetBufferedRawJsonToken(int startPosition, int endPosition)
        {
            return this.buffer.Slice(startPosition, endPosition - startPosition).Span;
        }
    }
}