﻿using System;
using System.Text;
using TrailSimulation.Game;

namespace TrailSimulation.Core
{
    /// <summary>
    ///     Provides base functionality for rendering out the simulation state via text user interface (TUI). This class has no
    ///     idea about how other modules work and only serves to query them for string data which will be compiled into a
    ///     console only view of the simulation which is intended to be the lowest level of visualization but theoretically
    ///     anything could be a renderer for the simulation.
    /// </summary>
    public sealed class SceneGraph : Module
    {
        /// <summary>
        ///     Fired when the screen back buffer has changed from what is currently being shown, this forces a redraw.
        /// </summary>
        public delegate void ScreenBufferDirty(string tuiContent);

        /// <summary>
        ///     Default string used when game Windows has nothing better to say.
        /// </summary>
        private const string GAMEMODE_DEFAULT_TUI = "[DEFAULT WINDOW TEXT]";

        /// <summary>
        ///     Default string used when there are no game modes at all.
        /// </summary>
        private const string GAMEMODE_EMPTY_TUI = "[NO WINDOW ATTACHED]";

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:TrailSimulation.Core.ModuleProduct" /> class.
        /// </summary>
        public SceneGraph()
        {
            ScreenBuffer = string.Empty;
        }

        /// <summary>
        ///     Holds the last known representation of the game simulation and current Windows text user interface, only pushes
        ///     update
        ///     when a change occurs.
        /// </summary>
        private string ScreenBuffer { get; set; }

        /// <summary>
        ///     Fired when the simulation is closing and needs to clear out any data structures that it created so the program can
        ///     exit cleanly.
        /// </summary>
        public override void Destroy()
        {
            ScreenBuffer = string.Empty;
        }

        /// <summary>
        ///     Called when the simulation is ticked by underlying operating system, game engine, or potato. Each of these system
        ///     ticks is called at unpredictable rates, however if not a system tick that means the simulation has processed enough
        ///     of them to fire off event for fixed interval that is set in the core simulation by constant in milliseconds.
        /// </summary>
        /// <remarks>Default is one second or 1000ms.</remarks>
        /// <param name="systemTick">
        ///     TRUE if ticked unpredictably by underlying operating system, game engine, or potato. FALSE if
        ///     pulsed by game simulation at fixed interval.
        /// </param>
        /// <param name="skipDay">
        ///     Determines if the simulation has force ticked without advancing time or down the trail. Used by
        ///     special events that want to simulate passage of time without actually any actual time moving by.
        /// </param>
        public override void OnTick(bool systemTick, bool skipDay = false)
        {
            // GetModule the current text user interface data from inheriting class.
            var tuiContent = OnRender();
            if (ScreenBuffer.Equals(tuiContent, StringComparison.InvariantCultureIgnoreCase))
                return;

            // Update the screen buffer with altered data.
            ScreenBuffer = tuiContent;
            ScreenBufferDirtyEvent?.Invoke(ScreenBuffer);
        }

        /// <summary>
        ///     Prints game Windows specific text and options.
        /// </summary>
        private string OnRender()
        {
            // Spinning ticker that shows activity, lets us know if application hangs or freezes.
            var tui = new StringBuilder();
            var windowMan = GameSimulationApp.Instance.WindowManager;
            tui.Append($"[ {GameSimulationApp.Instance.TickPhase} ] - ");

            // Keeps track of active Windows name and active Windows current state name for debugging purposes.
            tui.Append(windowMan.FocusedWindow?.CurrentForm != null
                ? $"Window({windowMan.Windows.Count}): {windowMan.FocusedWindow}({windowMan.FocusedWindow.CurrentForm}) - "
                : $"Window({windowMan.Windows.Count}): {windowMan.FocusedWindow}() - ");

            // Total number of turns that have passed in the simulation.
            tui.AppendLine($"Turns: {GameSimulationApp.Instance.TotalTurns.ToString("D4")}");

            // Vehicle and location status.
            tui.AppendLine(
                $"Vehicle: {GameSimulationApp.Instance.Vehicle?.Status} - Location:{GameSimulationApp.Instance.Trail?.CurrentLocation?.Status}");

            // Prints game Windows specific text and options. This typically is menus from commands, or states showing some information.
            tui.Append($"{RenderMode(windowMan)}{Environment.NewLine}");

            if (GameSimulationApp.Instance.WindowManager.AcceptingInput)
            {
                // Allow user to see their input from buffer.
                tui.Append($"What is your choice? {GameSimulationApp.Instance.InputManager.InputBuffer}");
            }

            // Outputs the result of the string builder to TUI builder above.
            return tui.ToString();
        }

        /// <summary>
        ///     Prints game Windows specific text and options.
        /// </summary>
        /// <param name="windowManager">
        ///     Instance of the window manager so we don't have to get it ourselves and just use the same one
        ///     renderer is using.
        /// </param>
        private string RenderMode(WindowManager windowManager)
        {
            // If TUI for active game Windows is not null or empty then use it.
            var activeModeTUI = windowManager.FocusedWindow?.OnRenderMode();
            if (!string.IsNullOrEmpty(activeModeTUI))
                return activeModeTUI;

            // Otherwise, display default message if null for Windows.
            return windowManager.FocusedWindow == null ? GAMEMODE_EMPTY_TUI : GAMEMODE_DEFAULT_TUI;
        }

        /// <summary>
        ///     Fired when the screen back buffer has changed from what is currently being shown, this forces a redraw.
        /// </summary>
        public event ScreenBufferDirty ScreenBufferDirtyEvent;
    }
}