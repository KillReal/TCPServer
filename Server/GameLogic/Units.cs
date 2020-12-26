using System;
using System.Collections.Generic;
using System.Text;

namespace Server.GameLogic
{
    [Serializable]
    public class GameObj
    {
        public enum typeObj
        {
            empty,
            block,
            mine,
            town,
            unit,
        }
        public typeObj type;
        public Coord Position;
        public Player owner;
        public int health;
        public int attack;
        public int defense;
        public int damage;
        public int rangeAttack;
        public int shootingDamage;
        public GameObj() { }
        public GameObj(typeObj type)
        {
            this.type = type;
        }
    }

    // Units //
    [Serializable]
    abstract public class Unit : GameObj
    {
        public enum typeUnit
        {
            Scout,
            Warrior1,
            Shooter1,
            Warrior2,
            Shooter2,
            Warrior3,
            Shooter3,
            Top
        }
        public int actionPoints;
        public int MAXactionPoints;
        public typeUnit type_unit;
        public void NextTurn()
        {
            actionPoints = MAXactionPoints;
        }
        abstract public void atack(GameObj unit);
        public Unit(Player owner) : base(typeObj.unit)
        {
            this.owner = owner;
        }
    }

    [Serializable]
    public class Scout : Unit
    {
        public Scout(Player owner) : base(owner)
        {
            if (owner.gold - 100 < 0)
                throw new Exception("Not money");
            owner.gold -= 100;
            this.type_unit = typeUnit.Scout;
            this.attack = 2;
            this.defense = 2;
            this.damage = 1;
            this.health = 3;
            this.MAXactionPoints = 15;
            this.actionPoints = this.MAXactionPoints;
        }
        override public void atack(GameObj unit)
        {
            Coord range = (this.Position - unit.Position).ABS;
            if (range > 1.5) throw new Exception("long range");

            if (this.attack == unit.defense)
                unit.health -= this.damage;
            else
                if (this.attack > unit.defense)
                unit.health -= (int)(this.damage * ((unit.defense - this.attack) * 0.5));
            else
                unit.health -= (int)(this.damage * (1 / ((unit.defense - this.attack) * 0.4)));
        }
    }

    [Serializable]
    public class Warior : Unit
    {
        public int level;
        public int id;
        public Warior(Player owner,  int id = 1) : base(owner)
        {
            this.level = owner.town.level;
            this.id = id;
            switch (id)
            {
                case 1:
                    if (owner.gold - 100 < 0)
                        throw new Exception("Not money");
                    owner.gold -= 100;
                    this.type_unit = typeUnit.Warrior1;
                    this.attack = 6;
                    this.defense = 5;
                    this.damage = 4;
                    this.health = 10;
                    this.MAXactionPoints = 10;
                    this.actionPoints = this.MAXactionPoints;
                    this.rangeAttack = 0;
                    this.shootingDamage = 0;
                    break;
                case 3:
                    if (owner.town.level < 1) throw new Exception("Town level not 1");
                    if (owner.gold - 100 < 0) throw new Exception("Not money");
                    owner.gold -= 100;
                    this.type_unit = typeUnit.Warrior2;
                    this.attack = 8;
                    this.defense = 8;
                    this.damage = 10;
                    this.health = 25;
                    this.MAXactionPoints = 12;
                    this.actionPoints = this.MAXactionPoints;
                    this.rangeAttack = 0;
                    this.shootingDamage = 0;
                    
                    break;
                case 5:
                    if (owner.town.level < 2) throw new Exception("Town level not 2");
                    if (owner.gold - 100 < 0) throw new Exception("Not money");
                    owner.gold -= 100;
                    this.type_unit = typeUnit.Warrior3;
                    this.attack = 10;
                    this.defense = 12;
                    this.damage = 16;
                    this.health = 40;
                    this.MAXactionPoints = 13;
                    this.actionPoints = this.MAXactionPoints;
                    this.rangeAttack = 0;
                    this.shootingDamage = 0;
                    break;
            }
        }
        override public void atack(GameObj unit)
        {
            Coord range = (this.Position - unit.Position).ABS;
            if (range > 1.5) throw new Exception("long range");

            if (this.attack == unit.defense)
                unit.health -= this.damage;
            else
                if (this.attack > unit.defense)
                unit.health -= (int)(this.damage * ((unit.defense - this.attack) * 0.5));
            else
                unit.health -= (int)(this.damage * (1 / ((unit.defense - this.attack) * 0.4)));
        }
    }

    [Serializable]
    public class Shooter : Unit
    {
        public int level;
        public int id;
        public Shooter(Player owner, int id = 2) : base(owner)
        {
            this.level = owner.town.level;
            this.id = id;
            switch (id)
            {
                case 2:
                    if (owner.gold - 100 < 0) throw new Exception("Not money");
                    owner.gold -= 100;
                    this.type_unit = typeUnit.Shooter1;
                    this.shootingDamage = 8;
                    this.rangeAttack = 3;
                    this.attack = 1;
                    this.defense = 1;
                    this.damage = 2;
                    this.health = 5;
                    this.MAXactionPoints = 7;
                    this.actionPoints = this.MAXactionPoints;
                    break;
                case 4:
                    if (level < 1) throw new Exception("Town level not 1");
                    if (owner.gold - 100 < 0) throw new Exception("Not money");

                    owner.gold -= 100;
                    this.type_unit = typeUnit.Shooter2;
                    this.shootingDamage = 15;
                    this.rangeAttack = 5;
                    this.attack = 8;
                    this.defense = 5;
                    this.damage = 6;
                    this.health = 20;
                    this.MAXactionPoints = 9;
                    this.actionPoints = this.MAXactionPoints;
                    
                    break;
                case 6:
                    if (level < 2) throw new Exception("Town level not 2");
                    if (owner.gold - 100 < 0) throw new Exception("Not money");

                    owner.gold -= 100;
                    this.type_unit = typeUnit.Shooter3;
                    this.shootingDamage = 20;
                    this.rangeAttack = 7;
                    this.attack = 13;
                    this.defense = 8;
                    this.damage = 12;
                    this.health = 30;
                    this.MAXactionPoints = 10;
                    this.actionPoints = this.MAXactionPoints;
                    
                    break;
            }
        }
        override public void atack(GameObj unit)
        {
            Coord range = (this.Position - unit.Position).ABS;
            if (range > rangeAttack) throw new Exception("long range");
            if (range < 1.5)
            {
                if (this.attack == unit.defense)
                    unit.health -= this.damage;
                else
                    if (this.attack > unit.defense)
                    unit.health -= (int)(this.damage * ((unit.defense - this.attack) * 0.5));
                else
                    unit.health -= (int)(this.damage * (1 / ((unit.defense - this.attack) * 0.4)));
            }
            else
            {
                if (this.attack == unit.defense)
                    unit.health -= this.shootingDamage;
                else
                    if (this.attack > unit.defense)
                    unit.health -= (int)(this.shootingDamage * ((unit.defense - this.attack) * 0.5));
                else
                    unit.health -= (int)(this.shootingDamage * (1 / ((unit.defense - this.attack) * 0.4)));
            }
        }
    }

    [Serializable]
    public class Top : Unit
    {
        public Top(Player owner) : base(owner)
        {
            if (owner.town.level < 2) throw new Exception("Town level not 2");
            if (owner.gold - 100 < 0) throw new Exception("Not money");

            owner.gold -= 100;
            this.type_unit = typeUnit.Top;
            this.shootingDamage = 15;
            this.rangeAttack = 5;
            this.attack = 15;
            this.defense = 15;
            this.damage = 20;
            this.health = 45;
            this.MAXactionPoints = 15;
            this.actionPoints = this.MAXactionPoints;
        }
        override public void atack(GameObj unit)
        {
            Coord range = (this.Position - unit.Position).ABS;
            if (range > rangeAttack) throw new Exception("long range");
            if (range < 1.5)
            {
                if (this.attack == unit.defense)
                    unit.health -= this.damage;
                else
                    if (this.attack > unit.defense)
                    unit.health -= (int)(this.damage * ((unit.defense - this.attack) * 0.5));
                else
                    unit.health -= (int)(this.damage * (1 / ((unit.defense - this.attack) * 0.4)));
            }
            else
            {
                if (this.attack == unit.defense)
                    unit.health -= this.shootingDamage;
                else
                    if (this.attack > unit.defense)
                    unit.health -= (int)(this.shootingDamage * ((unit.defense - this.attack) * 0.5));
                else
                    unit.health -= (int)(this.shootingDamage * (1 / ((unit.defense - this.attack) * 0.4)));
            }
        }
    }
}