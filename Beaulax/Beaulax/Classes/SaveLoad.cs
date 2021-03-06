﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;


namespace Beaulax.Classes
{
    class SaveLoad
    {
        // attributes
        private bool flashlight;
        private bool jumppack;
        private bool spaceSuit;
        private int access;
        private int locationX;
        private int locationY;
        private bool hasJumped;
        private string roomNum;
        private string roomWas;
        private int health;

        private float floatX; // need this to set the vector for location later
        private float floatY; // need this to set the vector for location later

        // default contructor
        public SaveLoad()
        {
            flashlight = false;
            jumppack = false;
            spaceSuit = false;
            access = 0;
            locationX = 0;
            locationY = 0;
            floatX = 0.0f;
            floatY = 0.0f;
            hasJumped = false;
        }

        // methods

        /// <summary>
        /// This method saves the player's data to an external file, allowing them to reaccess it later.
        /// </summary>
        /// <param name="p"> takes in a player object and uses it's properties to save the current gamestate </param>
        public void Save(Player p, Game1 game)
        {
            roomNum = game.currRoom;
            roomWas = game.wasPlayerRoom;
            flashlight = p.HasFlashlight;
            jumppack = p.HasJumppack;
            spaceSuit = p.HasSpacesuit;
            access = p.AccessLevel;
            locationX = (int)p.Location.X;
            locationY = (int)p.Location.Y;
            hasJumped = p.HasJumped;
            health = p.CharacterHealth;


            Stream outStream = File.OpenWrite("saveFile.data");

            BinaryWriter output = new BinaryWriter(outStream);

            output.Write(roomNum);
            output.Write(roomWas);
            output.Write(flashlight);
            output.Write(jumppack);
            output.Write(spaceSuit);
            output.Write(access);
            output.Write(locationX);
            output.Write(locationY);
            output.Write(hasJumped);
            output.Write(health);

            outStream.Close();

            Console.WriteLine("Save complete");
        }

        /// <summary>
        /// Loads the previously saved
        /// </summary>
        /// <param name="p"></param>
        public void Load(Player p, Game1 game)
        {
            
            Stream inStream = null;

            try
            {
                inStream = File.OpenRead("saveFile.data");

                BinaryReader input = new BinaryReader(inStream);

                game.currRoom = input.ReadString();
                game.wasPlayerRoom = input.ReadString();

                game.ReadMap(game.currRoom); // read the map at the designated room

                p.HasFlashlight = input.ReadBoolean();
                p.HasJumppack = input.ReadBoolean();
                p.HasSpacesuit = input.ReadBoolean();
                p.AccessLevel = input.ReadInt32();
                floatX = (float)input.ReadInt32();
                floatY = (float)input.ReadInt32();
                p.HasJumped = input.ReadBoolean();
                p.CharacterHealth = input.ReadInt32();

                p.Location = new Vector2(floatX, floatY);
                Console.WriteLine(p);
            }
            catch (Exception e)
            {
                Console.WriteLine("Warning: File Does Not Exist\n" + e.Message);
            }
            finally
            {
                inStream.Close();
            }
        }

        public void LoadExtP(Player p, Game1 game)
        {
            //Stream inStream = File.OpenRead("Z:\\IGMProfile\\Documents\\GitHub\\Beaulax\\ExternalTool_CharacterEditor\\ExternalTool_CharacterEditor\\bin\\Debug\\playerSave.data"); // reads in file from external tool // creates a stream
            Stream inStream = File.OpenRead("playerSave.data"); // reads in file from external tool // creates a stream

            try
            {
                BinaryReader input = new BinaryReader(inStream); // opens binary reader

                game.playerMaxHealth = input.ReadInt32();
                game.playerHealth = game.playerMaxHealth;
                p.CharacterHealth = game.playerMaxHealth;

                game.playerDamage = input.ReadInt32();
                p.CharacterDamage = game.playerDamage;

                game.playerSpeed = (float)input.ReadInt32();
                p.Speed = game.playerSpeed;

                game.playerJumpHeight = (float)input.ReadInt32();
                p.JumpHeight = game.playerJumpHeight;

                game.access = input.ReadInt32();
                p.AccessLevel = game.access;

                game.hasJump = input.ReadBoolean();
                p.HasJumppack = game.hasJump;

                game.hasFlash = input.ReadBoolean();
                p.HasFlashlight = game.hasFlash;

                game.hasTank = input.ReadBoolean();
                p.HasSpacesuit = game.hasTank;

                inStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("Error with external tool: " + e);
            }
            finally
            {
                inStream.Close();
            }
        }

        public void LoadExtE(Enemy e)
        {
            //Stream inStream = File.OpenRead("Z:\\IGMProfile\\Documents\\GitHub\\Beaulax\\ExternalTool_CharacterEditor\\ExternalTool_CharacterEditor\\bin\\Debug\\enemySave.data"); // reads in file from external tool // creates a stream
            Stream inStream = File.OpenRead("enemySave.data"); // reads in file from external tool // creates a stream

            try
            {
                BinaryReader input = new BinaryReader(inStream); // opens binary reader

                e.CharacterHealth = input.ReadInt32();
                e.CharacterDamage = input.ReadInt32();
                e.Speed = (float)input.ReadInt32();
                e.JumpHeight = (float)input.ReadInt32();
                e.AtkRange = input.ReadInt32();


                inStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                inStream.Close();
            }
        }
    }
}
