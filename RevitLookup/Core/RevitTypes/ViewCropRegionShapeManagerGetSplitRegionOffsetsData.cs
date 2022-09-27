﻿using Autodesk.Revit.DB;
using RevitLookup.Views;
using Form = System.Windows.Forms.Form;

namespace RevitLookup.Core.RevitTypes;

public class ViewCropRegionShapeManagerGetSplitRegionOffsetsData : Data
{
    private readonly ViewCropRegionShapeManager _viewCropRegionShapeManager;

    public ViewCropRegionShapeManagerGetSplitRegionOffsetsData(string label, ViewCropRegionShapeManager viewCropRegionShapeManager) : base(label)
    {
        _viewCropRegionShapeManager = viewCropRegionShapeManager;
    }

    public override bool HasDrillDown => _viewCropRegionShapeManager is {NumberOfSplitRegions: > 1};

    public override string AsValueString()
    {
        return "< Split Region Offsets >";
    }

    public override Form DrillDown()
    {
        if (!HasDrillDown) return null;

        var cropRegionOffsetObjects = new List<SnoopableWrapper>();

        for (var i = 0; i < _viewCropRegionShapeManager.NumberOfSplitRegions; i++)
            cropRegionOffsetObjects.Add(new SnoopableWrapper($"[{i}]", _viewCropRegionShapeManager.GetSplitRegionOffset(i)));

        if (cropRegionOffsetObjects.Count == 0) return null;

        var form = new ObjectsView(cropRegionOffsetObjects);
        return form;
    }
}