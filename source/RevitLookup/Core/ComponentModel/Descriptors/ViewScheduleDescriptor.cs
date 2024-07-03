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

public sealed class ViewScheduleDescriptor(ViewSchedule viewSchedule) : ElementDescriptor(viewSchedule)
{
    public override Func<IVariants> Resolve(Document context, string target, ParameterInfo[] parameters)
    {
        return target switch
        {
            nameof(ViewSchedule.GetStripedRowsColor) => ResolveStripedRowsColor,
            nameof(ViewSchedule.IsValidTextTypeId) => ResolveValidTextTypeId,
            nameof(ViewSchedule.GetDefaultNameForKeySchedule) => ResolveDefaultNameForKeySchedule,
            nameof(ViewSchedule.GetDefaultNameForMaterialTakeoff) => ResolveDefaultNameForMaterialTakeoff,
            nameof(ViewSchedule.GetDefaultNameForSchedule) => ResolveDefaultNameForSchedule,
            nameof(ViewSchedule.GetDefaultParameterNameForKeySchedule) => ResolveDefaultParameterNameForKeySchedule,
            nameof(ViewSchedule.IsValidCategoryForKeySchedule) => ResolveIsValidCategoryForKeySchedule,
            nameof(ViewSchedule.IsValidCategoryForMaterialTakeoff) => ResolveIsValidCategoryForMaterialTakeoff,
            nameof(ViewSchedule.IsValidCategoryForSchedule) => ResolveIsValidCategoryForSchedule,
            nameof(ViewSchedule.GetDefaultNameForKeynoteLegend) => ResolveGetDefaultNameForKeynoteLegend,
            nameof(ViewSchedule.GetDefaultNameForNoteBlock) => ResolveGetDefaultNameForNoteBlock,
            nameof(ViewSchedule.GetDefaultNameForRevisionSchedule) => ResolveGetDefaultNameForRevisionSchedule,
            nameof(ViewSchedule.GetDefaultNameForSheetList) => ResolveGetDefaultNameForSheetList,
            nameof(ViewSchedule.GetDefaultNameForViewList) => ResolveGetDefaultNameForViewList,
            nameof(ViewSchedule.GetValidFamiliesForNoteBlock) => ResolveGetValidFamiliesForNoteBlock,
            nameof(ViewSchedule.RefreshData) => Variants.Disabled,
#if REVIT2022_OR_GREATER
            nameof(ViewSchedule.GetScheduleInstances) => ResolveScheduleInstances,
            nameof(ViewSchedule.GetSegmentHeight) => ResolveSegmentHeight,
#endif
            _ => null
        };
        
        IVariants ResolveStripedRowsColor()
        {
            var patterns = Enum.GetValues(typeof(StripedRowPattern));
            var variants = new Variants<Color>(patterns.Length);
            
            foreach (StripedRowPattern pattern in patterns)
            {
                variants.Add(viewSchedule.GetStripedRowsColor(pattern), pattern.ToString());
            }
            
            return variants;
        }
        
        IVariants ResolveValidTextTypeId()
        {
            var types = context.EnumerateTypes<TextNoteType>().ToArray();
            var variants = new Variants<bool>(types.Length);
            
            foreach (var type in types)
            {
                var result = viewSchedule.IsValidTextTypeId(type.Id);
                variants.Add(result, $"{type.Name}: {result}");
            }
            
            return variants;
        }
        
        IVariants ResolveDefaultNameForKeySchedule()
        {
            var categories = ViewSchedule.GetValidCategoriesForKeySchedule();
            var variants = new Variants<string>(categories.Count);
            foreach (var categoryId in categories)
            {
                variants.Add(ViewSchedule.GetDefaultNameForKeySchedule(context, categoryId));
            }
            
            return variants;
        }
        
        IVariants ResolveDefaultNameForMaterialTakeoff()
        {
            var categories = ViewSchedule.GetValidCategoriesForMaterialTakeoff();
            var variants = new Variants<string>(categories.Count);
            foreach (var categoryId in categories)
            {
                variants.Add(ViewSchedule.GetDefaultNameForMaterialTakeoff(context, categoryId));
            }
            
            return variants;
        }
        
        IVariants ResolveDefaultNameForSchedule()
        {
            var categories = ViewSchedule.GetValidCategoriesForSchedule();
            var areas = context.EnumerateInstances<AreaScheme>().ToArray();
            var variants = new Variants<string>(categories.Count + areas.Length);
            var areaId = new ElementId(BuiltInCategory.OST_Areas);
            foreach (var categoryId in categories)
            {
                if (categoryId == areaId)
                {
                    foreach (var area in areas)
                    {
                        variants.Add(ViewSchedule.GetDefaultNameForSchedule(context, categoryId, area.Id));
                    }
                }
                else
                {
                    variants.Add(ViewSchedule.GetDefaultNameForSchedule(context, categoryId));
                }
            }
            
            return variants;
        }
        
        IVariants ResolveDefaultParameterNameForKeySchedule()
        {
            var categories = ViewSchedule.GetValidCategoriesForKeySchedule();
            var variants = new Variants<string>(categories.Count);
            var areaId = new ElementId(BuiltInCategory.OST_Areas);
            foreach (var categoryId in categories)
            {
                if (categoryId == areaId) continue;
                variants.Add(ViewSchedule.GetDefaultParameterNameForKeySchedule(context, categoryId));
            }
            
            return variants;
        }
        
        IVariants ResolveIsValidCategoryForKeySchedule()
        {
            var categories = context.Settings.Categories;
            var variants = new Variants<bool>(categories.Size);
            foreach (Category category in categories)
            {
                var result = ViewSchedule.IsValidCategoryForKeySchedule(category.Id);
                variants.Add(result, $"{category.Name}: {result}");
            }
            
            return variants;
        }
        
        IVariants ResolveIsValidCategoryForMaterialTakeoff()
        {
            var categories = context.Settings.Categories;
            var variants = new Variants<bool>(categories.Size);
            foreach (Category category in categories)
            {
                var result = ViewSchedule.IsValidCategoryForMaterialTakeoff(category.Id);
                variants.Add(result, $"{category.Name}: {result}");
            }
            
            return variants;
        }
        
        IVariants ResolveIsValidCategoryForSchedule()
        {
            var categories = context.Settings.Categories;
            var variants = new Variants<bool>(categories.Size);
            foreach (Category category in categories)
            {
                var result = ViewSchedule.IsValidCategoryForSchedule(category.Id);
                variants.Add(result, $"{category.Name}: {result}");
            }
            
            return variants;
        }
        
#if REVIT2022_OR_GREATER
        IVariants ResolveScheduleInstances()
        {
            var count = viewSchedule.GetSegmentCount();
            var variants = new Variants<IList<ElementId>>(count);
            
            for (var i = -1; i < count; i++)
            {
                variants.Add(viewSchedule.GetScheduleInstances(i));
            }
            
            return variants;
        }
        
        IVariants ResolveSegmentHeight()
        {
            var count = viewSchedule.GetSegmentCount();
            var variants = new Variants<double>(count);
            
            for (var i = 0; i < count; i++)
            {
                variants.Add(viewSchedule.GetSegmentHeight(i));
            }
            
            return variants;
        }
#endif
        IVariants ResolveGetDefaultNameForKeynoteLegend()
        {
            return Variants.Single(ViewSchedule.GetDefaultNameForKeynoteLegend(context));
        }
        
        IVariants ResolveGetDefaultNameForNoteBlock()
        {
            return Variants.Single(ViewSchedule.GetDefaultNameForNoteBlock(context));
        }
        
        IVariants ResolveGetDefaultNameForRevisionSchedule()
        {
            return Variants.Single(ViewSchedule.GetDefaultNameForRevisionSchedule(context));
        }
        
        IVariants ResolveGetDefaultNameForSheetList()
        {
            return Variants.Single(ViewSchedule.GetDefaultNameForSheetList(context));
        }
        
        IVariants ResolveGetDefaultNameForViewList()
        {
            return Variants.Single(ViewSchedule.GetDefaultNameForViewList(context));
        }
        
        IVariants ResolveGetValidFamiliesForNoteBlock()
        {
            return Variants.Single(ViewSchedule.GetValidFamiliesForNoteBlock(context));
        }
    }
    
    public override void RegisterExtensions(IExtensionManager manager)
    {
    }
}