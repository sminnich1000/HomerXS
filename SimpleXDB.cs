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
using System.IO;
using System.Collections.Generic;
using System.Xml.Linq;

namespace HomerXS
{	/*	
		Contains:
		Homer static class
		SimpleXDB xml file manager
		IPState definition
		IPHost definition
	*/
	
	public static class Homer
	{	//Homer constants
		public const string TIME_IDFORMAT = "yyyyMMddHHmmss";}
	
	public class SimpleXDB
	{	// Two Main Methods:
		// WriteIPStateToXDB
		// ReadIPStateFromXDB
		
		private List<IPState> State {get; set;}
		private string AppDataPath {get; set;}
		
		public string[] IPStateNames {get; set;}
		public string[] IPDataModels {get; set;}
		
		public SimpleXDB(string appDataPath)
		{		
			IPStateNames = new[] {"Local", "Icmp", "Known"};
			IPDataModels = new[] {"IPHost"};
			
			//if path !Exist, Create it.		
			AppDataPath = appDataPath;
			Directory.CreateDirectory(@AppDataPath);

			State = new List<IPState>();
			State.Add(new IPState("Local","IPHost",AppDataPath,".xdb")); 		//Current Local Machine IPHost state
			State.Add(new IPState("Icmp","IPHost",AppDataPath,".xdb"));		//Current Local Network IPHost state
			State.Add(new IPState("Known","IPHost",AppDataPath,".xdb")); 		//Known IP Host state
		}
		
		private IPState GetIPState (string state)
		{	//test if state is valid IPState
			if (Array.IndexOf(IPStateNames, state) >= 0)
			{	IPState xdbstate = State.Find(table => table.Name == state);
				return xdbstate;}
			return null;}

		public void WriteIPStateToXDB (string state, List<IPHost> iphosts)
		{	//test IPHost list for empty
			if (iphosts.Count < 1) { return;}
			//get State Table
			IPState xdbstate = GetIPState(state);
			if (xdbstate!=null)
			{	//add Records as new
				xdbstate.Records = new XElement(xdbstate.Name);
				foreach(var iphost in iphosts)
				{xdbstate.Records.Add(iphost.ToXElement());}
				XDocument xdoc = new XDocument(new XElement ("IPState",xdbstate.Records));
				xdoc.Save(xdbstate.XdbFile);}}
		
		public List<IPHost> ReadIPStateFromXDB(string state)
		{	var results = new List<IPHost> ();
			//get IPState
			IPState ipstate = GetIPState(state);
			if ((ipstate != null) && (ipstate.XdbFile != null))
			{	//Load XDB State XDocument
				try {XDocument xdoc = XDocument.Load(ipstate.XdbFile);
					IEnumerable<XElement> xrecords = xdoc.Descendants(ipstate.DataModel);
					foreach (var xiphost in xrecords)
					{	var hostToAdd = new IPHost (xiphost.Element("HostAddress").Value, xiphost.Element("HostName").Value);
						hostToAdd.HostID = xiphost.Element("HostID").Value;
						hostToAdd.PollStatus = Convert.ToBoolean(xiphost.Element("PollStatus").Value);
						results.Add(hostToAdd);}
				} catch
				{	if ((!File.Exists(ipstate.XdbFile)) && (ipstate.XdbFile != null))
					{	XDocument xdoc = new XDocument(new XElement ("IPState", ipstate.Records)); 
						xdoc.Save(ipstate.XdbFile);}}} 
			return results;}
	} //SimpleXDB
	
	public class IPState
	{
		public string Name {get; set;}
		public string DataModel {get; set;} //IPHost,IPDetail,IPLog
		public string XdbFile {get; set;}
		public XElement Records {get; set;}

		public IPState (string state, string datamodel, string appPath, string extension)
		{	var IPStateNames = new[] {"Local", "Icmp", "Known"};
			var IPDataModels = new[] {"IPHost"};
			//test name == IPState Name
			if (Array.IndexOf(IPStateNames, state) >= 0)
			{Name=state;}
			else{Name=null;}
			//test datamodel = {IPHost, IPLog} Homer.IPDataModels
			if (Array.IndexOf(IPDataModels, datamodel) >= 0)
			{DataModel=datamodel;}
			else{DataModel=null;}
			//set Records obj. to an instance
			Records=new XElement(DataModel);
			//XDB IPState File Name Final Construction
			XdbFile=appPath + Name + "_" + DataModel + extension;}
		public IPState()
		{	Name=null;
			XdbFile=null;
			DataModel=null;
			Records=default(XElement);}
		~IPState()
		{	Name=null;
			XdbFile=null;
			DataModel=null;
			Records = default(XElement);}
		public override string ToString()
    	{return ("Table: " + Name + " " + DataModel + " : " + XdbFile);}		
		public XElement ToXElement ()
		{	//Records = XElement(DataModel)
			return Records;}
	} //IPState
	
	public class IPHost
	{
		public string HostAddress {get; set; }
		public string HostName {get; set; }
		public string HostID {get; set;}
		public bool PollStatus {get; set;}	
		
		public IPHost(string hostaddress, string hostname)
		{	HostAddress = hostaddress;
			HostName = hostname;
			HostID = DateTime.Now.ToString(Homer.TIME_IDFORMAT);
			PollStatus = true;}
		public IPHost(string hostaddress, string hostname, string hostid, bool pollstatus)
		{	HostAddress = hostaddress;
			HostName = hostname;
			HostID = hostid;
			PollStatus= pollstatus;}
		public IPHost()
		{	HostAddress = null;
			HostName = null;
			HostID = DateTime.Now.ToString(Homer.TIME_IDFORMAT);
			PollStatus= false;}
		~IPHost()
		{	HostAddress = null;
			HostName = null;
			HostID = null;
			PollStatus= false;}
		public override string ToString()
    	{return (HostAddress + " " + HostName + " " + HostID + " " + PollStatus);}
		public void ToConsole()
		{	Console.WriteLine("\r\n==========  IPHost  ==========");
			Console.WriteLine("HostAddress: {0}",HostAddress);
			Console.WriteLine("HostName: {0}",HostName);
			Console.WriteLine("HostID: {0}",HostID);
			Console.WriteLine("HostID: {0}",PollStatus);}
		public XElement ToXElement()
		{	XElement xelement = new XElement(			
				new XElement("IPHost",
			    	new XElement("HostAddress",HostAddress),
			    	new XElement("HostName",HostName),
			    	new XElement("HostID",HostID),
			    	new XElement("PollStatus",PollStatus)
			    ) //IPHost	
    		);//XElement
			return xelement;}
	} //IPHost
}
