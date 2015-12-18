﻿using TrailSimulation.Entity;
using TrailSimulation.Game;

namespace TrailSimulation.Event
{
    /// <summary>
    ///     Temporary unconsciousness caused by a blow to the head. The term is also used loosely of the aftereffects such as
    ///     confusion or temporary incapacity.
    /// </summary>
    [DirectorEvent(EventCategory.Person)]
    public sealed class Concussion : EventPersonInjure
    {
        /// <summary>
        ///     Fired after the event has executed and the injury flag set on the person.
        /// </summary>
        /// <param name="person">Person whom is now injured by whatever you say they are here.</param>
        /// <returns>Describes what type of physical injury has come to the person.</returns>
        protected override string OnPostInjury(Person person)
        {
            return $"{person.Name} has a concussion.";
        }
    }
}