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
using System.Collections.Generic;
using System.Xml.Linq;

namespace HomerXS
{
	/// <summary>
	/// Description of SimpleXView.
	/// </summary>
	public class SimpleXView
	{
		public SimpleXView()
		{
		}
		const string BANNER = "HOMER Network Discovery and State Monitor\r\n";
		const string UNDERLINE = "============================================";
		const string PROMPT = "\r\nHomer>";
		//Viewer Methods
		public char GetChoice()
		{	//read and return choice from Console
			Console.Write(PROMPT);
			return Console.ReadKey().KeyChar;}
		
		public void ShowDefaultView (string header, XElement xiphosts)
		{	ShowViewHeader(header);
			ShowXIPHostList(xiphosts);
			ShowViewMenu();}
		
		public void ShowViewHeader (string viewname)
		{	//Initialize View
			Console.Clear();
			Console.WriteLine(BANNER);
			Console.WriteLine(viewname + "\r\n");}
		
		public void ShowViewMenu ()
		{	Console.WriteLine();
			Console.WriteLine("[D] Discover Icmp IPHosts");
			Console.WriteLine("[Default] Current Home View");
			Console.WriteLine("[0] Show Local IPHosts");
			Console.WriteLine("[1] Show Icmp IPHosts"); 
			Console.WriteLine("[2] Show Known IPHosts"); 
			Console.WriteLine("[3] Add Known IPHosts"); 
			Console.WriteLine("[4] Remove IPHosts");
			Console.WriteLine("[q] Quit");
			Console.WriteLine();}
		
		public void ShowXIPHostList (XElement xiphosts)
		{	//Format and display list of IPHosts
			IEnumerable<XElement> iphosts = xiphosts.Descendants("IPHost");
			//get stat count
			int hostCount=0, upCount=0;
			foreach (var iphost in iphosts)
			{	hostCount++;
				if(Convert.ToBoolean(iphost.Element("PollStatus").Value))
				{upCount++;}}
			//display stat count
			Console.WriteLine("IPHost\ttotal:{0}\t\tup:{1}\tdown:{2}",hostCount,upCount,(hostCount-upCount));
			Console.WriteLine(UNDERLINE);
			//display hosts
			foreach(var iphost in iphosts)
			{//xiphost.Element("HostAddress").Value
				var str = iphost.Element("HostAddress").Value + "\t\t";
				str+=iphost.Element("HostName").Value + "\t\t";
				str+=iphost.Element("PollStatus").Value;
				Console.WriteLine(str);}
		}
	}
}
