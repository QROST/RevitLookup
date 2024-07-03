// Copyright 2003-2024 by Autodesk, Inc.
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

using System.Reflection;
using RevitLookup.Core.Contracts;
using RevitLookup.Core.Objects;

namespace RevitLookup.Core.ComponentModel.Descriptors;

public sealed class TableDataDescriptor(TableData tableData) : Descriptor, IDescriptorResolver
{
    public Func<IVariants> Resolve(Document context, string target, ParameterInfo[] parameters)
    {
        return target switch
        {
            nameof(TableData.GetSectionData) when parameters.Length == 1 && 
                                                  parameters[0].ParameterType == typeof(SectionType) => ResolveSectionDataBySectionType,
            nameof(TableData.GetSectionData) when parameters.Length == 1 && 
                                                  parameters[0].ParameterType == typeof(int) => ResolveSectionDataByIndex,
            nameof(TableData.IsValidZoomLevel) => ResolveZoomLevel,
            _ => null
        };
        
        IVariants ResolveSectionDataBySectionType()
        {
            var sectionTypes = Enum.GetValues(typeof(SectionType));
            var variants = new Variants<TableSectionData>(sectionTypes.Length);
            foreach (SectionType sectionType in sectionTypes)
            {
                variants.Add(tableData.GetSectionData(sectionType), sectionType.ToString());
            }
            
            return variants;
        }    
        
        IVariants ResolveSectionDataByIndex()
        {
            var variants = new Variants<TableSectionData>(tableData.NumberOfSections);
            for (var i = 0; i < tableData.NumberOfSections; i++)
            {
                variants.Add(tableData.GetSectionData(i), i.ToString());
            }
            
            return variants;
        }
        
        IVariants ResolveZoomLevel()
        {
            var variants = new Variants<bool>(512);
            
            var zoom = 0;
            var emptyIterations = 0;
            while (emptyIterations < 50)
            {
                var isValid = tableData.IsValidZoomLevel(zoom);
                if (isValid)
                {
                    variants.Add(true, $"{zoom}: valid");
                    emptyIterations = 0;
                }
                else
                {
                    emptyIterations++;
                }
                
                zoom++;
            }
            
            return variants;
        }
    }
}