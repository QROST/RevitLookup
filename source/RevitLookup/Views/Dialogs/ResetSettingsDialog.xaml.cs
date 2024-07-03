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

using RevitLookup.Services.Contracts;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace RevitLookup.Views.Dialogs;

public sealed partial class ResetSettingsDialog
{
    private readonly IContentDialogService _dialogService;
    private readonly ISettingsService _settingsService;
    
    public ResetSettingsDialog(IContentDialogService dialogService, ISettingsService settingsService)
    {
        _dialogService = dialogService;
        _settingsService = settingsService;
        InitializeComponent();
    }
    
    public List<ISettings> SelectedSettings
    {
        get
        {
            var checkedSettings = new List<ISettings>();
            if (GeneralBox.IsChecked == true) checkedSettings.Add(_settingsService.GeneralSettings);
            if (RenderBox.IsChecked == true) checkedSettings.Add(_settingsService.RenderSettings);
            
            return checkedSettings;
        }
    }
    
    public async Task<bool> ShowAsync()
    {
        var dialogOptions = new SimpleContentDialogCreateOptions
        {
            Title = "Reset user settings",
            Content = this,
            PrimaryButtonText = "Reset",
            CloseButtonText = "Close",
            DialogMaxHeight = 300,
            DialogMaxWidth = 400
        };
        
        var dialogResult = await _dialogService.ShowSimpleDialogAsync(dialogOptions);
        if (dialogResult != ContentDialogResult.Primary) return false;
        
        return true;
    }
}