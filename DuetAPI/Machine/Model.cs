﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace DuetAPI.Machine
{
    /// <summary>
    /// Representation of the full machine model
    /// </summary>
    public class Model : ICloneable
    {
        /// <summary>
        /// Information about the main and expansion boards
        /// </summary>
        public Electronics.Model Electronics { get; set; } = new Electronics.Model();
        
        /// <summary>
        /// Information about the fans
        /// </summary>
        public List<Fans.Fan> Fans { get; set; } = new List<Fans.Fan>();
        
        /// <summary>
        /// Information about the heat subsystem
        /// </summary>
        public Heat.Model Heat { get; set; } = new Heat.Model();
        
        /// <summary>
        /// Information about the current file job (if any)
        /// </summary>
        public Job.Model Job { get; set; } = new Job.Model();
        
        /// <summary>
        /// Information about message box requests
        /// </summary>
        public MessageBox.Model MessageBox { get; set; } = new MessageBox.Model();
        
        /// <summary>
        /// Information about the move subsystem
        /// </summary>
        public Move.Model Move { get; set; } = new Move.Model();
        
        /// <summary>
        /// Information about connected network adapters
        /// </summary>
        public Network.Model Network { get; set; } = new Network.Model();
        
        /// <summary>
        /// Generic messages that do not belong explicitly to codes being executed
        /// </summary>
        public List<Message> Output { get; set; } = new List<Message>();
        
        /// <summary>
        /// Information about the 3D scanner subsystem
        /// </summary>
        public Scanner.Model Scanner { get; set; } = new Scanner.Model();
        
        /// <summary>
        /// Information about connected sensors including Z-probes and endstops
        /// </summary>
        public Sensors.Model Sensors { get; set; } = new Sensors.Model();
        
        /// <summary>
        /// Information about CNC spindles
        /// </summary>
        public List<Spindles.Spindle> Spindles { get; set; } = new List<Spindles.Spindle>();
        
        /// <summary>
        /// Information about the machine state
        /// </summary>
        public State.Model State { get; set; } = new State.Model();
        
        /// <summary>
        /// Information about the configured storage systems
        /// </summary>
        public List<Storages.Storage> Storages { get; set; } = new List<Storages.Storage>();
        
        /// <summary>
        /// Information about configured tools
        /// </summary>
        public List<Tools.Tool> Tools { get; set; } = new List<Tools.Tool>();

        /// <summary>
        /// Creates a duplicate of the full object model
        /// </summary>
        /// <returns>A clone of this model</returns>
        public object Clone()
        {
            return new Model
            {
                Electronics = (Electronics.Model)Electronics.Clone(),
                Fans = Fans.Select(fan => (Fans.Fan)fan.Clone()).ToList(),
                Heat = (Heat.Model)Heat.Clone(),
                Job = (Job.Model)Job.Clone(),
                MessageBox = (MessageBox.Model)MessageBox.Clone(),
                Move = (Move.Model)Move.Clone(),
                Network = (Network.Model)Network.Clone(),
                Output = Output.Select(item => (Message)item.Clone()).ToList(),
                Scanner = (Scanner.Model)Scanner.Clone(),
                Sensors = (Sensors.Model)Sensors.Clone(),
                Spindles = Spindles.Select(spindle => (Spindles.Spindle)spindle.Clone()).ToList(),
                State = (State.Model)State.Clone(),
                Storages = Storages.Select(storage => (Storages.Storage)storage.Clone()).ToList(),
                Tools = Tools.Select(tool => (Tools.Tool)tool.Clone()).ToList()
            };
        }
    }
}