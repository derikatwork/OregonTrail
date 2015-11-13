﻿using System.Collections.Generic;

namespace TrailEntities.Simulation
{
    /// <summary>
    ///     Deals with keep track of input to the simulation via whatever form that may end up taking. The default
    ///     implementation is a text user interface (TUI) which allows for the currently accepted commands to be seen and only
    ///     them accepted.
    /// </summary>
    public sealed class InputManager : SimulationModule
    {
        /// <summary>
        ///     Fired when the input buffer is going to be inserted into the running simulation for it to parse. Generally this is
        ///     done by user pressing return key... but it could be really anything that updates it even manual entry in code.
        /// </summary>
        public delegate void InputBufferUpdated(string inputBuffer, string addedKeycharString);

        /// <summary>
        ///     Fired when the input buffer has processed a queued command to be sent and has fired this event to let the
        ///     simulation know it wants him to deal with it.
        /// </summary>
        public delegate void InputManagerSendCommand(string command);

        /// <summary>
        ///     Holds a constant representation of the string telling the user to press enter key to continue so we don't repeat
        ///     ourselves.
        /// </summary>
        public const string PRESS_ENTER = "Press ENTER KEY to continue";

        /// <summary>
        ///     Holds constant representation of the text that is shown after a YES/NO question to the user.
        /// </summary>
        public const string PRESS_YESNO = "What is your response? Y/N";

        /// <summary>
        ///     Holds a series of commands that need to be executed in the order they come out of the collection.
        /// </summary>
        private Queue<string> _commandQueue;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public InputManager()
        {
            _commandQueue = new Queue<string>();
            InputBuffer = string.Empty;
        }

        /// <summary>
        ///     Input buffer that we will use to hold characters until need to send them to simulation.
        /// </summary>
        internal string InputBuffer { get; private set; }

        /// <summary>
        ///     Clears the input buffer and submits whatever was in there to the simulation for processing. Implementation is left
        ///     up the game simulation itself entirely.
        /// </summary>
        public void SendInputBufferAsCommand()
        {
            // Trim the result of the input so no extra whitespace at front or end exists.
            var lineBufferTrimmed = InputBuffer.Trim();

            // Destroy the input buffer if we are not accepting commands but return is pressed anyway.
            if (!GameSimulationApp.Instance.WindowManager.AcceptingInput)
                InputBuffer = string.Empty;

            // Send trimmed line buffer to game simulation, if not accepting input we just pass along empty string.
            AddCommandToQueue(lineBufferTrimmed);

            // Always forcefully clear the input buffer after returning it, this makes it ready for more input.
            InputBuffer = string.Empty;
        }

        /// <summary>
        ///     Fired when the simulation receives an individual character from then input system. Depending on what it is we will
        ///     do something, or not!
        /// </summary>
        /// <param name="addedKeyString">String character converted into a string representation of itself.</param>
        private void OnCharacterAddedToInputBuffer(string addedKeyString)
        {
            // Disable passing along input buffer if the simulation is not currently accepting input from the user.
            if (!GameSimulationApp.Instance.WindowManager.AcceptingInput)
                return;

            // Add the character to the end of the input buffer.
            InputBuffer += addedKeyString;

            // Fire event for all subscribers to get total buffer and added character string.
            CharacterAddedToInputBuffer?.Invoke(InputBuffer, addedKeyString);
        }

        /// <summary>
        ///     Populates an internal input buffer for the simulation that is used to eventually return a possible command string
        ///     to active game mode.
        /// </summary>
        public void AddCharToInputBuffer(char keyChar)
        {
            // Filter to prevent non-characters like delete, insert, scroll lock, etc.
            if (!char.IsLetter(keyChar) && !char.IsNumber(keyChar))
                return;

            // Convert character to string representation if itself.
            var addedKeyString = char.ToString(keyChar);
            OnCharacterAddedToInputBuffer(addedKeyString);
        }

        /// <summary>
        ///     Removes the last character from input buffer if greater than zero.
        /// </summary>
        public void RemoteLastCharOfInputBuffer()
        {
            if (InputBuffer.Length > 0)
                InputBuffer = InputBuffer.Remove(InputBuffer.Length - 1);
        }

        /// <summary>
        ///     Fired by messaging system or user interface that wants to interact with the simulation by sending string command
        ///     that should be able to be parsed into a valid command that can be run on the current game mode.
        /// </summary>
        /// <param name="returnedLine">Passed in command from controller, text was trimmed but nothing more.</param>
        private void AddCommandToQueue(string returnedLine)
        {
            // Trim the input.
            var trimmedInput = returnedLine.Trim();

            // Skip if we already entered the same command, simulation is state based... no need for flooding.
            if (_commandQueue.Contains(trimmedInput))
                return;

            // Adds the command to queue to be passed to simulation when input manager is ticked.
            _commandQueue.Enqueue(trimmedInput);
        }

        /// <summary>
        ///     Fired when the input buffer is going to be inserted into the running simulation for it to parse. Generally this is
        ///     done by user pressing return key... but it could be really anything that updates it even manual entry in code.
        /// </summary>
        public event InputBufferUpdated CharacterAddedToInputBuffer;

        /// <summary>
        ///     Fired when the simulation is closing and needs to clear out any data structures that it created so the program can
        ///     exit cleanly.
        /// </summary>
        public override void Destroy()
        {
            // Clear the input buffer.
            InputBuffer = string.Empty;

            // Clear the command queue.
            _commandQueue.Clear();
            _commandQueue = null;
        }

        /// <summary>
        ///     Fired when the simulation ticks the module that it created inside of itself.
        /// </summary>
        public override void Tick()
        {
            // Skip if there are no commands to tick.
            if (_commandQueue.Count <= 0)
                return;

            // Dequeue the next command to send and pass along to currently active game mode if it exists.
            InputManagerSendCommandEvent?.Invoke(_commandQueue.Dequeue());
        }

        /// <summary>
        ///     Fired when the input buffer has processed a queued command to be sent and has fired this event to let the
        ///     simulation know it wants him to deal with it.
        /// </summary>
        public event InputManagerSendCommand InputManagerSendCommandEvent;
    }
}