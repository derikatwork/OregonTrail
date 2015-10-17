﻿using System.Collections.ObjectModel;
using TrailCommon;
using TrailEntities;

namespace TrailGame
{
    public class StoreMode : Mode, IStore
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="T:TrailEntities.GameWindow" /> class.
        /// </summary>
        public StoreMode(Vehicle vehicle) : base(vehicle)
        {
        }

        public override GameMode ModeType
        {
            get { return GameMode.Store; }
        }

        public ReadOnlyCollection<IItem> StoreInventory
        {
            get { return StoreController.StoreInventory; }
        }

        public string StoreName
        {
            get { return StoreController.StoreName; }
        }

        public uint StoreBalance
        {
            get { return StoreController.StoreBalance; }
        }

        public void BuyItems(IItem item)
        {
            StoreController.BuyItems(item);
        }

        public void SellItem(IItem item)
        {
            StoreController.SellItem(item);
        }

        public IStore StoreController { get; }
    }
}