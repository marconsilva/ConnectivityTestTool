# ConnectivityTestTool
This is a small project in .Net Core to test connectivity from one Virtual Machine to a set of other machines in and generate an output from that test. The goal is to make networking configuration and test a bit more streamline

# Usage
To use this tool you can use this in one of the following ways:

If you are using the default config.json file you can simply use the command without any argument

    ConnectivityTestCommand.exe

If you wish to use a custom json (and overide the default file usage) you can pass the path for that custom file in the following way

    ConnectivityTestCommand.exe c:\test\customfile.json

If you wish to do a specific test to a port and address you can use the tool in the following way

    ConnectivityTestCommand.exe 192.168.1.1 80
    
Additionally you optionaly set output flags to customize the verbose level of the output.
The available flags are:

* `-silent` : you can use this to silent all outputs exept for reporting status (Sucess or failure) of each test
* `-showDebug` : you can use this to enable debug output of the tool
* `-hideErrors` : you can use this to hide output error messages such as stacktrace outputs
* `-hideWarnings` : you can use this to hide warning messages
* `-hideInfo` : you can use this to hide info outputs

# Config File
The config file can have any number of destination addresses each with his set of ports to test.
Here is an example of a file you can use to build your own file from

```json
{
  "Destinations": [
    {
      "IPAddress": "192.168.1.1",
      "Ports": [ 80, 121 ]
    },
    {
      "IPAddress": "192.168.1.2",
      "Ports": [ 80, 21 ]
    }
  ]
}
```
