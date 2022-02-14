﻿using System;
using System.Collections.Immutable;

namespace Biohazrd.CSharp.Trampolines;

public readonly struct TrampolineCollection : IDeclarationMetadataItem
{
    private readonly Trampoline _NativeFunction;
    private readonly Trampoline _PrimaryTrampoline;
    private ImmutableArray<Trampoline> _SecondaryTrampolines { get; init; }

    /// <summary>A dummy trampoline representing the low level native function for the associated function.</summary>
    /// <remarks>
    /// In the case of a function or non-virtual method, this represents the P/Invoke function.
    ///
    /// In the case of a virtual method this represents the virtual method function pointer.
    ///
    /// You generally do not want to target this trampoline directly and should either <see cref="PrimaryTrampoline"/> or clone <see cref="PrimaryTrampoline"/> instead.
    /// </remarks>
    public Trampoline NativeFunction
    {
        get => _NativeFunction;
        init
        {
            if (!value.IsNativeFunction)
            { throw new ArgumentException("The specified trampoline does not represent a native function.", nameof(value)); }

            if (value.TargetFunctionId != _NativeFunction.TargetFunctionId)
            { throw new ArgumentException("The specified trampoline belongs to a different function.", nameof(value)); }

            _NativeFunction = value;
        }
    }

    /// <summary>The primary entry point for the associated function.</summary>
    /// <remarks>
    /// This represnts the lowest level trampoline without exposing C++ ABI concerns to the callee. (IE: This trampoline will not expose the raw this pointer or implicit return by buffer semantics.)
    ///
    /// When creating your own trampolines you generally either want to target this trampoline directly or clone+modify it.
    ///
    /// For functions not involving any special ABI concerns, this may be the same value as <see cref="NativeFunction"/>.
    /// </remarks>
    public Trampoline PrimaryTrampoline
    {
        get => _PrimaryTrampoline;
        init
        {
            if (value.TargetFunctionId != _NativeFunction.TargetFunctionId)
            { throw new ArgumentException("The specified trampoline belongs to a different function.", nameof(value)); }

            _PrimaryTrampoline = value;
        }
    }

    /// <summary>Secondary trampolines associated with this function, usually added by transformations.</summary>
    public ImmutableArray<Trampoline> SecondaryTrampolines
    {
        get => _SecondaryTrampolines;
        init
        {
            foreach (Trampoline trampoline in value)
            {
                if (trampoline.TargetFunctionId != _NativeFunction.TargetFunctionId)
                { throw new ArgumentException("All trampolines in the specified trampoline set must belong to the same function as this collection.", nameof(value)); }

                if (trampoline.IsNativeFunction)
                { throw new ArgumentException("Native trampolines must not appear in the set of secondary trampolines.", nameof(value)); }
            }
        }
    }

    internal TrampolineCollection(Trampoline nativeFunction, Trampoline primaryTrampoline)
    {
        _NativeFunction = nativeFunction;
        _PrimaryTrampoline = primaryTrampoline;
        _SecondaryTrampolines = ImmutableArray<Trampoline>.Empty;
    }

    public TrampolineCollection WithTrampoline(Trampoline trampoline)
    {
        if (trampoline.TargetFunctionId != _NativeFunction.TargetFunctionId)
        { throw new ArgumentException("The specified trampoline must belong to the same function as this collection.", nameof(trampoline)); }

        if (trampoline.IsNativeFunction)
        { throw new ArgumentException("Native trampolines cannot be secondary trampolines.", nameof(trampoline)); }

        return this with
        {
            _SecondaryTrampolines = _SecondaryTrampolines.Add(trampoline)
        };
    }
}