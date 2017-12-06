using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PeanutButter.TrayIcon
{
  /// <summary>
  /// Interface of the tray icon utility. Substitute for testing (:
  /// </summary>
  public interface ITrayIcon: IDisposable
  {
    /// <summary>
    /// The win32 forms notify icon exposed for any functionality not already encapsulated in this utility
    /// </summary>
    NotifyIcon NotifyIcon { get; }
    /// <summary>
    /// Default timeout, in milliseconds, for balloon messages
    /// </summary>
    int DefaultBalloonTipTimeout { get; set; }
    /// <summary>
    /// The
    /// </summary>
    Icon Icon { get; set; }
    /// <summary>
    /// Default tooltip text
    /// </summary>
    string DefaultTipText { get; set; }
    /// <summary>
    /// Default tooltip title
    /// </summary>
    string DefaultTipTitle { get; set; }
    /// <summary>
    /// Default action to run when clicking the balloon tip
    /// </summary>
    Action DefaultBalloonTipClickedAction { get; set; }
    /// <summary>
    /// Default action to run when closing the ballon tip
    /// </summary>
    Action DefaultBalloonTipClosedAction { get; set; }

    /// <summary>
    /// Shows a balloon tip
    /// </summary>
    /// <param name="timeoutInMilliseconds">How long to show for</param>
    /// <param name="title">Title to show on the balloon</param>
    /// <param name="text">Text to show in the balloon</param>
    /// <param name="icon">Icon to display on the balloon</param>
    /// <param name="clickAction">Action to run when the user clicks the balloon</param>
    /// <param name="closeAction">Action to run when user closes the balloon</param>
    void ShowBalloonTipFor(int timeoutInMilliseconds, string title, string text, ToolTipIcon icon,
      Action clickAction = null, Action closeAction = null);

    /// <summary>
    /// Adds a submenu to an existing menu or to the root menu if no parent provided
    /// </summary>
    /// <param name="text">Text to display for the submenu</param>
    /// <param name="parent">Parent menu (optional: defaults to the root menu)</param>
    /// <returns></returns>
    MenuItem AddSubMenu(string text, MenuItem parent = null);
    /// <summary>
    /// Adds an item to a menu
    /// </summary>
    /// <param name="withText">Text to display on the menu item</param>
    /// <param name="withCallback">Action to run when the user clicks the menu item</param>
    /// <param name="parent">Parent menu (optional, defaults to the root menu)</param>
    void AddMenuItem(string withText, Action withCallback, MenuItem parent = null);
    /// <summary>
    /// Adds a menu separator
    /// </summary>
    /// <param name="subMenu">Menu to add to (optional, defaults to the root menu)</param>
    void AddMenuSeparator(MenuItem subMenu = null);
    /// <summary>
    /// Removes a menu item by its label
    /// </summary>
    /// <param name="withText">Label of the item to remove</param>
    void RemoveMenuItem(string withText);
    /// <summary>
    /// Add a mouse click handler for the icon
    /// </summary>
    /// <param name="clicks">Single or double</param>
    /// <param name="button">Which button to capture for</param>
    /// <param name="handler">Callback to invoke</param>
    /// <returns>A handler object which can be used to de-register the handler</returns>
    MouseClickHandler AddMouseClickHandler(MouseClicks clicks, MouseButtons button, Action handler);
    /// <summary>
    /// Remove the provided handler
    /// </summary>
    /// <param name="handler">Handler obtained through a prior AddMouseClickHandler call</param>
    void RemoveMouseClickHandler(MouseClickHandler handler);

    /// <summary>
    /// Initializes the Tray Icon with an Icon
    /// </summary>
    /// <param name="icon">Icon to display</param>
    void Init(Icon icon);
    /// <summary>
    /// Initializes the Tray Icon from a stream providing icon data
    /// </summary>
    /// <param name="iconStream">Stream providing icon data (ico)</param>
    void Init(Stream iconStream);
    /// <summary>
    /// Initializes the Tray Icon with a path to an icon (ico or png)
    /// </summary>
    /// <param name="pathToIcon">Path to the icon to use</param>
    void Init(string pathToIcon);
    /// <summary>
    /// Initialize with a Bitmap structure for the icon
    /// </summary>
    /// <param name="bitmap">Bitmap to use for the icon</param>
    void Init(Bitmap bitmap);
    /// <summary>
    /// Shows the Tray Icon if not visible
    /// </summary>
    void Show();
    /// <summary>
    /// Hides the Tray Icon if visible
    /// </summary>
    void Hide();
  }
}