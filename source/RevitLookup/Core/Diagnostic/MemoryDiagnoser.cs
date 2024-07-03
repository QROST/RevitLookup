﻿// Copyright 2003-2024 by Autodesk, Inc.
// 
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
// 
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS.
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC.
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
// 
// Use, duplication, or disclosure by the U.S. Government is subject to
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.

namespace RevitLookup.Core.Diagnostic;

public sealed class MemoryDiagnoser
{
    private long _initialAllocatedBytes;
    private long _finalAllocatedBytes;
    
    public void Start()
    {
        _initialAllocatedBytes = GetTotalAllocatedBytes();
    }
    
    public void Stop()
    {
        _finalAllocatedBytes = GetTotalAllocatedBytes();
    }
    
    public long GetAllocatedBytes()
    {
        var allocatedBytes = _finalAllocatedBytes - _initialAllocatedBytes;
        
        _finalAllocatedBytes = 0;
        _initialAllocatedBytes = 0;
        
        return allocatedBytes;
    }
    
    private static long GetTotalAllocatedBytes()
    {
        // Ref: https://github.com/dotnet/BenchmarkDotNet/blob/master/src/BenchmarkDotNet/Engines/GcStats.cs
        // GC.Collect() slows down the execution a lot, accuracy does not degrade;
        // GC.GetTotalAllocatedBytes() depends heavily on the garbage collection and gives inaccurate results;
        // AppDomain.MonitoringIsEnabled almost does not see memory changes when methods are called.
        // GetAllocatedBytesForCurrentThread is the perfect choice for reflexion calls
        
        return GC.GetAllocatedBytesForCurrentThread();
    }
}