// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Leszek Pomianowski and WPF UI Contributors.
// All Rights Reserved.

using System.Windows.Controls;

namespace Wpf.Ui;

/// <summary>
/// Set of properties used when creating a new simple content dialog.
/// </summary>
public class SimpleContentDialogCreateOptions
{
    /// <summary>
    /// Gets or sets a name at the top of the content dialog.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets a message displayed in the content dialog.
    /// </summary>
    public required object Content { get; set; }

    /// <summary>
    /// Gets or sets the name of the button that closes the content dialog.
    /// </summary>
    public required string CloseButtonText { get; set; }

    /// <summary>
    /// Gets or sets the default text of the primary button at the bottom of the content dialog.
    /// <para>If not added, or <see cref="String.Empty"/>, it will not be displayed.</para>
    /// </summary>
    public string PrimaryButtonText { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets the default text of the secondary button at the bottom of the content dialog.
    /// <para>If not added, or <see cref="String.Empty"/>, it will not be displayed.</para>
    /// </summary>
    public string SecondaryButtonText { get; set; } = String.Empty;

    /// <summary>
    /// Gets or sets a max content dialog width.
    /// </summary>
    public double DialogMaxWidth { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets or sets a max content dialog height.
    /// </summary>
    public double DialogMaxHeight { get; set; } = double.MaxValue;

    /// <summary>
    /// Gets or sets a dialog Horizontal Alignment.
    /// </summary>
    /// <returns>HorizontalAlignment.Stretch by default</returns>
    public HorizontalAlignment DialogHorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;

    /// <summary>
    /// Gets or sets a dialog Vertical Alignment.
    /// </summary>
    /// <returns>VerticalAlignment.Stretch by default</returns>
    public VerticalAlignment DialogVerticalAlignment { get; set; } = VerticalAlignment.Stretch;

    /// <summary>
    /// Gets or sets a dialog Horizontal Scroll Visibility.
    /// </summary>
    /// <returns>ScrollBarVisibility.Hidden by default</returns>
    public ScrollBarVisibility HorizontalScrollVisibility { get; set; } = ScrollBarVisibility.Hidden;

    /// <summary>
    /// Gets or sets a dialog Vertical Scroll Visibility.
    /// </summary>
    /// <returns>ScrollBarVisibility.Hidden by default</returns>
    public ScrollBarVisibility VerticalScrollVisibility { get; set; } = ScrollBarVisibility.Hidden;
}