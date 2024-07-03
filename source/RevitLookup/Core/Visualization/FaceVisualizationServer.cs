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

using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.ExternalService;
using RevitLookup.Core.Visualization.Helpers;
using RevitLookup.Models.Render;

namespace RevitLookup.Core.Visualization;

public sealed class FaceVisualizationServer : IDirectContext3DServer
{
    private Face _face;
    private bool _hasEffectsUpdates = true;
    private bool _hasGeometryUpdates = true;

    private readonly Guid _guid = Guid.NewGuid();
    private readonly object _renderLock = new();
    private readonly RenderingBufferStorage _meshGridBuffer = new();
    private readonly RenderingBufferStorage _normalBuffer = new();
    private readonly RenderingBufferStorage _surfaceBuffer = new();

    private double _extrusion;
    private double _transparency;

    private Color _meshColor;
    private Color _normalColor;
    private Color _surfaceColor;

    private bool _drawMeshGrid;
    private bool _drawNormalVector;
    private bool _drawSurface;

    public Guid GetServerId() => _guid;
    public string GetVendorId() => "RevitLookup";
    public string GetName() => "Face visualization server";
    public string GetDescription() => "Face geometry visualization";
    public ExternalServiceId GetServiceId() => ExternalServices.BuiltInExternalServices.DirectContext3DService;
    public string GetApplicationId() => string.Empty;
    public string GetSourceId() => string.Empty;
    public bool UsesHandles() => false;
    public bool CanExecute(View view) => true;
    public bool UseInTransparentPass(View view) => _drawSurface && _transparency > 0;

    public Outline GetBoundingBox(View view)
    {
        if (_face.Reference is null) return null;

        var element = _face.Reference.ElementId.ToElement(view.Document)!;
        var boundingBox = element.get_BoundingBox(null) ?? element.get_BoundingBox(view);
        if (boundingBox is null) return null;

        var minPoint = boundingBox.Transform.OfPoint(boundingBox.Min);
        var maxPoint = boundingBox.Transform.OfPoint(boundingBox.Max);

        return new Outline(minPoint, maxPoint);
    }

    public void RenderScene(View view, DisplayStyle displayStyle)
    {
        lock (_renderLock)
        {
            try
            {
                if (_hasGeometryUpdates || !_surfaceBuffer.IsValid() || !_meshGridBuffer.IsValid() || !_normalBuffer.IsValid())
                {
                    MapGeometryBuffer();
                    _hasGeometryUpdates = false;
                }

                if (_hasEffectsUpdates)
                {
                    UpdateEffects();
                    _hasEffectsUpdates = false;
                }

                if (_drawSurface)
                {
                    var isTransparentPass = DrawContext.IsTransparentPass();
                    if (isTransparentPass && _transparency > 0 || !isTransparentPass && _transparency == 0)
                    {
                        DrawContext.FlushBuffer(_surfaceBuffer.VertexBuffer,
                            _surfaceBuffer.VertexBufferCount,
                            _surfaceBuffer.IndexBuffer,
                            _surfaceBuffer.IndexBufferCount,
                            _surfaceBuffer.VertexFormat,
                            _surfaceBuffer.EffectInstance, PrimitiveType.TriangleList, 0,
                            _surfaceBuffer.PrimitiveCount);
                    }
                }

                if (_drawMeshGrid)
                {
                    DrawContext.FlushBuffer(_meshGridBuffer.VertexBuffer,
                        _meshGridBuffer.VertexBufferCount,
                        _meshGridBuffer.IndexBuffer,
                        _meshGridBuffer.IndexBufferCount,
                        _meshGridBuffer.VertexFormat,
                        _meshGridBuffer.EffectInstance, PrimitiveType.LineList, 0,
                        _meshGridBuffer.PrimitiveCount);
                }

                if (_drawNormalVector)
                {
                    DrawContext.FlushBuffer(_normalBuffer.VertexBuffer,
                        _normalBuffer.VertexBufferCount,
                        _normalBuffer.IndexBuffer,
                        _normalBuffer.IndexBufferCount,
                        _normalBuffer.VertexFormat,
                        _normalBuffer.EffectInstance, PrimitiveType.LineList, 0,
                        _normalBuffer.PrimitiveCount);
                }
            }
            catch (Exception exception)
            {
                RenderFailed?.Invoke(this, new RenderFailedEventArgs
                {
                    Exception = exception
                });
            }
        }
    }

    private void MapGeometryBuffer()
    {
        var mesh = _face.Triangulate();
        var faceBox = _face.GetBoundingBox();
        var center = (faceBox.Min + faceBox.Max) / 2;
        var normal = _face.ComputeNormal(center);
        var offset = RenderGeometryHelper.InterpolateOffsetByArea(_face.Area);
        var normalLength = RenderGeometryHelper.InterpolateAxisLengthByArea(_face.Area);

        RenderHelper.MapSurfaceBuffer(_surfaceBuffer, mesh, _extrusion);
        RenderHelper.MapMeshGridBuffer(_meshGridBuffer, mesh, _extrusion);
        RenderHelper.MapNormalVectorBuffer(_normalBuffer, _face.Evaluate(center) + normal * (offset + _extrusion), normal, normalLength);
    }

    private void UpdateEffects()
    {
        _surfaceBuffer.EffectInstance ??= new EffectInstance(_surfaceBuffer.FormatBits);
        _meshGridBuffer.EffectInstance ??= new EffectInstance(_surfaceBuffer.FormatBits);
        _normalBuffer.EffectInstance ??= new EffectInstance(_surfaceBuffer.FormatBits);

        _surfaceBuffer.EffectInstance.SetColor(_surfaceColor);
        _meshGridBuffer.EffectInstance.SetColor(_meshColor);
        _normalBuffer.EffectInstance.SetColor(_normalColor);
        _surfaceBuffer.EffectInstance.SetTransparency(_transparency);
    }

    public void UpdateSurfaceColor(Color value)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _surfaceColor = value;
            _hasEffectsUpdates = true;

            uiDocument.UpdateAllOpenViews();
        }
    }

    public void UpdateMeshGridColor(Color value)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _meshColor = value;
            _hasEffectsUpdates = true;

            uiDocument.UpdateAllOpenViews();
        }
    }

    public void UpdateNormalVectorColor(Color value)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _normalColor = value;
            _hasEffectsUpdates = true;

            uiDocument.UpdateAllOpenViews();
        }
    }

    public void UpdateExtrusion(double value)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _extrusion = value;
            _hasGeometryUpdates = true;

            uiDocument.UpdateAllOpenViews();
        }
    }

    public void UpdateTransparency(double value)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _transparency = value;
            _hasEffectsUpdates = true;

            uiDocument.UpdateAllOpenViews();
        }
    }

    public void UpdateSurfaceVisibility(bool visible)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _drawSurface = visible;
            uiDocument.UpdateAllOpenViews();
        }
    }

    public void UpdateMeshGridVisibility(bool visible)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _drawMeshGrid = visible;
            uiDocument.UpdateAllOpenViews();
        }
    }

    public void UpdateNormalVectorVisibility(bool visible)
    {
        var uiDocument = Context.UiDocument;
        if (uiDocument is null) return;

        lock (_renderLock)
        {
            _drawNormalVector = visible;
            uiDocument.UpdateAllOpenViews();
        }
    }

    public void Register(Face face)
    {
        _face = face;

        Application.ActionEventHandler.Raise(application =>
        {
            if (application.ActiveUIDocument is null) return;

            var directContextService = (MultiServerService) ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            var serverIds = directContextService.GetActiveServerIds();

            directContextService.AddServer(this);
            serverIds.Add(GetServerId());
            directContextService.SetActiveServers(serverIds);

            application.ActiveUIDocument.UpdateAllOpenViews();
        });
    }

    public void Unregister()
    {
        Application.ActionEventHandler.Raise(application =>
        {
            var directContextService = (MultiServerService) ExternalServiceRegistry.GetService(ExternalServices.BuiltInExternalServices.DirectContext3DService);
            directContextService.RemoveServer(GetServerId());

            application.ActiveUIDocument?.UpdateAllOpenViews();
        });
    }

    public event EventHandler<RenderFailedEventArgs> RenderFailed;
}