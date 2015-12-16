﻿using System;
using System.Diagnostics;
using System.Linq;
using TrailSimulation.Event;
using TrailSimulation.Game;

namespace TrailSimulation.Entity
{
    /// <summary>
    ///     Represents a human-being. Gender is not tracked, we only care about them as an entity that consumes food and their
    ///     health.
    /// </summary>
    public sealed class Person : IEntity
    {
        /// <summary>
        ///     Defines the current health of the person. It will be tracked and kept within bounds of HealthMin and HealthMax
        ///     constants.
        /// </summary>
        private int _health;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:TrailEntities.Entities.Person" /> class.
        /// </summary>
        public Person(Profession profession, string name, bool isLeader)
        {
            Profession = profession;
            Name = name;
            IsLeader = isLeader;

            // Starts the player at maximum health.
            Health = (int) HealthLevel.Good;
        }

        /// <summary>
        ///     Current health of this person which is enum that also represents the total points they are currently worth.
        /// </summary>
        public HealthLevel HealthLevel
        {
            get
            {
                // Default response is to report the person as dead.
                return Health >= (int) HealthLevel.Good ? HealthLevel.Good : HealthLevel.Dead;
            }
        }

        /// <summary>
        ///     Defines the current health of the person. It will be tracked and kept within bounds of HealthMin and HealthMax
        ///     constants.
        /// </summary>
        private int Health
        {
            get { return _health; }
            set
            {
                // Check that value is not above max.
                if (_health > (int) HealthLevel.Good)
                    value = (int) HealthLevel.Good;

                // Check that value is not below min.
                if (_health < (int) HealthLevel.Dead)
                    value = (int) HealthLevel.Dead;

                // Set health to ceiling corrected value.
                _health = value;
            }
        }

        /// <summary>
        ///     Profession of this person, typically if the leader is a banker then the entire family is all bankers for sanity
        ///     sake.
        /// </summary>
        public Profession Profession { get; }

        /// <summary>
        ///     Determines if this person is the party leader, without this person the game will end. The others cannot go on
        ///     without them.
        /// </summary>
        public bool IsLeader { get; }

        /// <summary>
        ///     Name of the person as they should be known by other players and the simulation.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Defines what type of entity this will take the role of in the simulation. Depending on this value the simulation
        ///     will affect how it is treated, points tabulated, and interactions governed.
        /// </summary>
        public Entities Category
        {
            get { return Entities.Person; }
        }

        public int Compare(IEntity x, IEntity y)
        {
            Debug.Assert(x != null, "x != null");
            Debug.Assert(y != null, "y != null");

            var result = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            if (result != 0) return result;

            return result;
        }

        public int CompareTo(IEntity other)
        {
            Debug.Assert(other != null, "other != null");

            var result = string.Compare(other.Name, Name, StringComparison.Ordinal);
            if (result != 0) return result;

            return result;
        }

        public bool Equals(IEntity other)
        {
            // Reference equality check
            if (this == other)
            {
                return true;
            }

            if (other == null)
            {
                return false;
            }

            if (other.GetType() != GetType())
            {
                return false;
            }

            if (Name.Equals(other.Name))
            {
                return true;
            }

            return false;
        }

        public bool Equals(IEntity x, IEntity y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IEntity obj)
        {
            var hash = 23;
            hash = (hash*31) + Name.GetHashCode();
            return hash;
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
        public void OnTick(bool systemTick)
        {
            // Only tick person with simulation.
            if (systemTick)
                return;

            ConsumeFood();
            CheckIllness();
        }

        /// <summary>
        ///     Determines how much food party members in the vehicle will eat today.
        /// </summary>
        private void ConsumeFood()
        {
            // Grab instance of the game simulation to increase readability.
            var game = GameSimulationApp.Instance;

            var cost_food = game.Vehicle.Inventory[Entities.Food].TotalValue;
            cost_food = cost_food - 8 - 5*(int) game.Vehicle.Ration;
            if (cost_food >= 13 && HealthLevel != HealthLevel.Dead)
            {
                // Consume the food since we still have some.
                game.Vehicle.Inventory[Entities.Food] = new SimItem(
                    game.Vehicle.Inventory[Entities.Food],
                    (int) cost_food);

                // Change to get better when eating well.
                Heal();
            }
            else
            {
                // Reduce the players health until they are dead.
                Damage();
            }
        }

        /// <summary>
        ///     Increases person's health until it reaches maximum value. When it does will fire off event indicating to player
        ///     this person is now well again and fully healed.
        /// </summary>
        public void Heal()
        {
            // Skip if already at max health.
            if (HealthLevel == HealthLevel.Good)
                return;

            // Grab instance of the game simulation to increase readability.
            var game = GameSimulationApp.Instance;

            // Increase health by a random amount.
            Health += game.Random.Next(1, 10);

            // Skip if we still have some more healing to do.
            if (HealthLevel != HealthLevel.Good)
                return;

            // Full health fires off event indicating that we are at full health once more.
            game.EventDirector.TriggerEvent(this, typeof (WellAgain));
        }

        /// <summary>
        ///     Check if party leader or a member of it has been killed by an illness.
        /// </summary>
        private void CheckIllness()
        {
            // Grab instance of the game simulation to increase readability.
            var game = GameSimulationApp.Instance;

            // Cannot calculate illness for the dead.
            if (HealthLevel == HealthLevel.Dead)
                return;

            if (game.Random.Next(100) <= 10 +
                35*((int) game.Vehicle.Ration - 1))
            {
                // Mild illness.
                game.Vehicle.ReduceMileage(5);
                Damage();
            }
            else if (game.Random.Next(100) <= 5 -
                     (40/game.Vehicle.Passengers.Count()*
                      ((int) game.Vehicle.Ration - 1)))
            {
                // Bad illness.
                game.Vehicle.ReduceMileage(10);
                Damage();
            }
            else
            {
                // Severe illness.
                game.Vehicle.ReduceMileage(15);
                Damage();
                TryInfect();
            }

            // If vehicle is not moving we will assume we are resting.
            if (game.Vehicle.Status != VehicleStatus.Moving)
                Heal();

            // Determines if we should roll for infections based on previous complications.
            switch (HealthLevel)
            {
                case HealthLevel.Good:
                    // Congrats on living a healthy lifestyle...
                    Heal();
                    break;
                case HealthLevel.Fair:
                    // Not eating for a couple days is going to hit you hard.
                    if (game.Vehicle.Inventory[Entities.Food].Quantity <= 0 &&
                        game.Vehicle.Status != VehicleStatus.Stopped)
                    {
                        game.Vehicle.ReduceMileage(5);
                        Damage();
                    }
                    break;
                case HealthLevel.Poor:
                    // Player is working themselves to death.
                    if (game.Vehicle.Inventory[Entities.Food].Quantity <= 0 &&
                        game.Vehicle.Status != VehicleStatus.Stopped)
                    {
                        game.Vehicle.ReduceMileage(10);
                        Damage();
                    }
                    break;
                case HealthLevel.VeryPoor:
                    Damage();
                    TryInfect();
                    break;
                case HealthLevel.Dead:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Attempts to infect the person with some ailment, rolls the dice to determine if this should be done. Will not
        ///     infect people that already have an infection.
        /// </summary>
        private void TryInfect()
        {
            // Grab instance of the game simulation to increase readability.
            var game = GameSimulationApp.Instance;

            // Infects the uninfected, progresses infections of existing people.
            if (game.Random.Next(100) <= 5)
            {
                // Pick an actual severe illness from list, roll the dice for it on very low health.
                game.EventDirector.TriggerEventByType(this, EventCategory.Person);
            }
            else if (game.Random.Next(100) >= 50)
            {
                Health--;
            }
        }

        /// <summary>
        ///     Reduces the persons health by a random amount from minimum health value to highest. If this reduces the players
        ///     health below zero the person will be considered dead.
        /// </summary>
        private void Damage()
        {
            // Skip what is already dead, no damage to be applied.
            if (HealthLevel == HealthLevel.Dead)
                return;

            // Grab instance of the game simulation to increase readability.
            var game = GameSimulationApp.Instance;

            // Reduce the persons health by random amount from death amount to desired damage level.
            Health -= game.Random.Next(10, 50);

            // Chance for broken bones and other ailments related to damage (but not death).
            game.EventDirector.TriggerEventByType(this, EventCategory.Person);

            // Check if health dropped to dead levels.
            if (HealthLevel != HealthLevel.Dead)
                return;

            // Reduce person's health to dead level.
            Health = (int) HealthLevel.Dead;

            // Death makes everybody take a huge morale hit.
            game.Vehicle.ReduceMileage(50);

            // Check if leader died or party member.
            game.EventDirector.TriggerEvent(this, IsLeader ? typeof (DeathPlayer) : typeof (DeathCompanion));
        }
    }
}