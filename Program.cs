/*
 * HOMER Network Discovery and State Monitor
 * HomerXS Build
 * Copyright 2018
 *
 * Author: Stephen Minnich
 * Date: 3/20/2018
 * Time: 1:45 PM
 */
 
using System;
using System.Linq;

namespace HomerXS
{
	class Program
	{	//Main uses a simple console menu to 
		//display XElements returned from IPDiscovery	
		
		public static void Main(string[] args)
		{	//***[0] Startup: IPDiscovery
			IPDiscovery ipDiscovery = new IPDiscovery();
			
			//***[0] Startup: pause to view startup trace
			Console.Write("\r\n\r\nPress any key to continue . . . ");
			Console.ReadKey(true);
			
			//Show Console Views until Quit
			var view = new SimpleXView();
			view.ShowDefaultView("Home Console View",ipDiscovery.ShowLocalMachineXState());
			bool run=true;
			//start run loop
			do {var choice = view.GetChoice();
				switch (choice) 
				{	case 'D': //Icmp discovery
						{	var header = "Icmp Discovery View";
							view.ShowDefaultView(header,ipDiscovery.ShowIcmpXState());
							Console.WriteLine("Discovering...");
							ipDiscovery.ShowIcmpDiscovery();
							view.ShowDefaultView(header,ipDiscovery.ShowIcmpXState());
							break;}
					default: //Default View
						{	var header = "Home Console View";
							view.ShowDefaultView(header, ipDiscovery.ShowKnownXState());
							break;}
					case '0': //Show Local
						{	var header = "Local IPHosts View";
							view.ShowDefaultView(header,ipDiscovery.ShowLocalMachineXState());
							break;}
					case '1': //Show Icmp
						{	var header = "Icmp IPHosts View";
							view.ShowDefaultView(header,ipDiscovery.ShowIcmpXState());
							break;}
					case '2': //Show Remote
						{	var header = "Known IPHosts View";							
							view.ShowDefaultView(header,ipDiscovery.ShowKnownXState());
							break;}
					case '3': //Add Remote
						{	var header = "Add Remote IPHosts Console View";
							Console.Write("\r\nEnter IP address to add >");
							string reply =  Console.ReadLine();
							Console.WriteLine("\r\nreply> {0}", reply);
							view.ShowDefaultView(header,ipDiscovery.ShowAddKnownXState(reply));
							break;}
					case '4': //Remove IPHost
						{	var header = "Remove IPHosts Console View";
							Console.Write("\r\nEnter IP address to remove >");
							string reply =  Console.ReadLine();
							Console.WriteLine("\r\nreply> {0}", reply);
							view.ShowDefaultView(header,ipDiscovery.ShowRemoveIPHostXState(reply));
							break;}
					case 'q': //***quit***
						{run=false;break;}} //switch(choice)
			} while (run);
			//end run loop
			Console.Write("\r\n\r\nPress any key to quit . . . ");
			Console.ReadKey(true);}

	} //Program
}