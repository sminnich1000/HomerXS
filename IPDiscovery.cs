/*
 * HOMER Network Discovery and State Monitor
 * HomerXS Build
 * Copyright 2018
 *
 * Author: Stephen Minnich
 * Date: 3/20/2018
 * Time: 1:45 PM
 * 
 * A basic framework for discovering and reporting information
 * about the local network using the IPHost/IPState data model.
 */
 
using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Xml.Linq;
using System.Linq;

namespace HomerXS
{
	//Local network discovery and monitor application/service
	
	public class IPDiscovery
	{	//Discovers IPHosts using Local machine and network information
		//Returns XElement network state list information
		//Uses SimplbeXDB to save state lists to file
		
		public string AppDataPath {get; set;}
		private SimpleXDB db {get; set;}
		
		private List<IPHost> Local {get; set;}
		private List<IPHost> Icmp {get; set;}
		private List<IPHost> Known {get; set;}
		
		public IPDiscovery()
		{
			AppDataPath = "C:\\Users\\Public\\AppData\\HomerXS\\";
			//Handle path !Exist
			Directory.CreateDirectory(@AppDataPath);
			
			db = new SimpleXDB(AppDataPath);
			
			//Initialize IPDiscovery state lists
			Local = db.ReadIPStateFromXDB("Local");
			Icmp = db.ReadIPStateFromXDB("Icmp");
			Known = db.ReadIPStateFromXDB("Known");
			//get current Local state, log any changes
			GetLocalMachineState(); 
		}
		
			//All Public Methods Should Return XElement	

			// *Public Show Methods
			// ShowLocalMachineState
			// ShowIcmpState
			// ShowIcmpDiscovery
			// ShowKnownState
			// ShowAddKnownState

			
			// *Private IPHost Comparison Methods
			// CompareIPHostLists(list1, list2) : returns list of items in list1 and ! list2
			// CompareIPHostToList
			// CompareIPHostAddressToList
			// CheckIfKnown : adds IPHost to Known IPHost list
			
			// *Private Get Methods
			// GetIPHostXState : returns XElement from List<IPHost> state
			// GetLocalMachineState : returns IPHost list from current Local Machine query
			// GetIcmpWalk : returns IPHost list from ping poll of local network
			// GetIPHostPollState : returns List<IPHost> state with current PollStatus
			
			// *Private Network Methods
			// GetLocalIPAddressTool
			// GetServerIPAddressTool
			// GetPingReplyTool
			// GetNetworkCTool
			// GetNetworkCPollTool

		public XElement GetDefaultState(){return default(XElement);}
		
		private XElement GetIPHostXState(string statename, List<IPHost> state)
		{	var XState = new XElement(statename);
			foreach(var iphost in state)
			{XState.Add(iphost.ToXElement());}
			return XState;}
		
		public XElement ShowLocalMachineXState()
		{	Local = GetIPHostPollState(Local);
			return GetIPHostXState("Local", Local);}
		public XElement ShowIcmpXState()
		{	Icmp = GetIPHostPollState(Icmp);
			return GetIPHostXState("Icmp", Icmp);}
		public XElement ShowIcmpDiscovery()
		{	GetIcmpWalk();
			return GetIPHostXState("Icmp", Icmp);}
		public XElement ShowKnownXState()
		{	Known = GetIPHostPollState(Known);
			return GetIPHostXState("Known", Known);}
		public XElement ShowAddKnownXState(string ipaddress)
		{	//test ipaddress
			try {
				if (IPAddress.Parse(ipaddress).AddressFamily == AddressFamily.InterNetwork)
				{CheckIfKnown(new IPHost(ipaddress,"Remote"));}}catch{}
			
			Known = GetIPHostPollState(Known);
			return GetIPHostXState("Known", Known);}
		public XElement ShowRemoveIPHostXState(string ipaddress)
		{	//test ipaddress
			try {
				if (IPAddress.Parse(ipaddress).AddressFamily == AddressFamily.InterNetwork)
				{RemoveIPHost(ipaddress);}}catch{}
			
			Known = GetIPHostPollState(Known);
			return GetIPHostXState("Known", Known);}
		
		// Private Get / Compare / Network

		private void GetLocalMachineState()  							//updates Local, Known
		{	//discover Local IPState, save new state
			//set current state to old
			var oldLocal = new List<IPHost>();
			foreach(var iphost in Local)
			{oldLocal.Add(iphost);}
			//discover current Local state
			Local = GetLocalIPAddressTool();
			foreach (var iphost in  GetServerIPAddressTool())
			{Local.Add(iphost);}
			//test and save new Local state
			foreach (var iphost in Local)
			{CheckIfKnown(iphost);}
			if (Local.Count>0) //If list has IPHosts save them
			{db.WriteIPStateToXDB("Local",Local);}}
		
		public void GetIcmpWalk ()  //updates Icmp, Known
		{	//discover local network IPHosts, save new state
			//set current state to old
			var oldIcmp = new List<IPHost>();
			foreach(var iphost in Icmp)
			{oldIcmp.Add(iphost);}
			//discover current Icmp state
			Icmp = GetNetworkCPollTool(Local[0].HostAddress);
			//test and save new Local state
			foreach (var iphost in Icmp)
			{CheckIfKnown(iphost);}
			//compare new Icmp state to old and log changes
			//if list has IPHosts save them
			if (Local.Count>0) 
			{db.WriteIPStateToXDB("Icmp",Icmp);}}
	
		private List<IPHost> GetIPHostPollState(List<IPHost> state)
		{	foreach(var iphost in state)
			{	PingReply reply = GetPingReplyTool(iphost.HostAddress);
				if(reply.Status == IPStatus.Success)
				{iphost.PollStatus=true;}
				else{iphost.PollStatus=false;}}
			return state;}
		
		private bool CheckIfKnown(IPHost iphost)
		{	//Add to Known IPHost state if not already there
			if(CompareIPHostAddressToList(iphost, Known))
			{return true;}
			else{//Add to known list
				Known.Add(iphost);
				//sort and save
				Known = Known.OrderBy(k=>k.HostAddress).ToList();
				db.WriteIPStateToXDB("Known", Known);
				return false;}}
		
		private void RemoveIPHost(string ipaddress)				//updates Local, Icmp and Known
		{	var iphostlist = new List<IPHost>();
			foreach(var iphost in Local)
			{if(!(ipaddress.CompareTo(iphost.HostAddress)==0))
				{iphostlist.Add(iphost);}}
			Local = iphostlist;
			iphostlist = new List<IPHost>();
			db.WriteIPStateToXDB("Local", Local);
			foreach(var iphost in Icmp)
			{if(!(ipaddress.CompareTo(iphost.HostAddress)==0))
				{iphostlist.Add(iphost);}}
			Icmp = iphostlist;
			iphostlist = new List<IPHost>();
			db.WriteIPStateToXDB("Icmp", Icmp);
			foreach(var iphost in Known)
			{if(!(ipaddress.CompareTo(iphost.HostAddress)==0))
				{iphostlist.Add(iphost);}}
			Known = iphostlist;
			db.WriteIPStateToXDB("Known", Known);}
		
		private List<IPHost> CompareIPHostLists(List<IPHost> iphostlist, List<IPHost> comparelist)
		{   //return list of iphosts from iphostlist NOT on comparelist, compare IPHost.HostAddress
			var results = new List<IPHost>();
			foreach(var iphost in iphostlist)
			{	if (!CompareIPHostToList(iphost,comparelist))
				{	results.Add(iphost);}}
			return results;}
		
		private bool CompareIPHostToList (IPHost hosttocheck, List<IPHost> iphostlist)
		{	//return true if HostAddress gets match
			var results = false;
			foreach (var iphost in iphostlist)
			{	if ((iphost.HostAddress.CompareTo(hosttocheck.HostAddress) == 0) && (iphost.HostName.CompareTo(hosttocheck.HostName) == 0))
				{results = true;}}	
			return results;}
		
		private bool CompareIPHostAddressToList (IPHost hosttocheck, List<IPHost> iphostlist)
		{	//return true if HostAddress gets match
			var results = false;
			foreach (var iphost in iphostlist)
			{	if (iphost.HostAddress.CompareTo(hosttocheck.HostAddress) == 0)
				{results = true;}}	
			return results;}
		
		//network discovery
		private List<IPHost> GetLocalIPAddressTool()
		{	//return list with local IPv4 addresses, if any
			var results = new List<IPHost>();
    		var host = Dns.GetHostEntry(Dns.GetHostName());
    		foreach (var ip in host.AddressList)
    		{	if (ip.AddressFamily.CompareTo(AddressFamily.InterNetwork) == 0)
        		{	IPHost newhost = new IPHost(ip.ToString(),"LocalIP");
        			results.Add(newhost);}}
    		return results;}
		
		private List<IPHost> GetServerIPAddressTool ()
        {	var results = new List<IPHost>();
            NetworkInterface[] networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
			foreach (NetworkInterface networkInterface in networkInterfaces)
            {	if (networkInterface.OperationalStatus == OperationalStatus.Up)
                {	IPInterfaceProperties ipProperties = networkInterface.GetIPProperties();
                    IPAddressCollection dnsAddresses = ipProperties.DnsAddresses;
                    IPAddressCollection dhcpAddresses = ipProperties.DhcpServerAddresses;
                    GatewayIPAddressInformationCollection gateways = ipProperties.GatewayAddresses;
                    UnicastIPAddressInformationCollection unicastIp = ipProperties.UnicastAddresses; // for subnet mask
                    foreach(UnicastIPAddressInformation UnicatIPInfo in unicastIp)
   					{	Console.WriteLine("\tIP Address is {0}", UnicatIPInfo.Address);
    					Console.WriteLine("\tSubnet Mask is {0}", UnicatIPInfo.IPv4Mask);}
                    //add any IPv4 addresses in gateways to the list
                    foreach (GatewayIPAddressInformation gateway in gateways)
                    {	if(gateway.Address.AddressFamily.CompareTo(AddressFamily.InterNetwork) == 0)
                    	{	IPHost newhost = new IPHost(gateway.Address.ToString(), "Gateway");
                    		results.Add(newhost);}}
                    //add any IPv4 addresses in DnsAddresses to the list
                    foreach (IPAddress dnsAddress in dnsAddresses)
                    {	if(dnsAddress.AddressFamily.CompareTo(AddressFamily.InterNetwork) == 0)
                    	{	IPHost newhost = new IPHost(dnsAddress.ToString(), "DNS");
                    		results.Add(newhost);}}
                    //add any IPv4 addresses in DhcpAddresses to the list
                    foreach (IPAddress dhcpAddress in dhcpAddresses)
                    {	if(dhcpAddress.AddressFamily.CompareTo(AddressFamily.InterNetwork) == 0)
	                    {	IPHost newhost = new IPHost(dhcpAddress.ToString(), "DHCP");
                    		results.Add(newhost);}}}}
            return results;}
		
		private PingReply GetPingReplyTool (string destination)
		{	//return PingReply object from Ping(destination ip address)
    		var pinghost = new Ping();
    		return pinghost.Send (destination);}
		
		private string GetNetworkCTool (string ipaddress)
		{	//return first 3 octects of ipaddress
			string results = ipaddress.Substring(0,ipaddress.LastIndexOf("."));
			results += ".";
			return results;}
		
		private List<IPHost> GetNetworkCPollTool (string ipaddress)
		{	//return list of IPHosts from ip addresses that return PingReply.Status Success
			var results = new List<IPHost>();
			//Test ipaddress
			if (ipaddress != null)
			{	//If the ipaddress is good, get the class C network	
				var network = GetNetworkCTool(ipaddress);
				for (int x = 1; x < 255; x++)
				{	string pollAddress =  network + x.ToString();
					//if reply.Success add polled host to list
					if (GetPingReplyTool(pollAddress).Status == IPStatus.Success)
					{	var newhost = new IPHost(pollAddress, "Network");
						results.Add(newhost);}}} 
			return results;}
		
	} //IPDiscovery
}
