﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace TrailEntities.Simulation
{
    /// <summary>
    ///     Builds up a list of game modes and their states using reflection and attributes. Contains methods to add game modes
    ///     to running simulation. Can also remove modes and modify them further with states.
    /// </summary>
    public sealed class WindowManager : SimulationModule
    {
        /// <summary>
        ///     Fired when the window manager has added or removed a game mode.
        /// </summary>
        /// <param name="modeType">Game mode that is going to be the new active one.</param>
        public delegate void ModeChanged(ModeType modeType);

        /// <summary>
        ///     Factory pattern that will create game modes for it based on attribute at the top of each one that defines what mode
        ///     category it is responsible for.
        /// </summary>
        private ModeFactory _modeFactory;

        /// <summary>
        ///     Keeps track of all the possible states a given game mode can have by using attributes and reflection to keep track
        ///     of which user data object gets mapped to which particular state.
        /// </summary>
        private StateFactory _stateFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:TrailEntities.Simulation.WindowManager" /> class.
        /// </summary>
        internal WindowManager()
        {
            // References all of the active game modes that need to be ticked.
            Modes = new Dictionary<ModeType, IModeProduct>();
            _modeFactory = new ModeFactory();

            // References to states that can be attached to a particular game mode via attribute association.
            _stateFactory = new StateFactory();
        }

        /// <summary>
        ///     Statistics for mode runtime. Keeps track of how many times a given mode type was attached to the simulation for
        ///     record keeping purposes.
        /// </summary>
        public Dictionary<ModeType, int> RunCount
        {
            get { return _modeFactory.RunCount; }
        }

        /// <summary>
        ///     References the current active game mode, or the last attached game mode in the simulation.
        /// </summary>
        public IModeProduct ActiveMode
        {
            get { return Modes.LastOrDefault().Value; }
        }

        /// <summary>
        ///     Returns the total number of active game modes that are currently loaded into the simulation.
        /// </summary>
        public int ModeCount
        {
            get { return Modes.Count; }
        }

        /// <summary>
        ///     Current list of all game modes, only the last one added gets ticked this is so game modes can attach things on-top
        ///     of themselves like stores and trades.
        /// </summary>
        internal Dictionary<ModeType, IModeProduct> Modes { get; }

        /// <summary>
        ///     Determines if this simulation is currently accepting input at all, the conditions for this require some game mode
        ///     to be attached and or active move to not be null.
        /// </summary>
        internal bool AcceptingInput
        {
            get
            {
                // Skip if there is no active modes.
                if (ActiveMode == null)
                    return false;

                // Skip if mode doesn't want input and has no state.
                if (!ActiveMode.AcceptsInput && ActiveMode.CurrentState == null)
                    return false;

                // Skip if mode state doesn't want input and current state is not null.
                if ((ActiveMode.CurrentState != null && !ActiveMode.AcceptsInput))
                    return false;

                // Skip if state is not null and, game mode accepts input, but current state doesn't want input.
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (ActiveMode.CurrentState != null && ActiveMode.AcceptsInput && !ActiveMode.CurrentState.AcceptsInput)
                    return false;

                // Default response is to return true if nothing else stops it above.
                return true;
            }
        }

        /// <summary>
        ///     Creates and adds the specified type of state to currently active game mode.
        /// </summary>
        public IStateProduct CreateStateFromType(Type stateType)
        {
            return _stateFactory.CreateStateFromType(stateType, ActiveMode.ModeType);
        }

        /// <summary>
        ///     Removes any and all inactive game modes that need to be removed from the simulation.
        /// </summary>
        private void RemoveDirtyModes()
        {
            // Ensure the mode exists as active mode.
            if (ActiveMode == null)
                throw new InvalidOperationException("Attempted to remove active mode when it is null!");

            // Create copy of all modes so we can destroy while iterating.
            var copyModes = new Dictionary<ModeType, IModeProduct>(Modes);
            foreach (var mode in copyModes)
            {
                // Skip if the mode doesn't want to be removed.
                if (!mode.Value.ShouldRemoveMode)
                    continue;

                // Remove the mode from list if it is flagged for removal.
                Modes.Remove(mode.Key);

                // Fire virtual method which will allow game simulation above and attempt to pass this data along to internal game mode and game mode states.
                if (ActiveMode != null)
                    OnModeChanged(ActiveMode.ModeType);
            }
            copyModes.Clear();
        }

        /// <summary>
        ///     Creates and adds the specified game mode to the simulation if it does not already exist in the list of modes.
        /// </summary>
        /// <param name="modeType">Enumeration value of the mode which should be created.</param>
        public void AddMode(ModeType modeType)
        {
            // Check if any other modes match the one we are adding.
            if (Modes.ContainsKey(modeType))
                return;

            // Add the game mode to the simulation now that we know it does not exist in the stack yet.
            Modes.Add(modeType, _modeFactory.CreateMode(modeType));
            ModeChangedEvent?.Invoke(Modes[modeType].ModeType);
        }

        /// <summary>
        ///     Fired when the active game mode has been changed, this allows any underlying mode to know about a change in
        ///     simulation.
        /// </summary>
        /// <param name="modeType">Current mode which the simulation is changing to.</param>
        private void OnModeChanged(ModeType modeType)
        {
            // Fire event that lets subscribers know we changed something.
            ModeChangedEvent?.Invoke(modeType);
        }

        /// <summary>
        ///     Fired when the window manager has added or removed a game mode.
        /// </summary>
        public event ModeChanged ModeChangedEvent;

        /// <summary>
        ///     Fired when the simulation is closing and needs to clear out any data structures that it created so the program can
        ///     exit cleanly.
        /// </summary>
        public override void Destroy()
        {
            // Mode factory and list of modes in simulation.
            _modeFactory.Destroy();
            _modeFactory = null;
            Modes.Clear();

            // State factory only references parent mode type, they are added directly to active mode so no list of them here.
            _stateFactory.Destroy();
            _stateFactory = null;
        }

        /// <summary>
        ///     Fired when the simulation ticks the module that it created inside of itself.
        /// </summary>
        public override void Tick()
        {
            // If the active mode is not null and flag is set to remove then do that!
            if (ActiveMode != null && ActiveMode.ShouldRemoveMode)
                RemoveDirtyModes();

            // Otherwise just tick the game mode logic.
            ActiveMode?.TickMode();
        }
    }
}