using System;
using NovationLaunchpad.Models.Effects.Piskel;
using NovationLaunchpad.Models.Launchpads;

namespace NovationLaunchpad.Sample
{
    class Program
    {
        static void Main(string[] args)
        {
            var launchpad = LaunchpadMk2.GetInstance().Result;

            var input = string.Empty;

            while(input != "q")
            {
                Console.Clear();
                Console.WriteLine("Welcome to the Novation Launchpad sample app.");
                Console.WriteLine("Please enter a command to see examples in action. Enter ? to get a list of all valid commands.");

                input = Console.ReadLine();
                switch(input.ToLower())
                {
                    case "c":
                        launchpad.Clear();
                        break;
                    case "piskel":
                        launchpad.RegisterEffect(new PiskelEffect("demo.piskel", true));
                        break;
                    case "pulse":
                        Console.WriteLine("Pulse");
                        Console.WriteLine("Enter the button x value:");
                        if (!int.TryParse(Console.ReadLine(), out int pulseX))
                            continue;
                        Console.WriteLine("Enter the button y value:");
                        if (!int.TryParse(Console.ReadLine(), out int pulseY))
                            continue;
                        Console.WriteLine("Enter the color value as an int:");
                        if (!int.TryParse(Console.ReadLine(), out int pulseColor))
                            continue;
                        launchpad.PulseButton(pulseX, pulseY, LaunchpadMk2Color.DarkHotPink);
                        break;
                }
            }
        }
    }
}
