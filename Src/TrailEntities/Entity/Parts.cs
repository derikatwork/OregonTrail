﻿namespace TrailEntities.Entity
{
    /// <summary>
    ///     Defines a bunch of items that are used as parts in the vehicle.
    /// </summary>
    public static class Parts
    {
        /// <summary>
        ///     Zero weight animal that is attached to the vehicle but not actually 'inside' of it, but is still in the list of
        ///     inventory items that define the vehicle the player and his party is making the journey in.
        /// </summary>
        public static Item Oxen
        {
            get { return new Item(Entity.Animal, "Oxen", "oxen", "ox", 20, 20, 0); }
        }

        /// <summary>
        ///     Required to keep the vehicle moving if this part is broken it must be replaced before the player can
        ///     continue their journey.
        /// </summary>
        public static Item Axle
        {
            get { return new Item(Entity.Axle, "Vehicle Axle", "axles", "axle", 3, 10, 0); }
        }

        /// <summary>
        ///     Required to keep the vehicle running, if the tongue breaks then the player will have to fix or replace it before
        ///     they can continue on the journey again.
        /// </summary>
        public static Item Tongue
        {
            get { return new Item(Entity.Tongue, "Vehicle Tongue", "tongues", "tongue", 3, 10, 0); }
        }

        /// <summary>
        ///     Required to keep the vehicle moving down the path, if any of the wheel parts break they must be replaced before the
        ///     journey can continue.
        /// </summary>
        public static Item Wheel
        {
            get { return new Item(Entity.Wheel, "Vehicle Wheel", "wheels", "wheel", 3, 10, 0); }
        }
    }
}