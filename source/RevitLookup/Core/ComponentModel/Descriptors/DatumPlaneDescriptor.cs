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

public sealed class DatumPlaneDescriptor(DatumPlane datumPlane) : ElementDescriptor(datumPlane)
{
    public override Func<IVariants> Resolve(Document context, string target, ParameterInfo[] parameters)
    {
        return target switch
        {
#if REVIT2025_OR_GREATER //TODO Fatal https://github.com/jeremytammik/RevitLookup/issues/225
            nameof(DatumPlane.CanBeVisibleInView) => Variants.Disabled,
            nameof(DatumPlane.GetPropagationViews) => Variants.Disabled,
#else
            nameof(DatumPlane.CanBeVisibleInView) => ResolveCanBeVisibleInView,
            nameof(DatumPlane.GetPropagationViews) => ResolvePropagationViews,
#endif
            nameof(DatumPlane.GetDatumExtentTypeInView) => ResolveDatumExtentTypeInView,
            nameof(DatumPlane.HasBubbleInView) => ResolveHasBubbleInView,
            nameof(DatumPlane.IsBubbleVisibleInView) => ResolveBubbleVisibleInView,
            nameof(DatumPlane.GetCurvesInView) => ResolveGetCurvesInView,
            nameof(DatumPlane.GetLeader) => ResolveGetLeader,
            _ => null
        };
#if !REVIT2025_OR_GREATER
        
        IVariants ResolveCanBeVisibleInView()
        {
            var views = context.EnumerateInstances<View>().ToArray();
            var variants = new Variants<bool>(views.Length);
            
            foreach (var view in views)
            {
                var result = datumPlane.CanBeVisibleInView(view);
                variants.Add(result, $"{view.Name}: {result}");
            }
            
            return variants;
        }
        
        IVariants ResolvePropagationViews()
        {
            var views = context.EnumerateInstances<View>().ToArray();
            var variants = new Variants<ISet<ElementId>>(views.Length);
            
            foreach (var view in views)
            {
                if (!datumPlane.CanBeVisibleInView(view)) continue;
                
                var result = datumPlane.GetPropagationViews(view);
                variants.Add(result, view.Name);
            }
            
            return variants;
        }
#endif
        
        IVariants ResolveDatumExtentTypeInView()
        {
            var variants = new Variants<DatumExtentType>(2);
            
            var resultEnd0 = datumPlane.GetDatumExtentTypeInView(DatumEnds.End0, context.ActiveView);
            var resultEnd1 = datumPlane.GetDatumExtentTypeInView(DatumEnds.End1, context.ActiveView);
            variants.Add(resultEnd0, $"End 0, Active view: {resultEnd0}");
            variants.Add(resultEnd1, $"End 1, Active view: {resultEnd1}");
            
            return variants;
        }
        
        IVariants ResolveHasBubbleInView()
        {
            var variants = new Variants<bool>(2);
            
            var resultEnd0 = datumPlane.HasBubbleInView(DatumEnds.End0, context.ActiveView);
            var resultEnd1 = datumPlane.HasBubbleInView(DatumEnds.End1, context.ActiveView);
            variants.Add(resultEnd0, $"End 0, Active view: {resultEnd0}");
            variants.Add(resultEnd1, $"End 1, Active view: {resultEnd1}");
            
            return variants;
        }
        
        IVariants ResolveBubbleVisibleInView()
        {
            var variants = new Variants<bool>(2);
            
            var resultEnd0 = datumPlane.IsBubbleVisibleInView(DatumEnds.End0, context.ActiveView);
            var resultEnd1 = datumPlane.IsBubbleVisibleInView(DatumEnds.End1, context.ActiveView);
            variants.Add(resultEnd0, $"End 0, Active view: {resultEnd0}");
            variants.Add(resultEnd1, $"End 1, Active view: {resultEnd1}");
            
            return variants;
        }
        
        IVariants ResolveGetCurvesInView()
        {
            return new Variants<IList<Curve>>(2)
                .Add(datumPlane.GetCurvesInView(DatumExtentType.Model, context.ActiveView), "Model, Active view")
                .Add(datumPlane.GetCurvesInView(DatumExtentType.ViewSpecific, context.ActiveView), "ViewSpecific, Active view");
        }
        
        IVariants ResolveGetLeader()
        {
            return new Variants<Leader>(2)
                .Add(datumPlane.GetLeader(DatumEnds.End0, context.ActiveView), "End 0, Active view")
                .Add(datumPlane.GetLeader(DatumEnds.End1, context.ActiveView), "End 1, Active view");
        }
    }
    
    public override void RegisterExtensions(IExtensionManager manager)
    {
    }
}