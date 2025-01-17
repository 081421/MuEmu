﻿using MU.DataBase;
using MU.Resources;
using MuEmu.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MuEmu
{
    public class Account
    {
        private Dictionary<byte, Storage> _vaults { get; set; }
        private int _vaultMoney;

        public bool NeedSave { get; set; }
        public int ID { get; set; }
        public string Nickname { get; set; }
        public Player Player { get; set; }
        public Dictionary<byte, CharacterDto> Characters { get; set; }
        public byte ActiveVault { get; set; }
        public Storage Vault => _vaults[ActiveVault];
        public int VaultMoney { get => _vaultMoney; set { _vaultMoney = value; NeedSave = true; } }

        public Account(Player player, AccountDto accountDto)
        {
            Player = player;
            ActiveVault = 0;

            _vaults = new Dictionary<byte, Storage>();
            ID = accountDto.AccountId;
            Nickname = accountDto.Account;
            VaultMoney = accountDto.VaultMoney;

            using (var db = new GameContext())
                for (var i = (byte)0; i < accountDto.VaultCount; i++)
                {
                    _vaults.Add(i, new Storage(Storage.WarehouseSize));
                    var items = db.Items
                        .Where(x => x.VaultId == (int)StorageID.Warehouse);

                    foreach (var it in items)
                    {
                        var item = new Item(it, this);
                        item.Account = this;
                        _vaults[i].Add(item, (byte)it.SlotId);
                    }
                }
        }

        public async Task Save(GameContext db)
        {
            foreach (var vault in _vaults.Values)
            {
                foreach (var it in vault.Items.Values)
                    await it.Save(db);
            }

            if (!NeedSave)
                return;

            var accDto = db.Accounts.First(x => x.AccountId == ID);
            accDto.VaultMoney = VaultMoney;
            db.Accounts.Update(accDto);
            NeedSave = false;
        }
    }
}
