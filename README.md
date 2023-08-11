# Terraria Pixelbox Animation Builder

[![Animated Shrek](https://i.imgur.com/QIHeAA8.png)](https://www.youtube.com/watch?v=EVP2zqgrtzg "Terraria But Its Animated Shrek - Click to Watch!")

 **Step1: Converting a video**

Notes:
 - ROM has a max of 3000 frames.
 - Map has a built FPS settings - 24, 30, 60, 120.

Steps:
1) Download Original Video - https://yt1s.com/en5
2) Calcualte what FPS is required to fit within 3000 frames - https://www.zapstudio.net/framecalc/
3) Reduce FPS of the video - https://www.video2edit.com/convert-to-video
4) Reduce Video To 120x68 - https://www.videosoftdev.com/ or https://www.onlineconverter.com/resize-video
5) Convert To Image Sequence - https://mconverter.eu/convert/mp4/png/
6) Bulk Convert To Black/White Using Mono Mixer & Adjust - https://www.xnview.com/en/xnconvert/

**Step2: Converting Image Sequence To Schematic**
Using the tool `PictureToBinary()` its possible to easily convert all the images to a binary sequance of 1 and 0s. Simply input the sequance, select an output file location, and click convert. Drop this file in your Terraria directory `\steamapps\common\Terraria\#romupload`. 

**Step3: Programing video to ROM**

Now that we have created a map with every possible tile and wall, its time we attempt to try and read the colors just as the game would. To do this, I will be using an open source tool called [DnSpy](https://github.com/dnSpy/dnSpy) to edit the games compiled code. Using one of the games functions called `ProcessIncomingMessage()` I'm able to add some code onto this that will be able to interpret commands. We will add two commands; `/arom` for programing, and `/delrom` to remove it.
 
<details><summary>Show Code</summary>
 
```c#
//###################################################################################
//Terraria.Chat > ChatCommandProcessor (OR CreateOutgoingMessage)
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Terraria.Chat.Commands;
using Terraria.Localization;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace Terraria.Chat
{
	public partial class ChatCommandProcessor : IChatProcessor
	{
		public void ProcessIncomingMessage(ChatMessage message, int clientId)
		{
			// ###################################################################################
			if (message.Text.Contains("/arom")) // Animation rom.
			{
				string SchemName = "";
				int RunBy = 0;
				string Message = message.Text;
				int romWidth = 8160;
				int romHeight = 2249;

				// We are starting from the right to left so add the width.
				Vector2 playerPosition = new Vector2((Main.LocalPlayer.position.X / 16), (Main.LocalPlayer.position.Y / 16) + 5); // Offset 5 down.
				Vector2 wirePosition = playerPosition;
				int lineOffset = 1;
				int WireColor = 1;
				try
				{
					// Create a new list of words based on a sentences spaces.
					List<string> wordList = Message.TrimStart(new char[]{' '}).Split(new char[]{' '}).ToList<string>();
					wordList.RemoveAt(0); // Remove the first word -> /schem.

					// Define variables based on the list of words.

					foreach (string OutString in string.Join(" ", wordList.ToArray()).Split(new char[]{' '}))
					{
						if (RunBy == 0) // Define the first variable.
						{
							SchemName = OutString;
							RunBy++;
						}
					}

					// Load Schem
					if (SchemName == "")
					{
						Console.WriteLine("ERROR: Type a schematic name!");
						Main.NewTextMultiline("ERROR: Type a schematic name!", false, Color.Red, -1);
						return;
					}
					else
					{
						try
						{
							// Create a string of 0s width long.
							char[] previousLine = new char[romWidth];
							for (int i = 0; i < romWidth; i++)
							{
								previousLine[i] = '0';
							}

							// Go through each line of wordlist.
							foreach (string Line in File.ReadLines(@"C:\Program Files (x86)\Steam\steamapps\common\Terraria\#romupload\" + SchemName + ".txt"))
							{
								// Check if to use encoding.
								string bitFormat = "";
								for (int i = 0; i < romWidth; i++)
								{
									bitFormat += (int.Parse(previousLine[i].ToString()) ^ int.Parse(Line[i].ToString())).ToString();
									previousLine[i] = Line[i];
								}

								// System.Windows.Forms.MessageBox.Show(ConvertArrayBuilder(arrayHalf));        
								// Place the wires based on a status serial format - XOR ROM.
								//
								// Go through each bit within the returned binary.
								foreach (char bit in bitFormat) // For xor ConvertXORArray(arrayFourth)
								{
									// If bit is a 1 then place wire.
									if (bit.ToString() == "1")
									{
										// Get the wire color.
										if (WireColor == 1) // Blue first 1-14, red second 15,28.
										{
											// Place red wire.
											WorldGen.PlaceWire((int)wirePosition.X, (int)wirePosition.Y);
										}
										else if (WireColor == 2)
										{
											// Place blue wire.
											WorldGen.PlaceWire2((int)wirePosition.X, (int)wirePosition.Y);
										}
										else if (WireColor == 3)
										{
											// Place green wire.
											WorldGen.PlaceWire3((int)wirePosition.X, (int)wirePosition.Y);
										}
										else if (WireColor == 4)
										{
											// Place yellow wire.
											WorldGen.PlaceWire4((int)wirePosition.X, (int)wirePosition.Y);
										}
									}

									// Move the postion 1 right for the next byte.
									wirePosition.X++;
								}

								// Continue to go down.
								//
								// Reset the X position.
								wirePosition.X = playerPosition.X;

								// Progress wire color.
								if (WireColor == 4)
								{
									// Reset wirecolor back to 1.
									WireColor = 1;

									// Progress position down.
									wirePosition.Y += 3;
								}
								else
								{
									WireColor++;
								}
							}

							// Display Console
							Console.WriteLine(string.Concat(new string[]{"Schematic: ", SchemName.ToString(), " has loaded successfully!"}));
							Main.NewTextMultiline(string.Concat(new string[]{"Schematic: ", SchemName.ToString(), " has loaded successfully!"}), false, Color.Green, -1);
						}
						catch (Exception)
						{
							Console.WriteLine("Schematic: " + SchemName.ToString() + " was not found!");
							Main.NewTextMultiline("Schematic: " + SchemName.ToString() + " was not found!", false, Color.Red, -1);
						}
					}

					// Command Finished
					return;
				}
				catch (Exception)
				{
					Console.WriteLine("ERROR: Command Usage - /arom [schem]");
					Main.NewTextMultiline("ERROR: Command Usage - /arom [schem]", false, Color.Red, -1);
				}

				return;
			}

			// ###################################################################################
			if (message.Text.Contains("/delrom")) // Animation rom.
			{
				string SchemName = "";
				int RunBy = 0;
				string Message = message.Text;
				int romWidth = 8160;
				int romHeight = 2249;

				// We are starting from the right to left so add the width.
				Vector2 playerPosition = new Vector2((Main.LocalPlayer.position.X / 16), (Main.LocalPlayer.position.Y / 16) + 5); // Offset 5 down.
				Vector2 wirePosition = playerPosition;
				int lineOffset = 1;
				int WireColor = 1;
				try
				{
					// Create a list with height amount of rows of width long charecters.
					var clearSchematic = new List<string>(Enumerable.Repeat("".PadLeft(romWidth, '0'), romHeight).ToList());

					// Go through each line of wordlist.
					foreach (string Line in clearSchematic)
					{
						// System.Windows.Forms.MessageBox.Show(ConvertArrayBuilder(arrayHalf));        
						// Place the wires based on a status serial format - XOR ROM.
						//
						// Go through each bit within the returned binary.
						foreach (char bit in Line) // For xor ConvertXORArray(arrayFourth)
						{
							// Remove wire colors.
							WorldGen.KillWire((int)wirePosition.X, (int)wirePosition.Y);
							WorldGen.KillWire2((int)wirePosition.X, (int)wirePosition.Y);
							WorldGen.KillWire3((int)wirePosition.X, (int)wirePosition.Y);
							WorldGen.KillWire4((int)wirePosition.X, (int)wirePosition.Y);
							// Move the postion 1 right for the next byte.
							wirePosition.X++;
						}

						// Continue to go down.
						//
						// Reset the X position.
						wirePosition.X = playerPosition.X;

						// Progress wire color.
						if (WireColor == 4)
						{
							// Reset wirecolor back to 1.
							WireColor = 1;

							// Progress position down.
							wirePosition.Y += 3;
						}
						else
						{
							WireColor++;
						}
					}

					// Display Console
					Console.WriteLine(string.Concat(new string[]{"The ROM has been wiped! Area cleaned: " + romWidth + "x" + romHeight}));
					Main.NewTextMultiline(string.Concat(new string[]{"The ROM has been wiped! Area cleaned: " + romWidth + "x" + romHeight}), false, Color.Green, -1);

					// Command Finished
					return;
				}
				catch (Exception)
				{
					Console.WriteLine("ERROR: Command Usage - /delrom");
					Main.NewTextMultiline("ERROR: Command Usage - /delrom", false, Color.Red, -1);
				}

				return;
			}

			// ###################################################################################
			IChatCommand chatCommand;
			if (this._commands.TryGetValue(message.CommandId, out chatCommand))
			{
				chatCommand.ProcessIncomingMessage(message.Text, (byte)clientId);
				message.Consume();
				return;
			}

			if (this._defaultCommand != null)
			{
				this._defaultCommand.ProcessIncomingMessage(message.Text, (byte)clientId);
				message.Consume();
			}
		}
	}
}
```
</details>
