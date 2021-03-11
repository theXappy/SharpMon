![icon](https://raw.githubusercontent.com/theXappy/SharpMon/main/icon.png)
# SharpMon
Boilerplate code around Microsoft's Network Monitor API so you can focus on the packets instead of calling ugly C functions.

## Prerequisites 
SharpMon uses Microsoft Network Monitor API to capture traffic.\
The API is available on Windows machines where Microsoft Network Monitor was installed.

Please [downloaded Microsoft Network Monitor](https://www.microsoft.com/en-us/download/details.aspx?id=4865) before using the code in this repo.

## How to include SharpMon in your project
There are 2 ways to add the SharpMon library to your project:

1. Get it [from NuGet](https://www.nuget.org/packages/SharpMon/)\
or
2. Download the code in this repo and add the SharpMon project (.csproj) to your solution


## Quick Start
SharpMon's central piece is the ```NetMonSniffer``` class.
You can use it to start sniffing frames from your network adapter in very few steps.


This short example shows basic usage of ```NetMonSniffer```
```C#
    // Initialize
    NetMonSniffer sniffer = new NetMonSniffer();

    // Find an adapter to capture on
    var adapters = sniffer.GetAdapters();
    uint firstAdapterId = adapters.Keys.First();

    // Register callback to handle every captured frame
    sniffer.FrameAvailable += (sender, frmType, frmData) => { Console.WriteLine("Another frame found!"); };

    // Start capturing
    sniffer.Start(firstAdapterId);

    ... do more stuff / sleep ...
    Thread.Sleep(10_000);

    // Stop capturing and release capuring resources
    sniffer.Stop();
```

If your machine has more than one network interface you'll need to find the right ```adapterId``` to pass to ```sniffer.Start( )```.\
The returned value from ```sniffer.GetAdapters()``` is a dictionary mapping adapter IDs to Netmon/.NET structures representing the adapter.
You can use either of those to find the right one.


The next example shows how to find an adapter called "Ethernet 3" (Name from the control panel):
```C#
    // Initialize
    NetMonSniffer sniffer = new NetMonSniffer();
    
    // Find the id of a specific adapter
    Dictionary<uint, AdapterInformation> adapters = sniffer.GetAdapters();
    uint eth3AdapterId = adapters.Single(kvp =>
    {
        NetworkInterface ni = kvp.Value.DotNetInfo;
        return ni.Name == "Ethernet 3";
    }).Key;
    
    // Start capturing
    sniffer.Start(eth3AdapterId);
```


## Using the Network Monitor API directly
You do not need this project to use the Network Monitor API directly (NMAPI.dll and NetmonAPI.cs)
but when I developed this library I've added some more '.Net-ish' ways to consume this API.\
These changes are mostly found in the ```NetmonAPIManaged.cs``` file which extends the ```NetmonAPI``` class.\
You might find them useful.

## Thanks
Icon: [monitoring](https://thenounproject.com/search/?q=Monitor&i=2676857) by [Eucalyp](https://thenounproject.com/eucalyp/) from the Noun Project
