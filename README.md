# IT-PRoject-1: LVC Simulation Integration in Unity using OpenDIS

Introduction

This is Softwars' repository for our IT Project 1, in which we algorithmically generate crowd behaviour (in birds and people respectively) in Unity and use OpenDIS (an open source DIS tool) to send packets of information to a network. We also demonstrate that those packets are being sent by viewing them using OpenDIS’s receiver tool.

This repository contains files for the 2 behavioural models (Boids/birds and Crowd/human) to be run in Unity, as well as some documentation (e.g project review documentation, our functional specification, meeting minutes etc). Note that inside of the Boids and Crowd simulation model branches are some .dll files from OpenDIS (the middleware for this project).

What Exactly Is Our Product?

This project, simply put, involves running a behavioural model in Unity which will periodically send out packets of data to a given IP address and port. These data packets are called espdus: entity state protocol data units. Espdus are a part of the Distributed Interactive Simulation (DIS) protocol. Each simulated entity will send out an espdu, containing basic information about its identity and whereabouts, after it moves past a certain threshold amount. The process of using such a threshold is called Dead Reckoning, and it exists to prevent too many espdus from being sent and flooding the network. 

Read more about the protocol here: https://standards.ieee.org/standard/1278_2-2015.html.

The espdus are sent using methods from the OpenDIS CSharpDis6 project available on github – see the OpenDIS repository on Github here: https://github.com/open-dis. The project was included as a .dll file and imported into the simulation scripts themselves. The information about the entities is set in a Sender object imported from OpenDIS, and this object is then used to send the espdu to a configured location in the network. <<<INFO ABOUT CONFIG FILE HERE.>>>

Once sent, the espdus can be received, translated and mapped to environment-specific entities by any LVC simulation being run on the same network. However, actually setting up such a simulator was beyond our project’s scope. Instead, to demonstrate that the espdu packets were being sent with the correct information, we used OpenDIS’s Receiver class. We ran the EspduReceiver.exe file separately to the simulations and read what that printed out – which was all the XML formatted information about the simulated entities contained within the espdus.

As for the simulation projects themselves, there are 2 to choose from. The Boids Simulation model is based on Boids, which behave like a simple flock (think birds) – read more about them here: https://medium.com/swlh/boids-a-simple-way-to-simulate-how-birds-flock-in-processing-69057930c229. The Crowd Simulation is 3D, unlike the Boids, and is based on human behaviours. It was developed from scratch without using an existing model.

How to Use our Files

First, you’ll need to pull the repository from our git using git bash. I won’t go into that as there are countless guides to git bash on the internet.

You will then need to load the simulations into Unity before doing anything else. To do this, open UnityHub (you can do this in Unity’s free versions, Personal or Student) and add a new project to the hub. Click add, then in the browse folder menu that appears, simply select on either the “Crowd Sim” branch folder or the “Boids Simulation” folder, depending on which simulation you would like to run. Then click “Select Folder”, and the project should be added to your Projects list. Click on the project to open it in Unity. This should open up the Unity UI; to run the simulation, simply select the play button in the top/centre of the screen. To stop the simulation, simply press this play button again. To pause the simulation, press the pause button.

To actually see the espdus being sent, you will have to do a couple of things.

For the Boids simulation, labelled "Boids Simulation w. OpenDIS XYZ", there exists a configuration (JSON) file to make this part easy. All you have to do is open up the Assets folder within the "Boids Simulation w. OpenDIS XYZ" parent folder and open up the file labelled Appsettings.json. Within this file, you should see the different configurable values that you will need to change.

       "threshold": "5.0",
       "localx": "0",
       "localy": "0",
       "localIPAddr": "IPAddress.any",         // Leave this line alone; It's a default and refers to the IP address format accepted
       "IPAddr": "239.1.2.3",
       "mcastPort": "62040",
       "broadcastPort": "62040"

The threshold value means the amount of distance (in Unity's units) that a Boids can travel before an Espdu update is sent.
Localx and localy mean the X and Y coordinates, in latitude/longitude, of the origin - the top left hand corner - of the simulation's area (or "map").
IPAddr holds the IP address that Espdu packets will be sent and read from.
The mcastPort holds the local port that Espdu packets will be sent via multicast. The broadcastPort holds the same but for broadcast.

Simply change the values to suit your needs and save and close the json file.

For the Crowd simulation, there is no config file, but setting it up is still simple enough. Firstly, you will need to go into the Crowd Sim folder and prepare to edit some of the code there. From inside the Crowd Sim folder, go to the Assets folder, and then open up the NPCMove.cs script in Microsoft Visual Studio (a code editor that comes with Unity – we used the 2019 version). Now, this script will have a Start() function; go into this function and find the section containing the following code:

       sender = new Sender(IPAddress.Parse("255.255.255.0"), 62040, 62040);
       Sender.StartBroadcast();

Now, replace the IP address listed inside this definition with the desired IP address to receive the packets, and similarly edit the two ports listed beside it. Note that the first port is for multicast and the second for broadcast. There will also be the (dead reckoning) threshold value for sending an Espdu here too, which you can play around with to find the optimal amount to send in a given frame without DOS-ing yourself.

       public float threshold = 2.0f;

Save and exit the files. Now, the simulation is configured. The second step is to do the same thing but for EspduReceiver.cs, which can be found inside OpenDIS’s c-sharp-dis-6 folder on this path: open-dis-csharp\CsharpDis6\EspduReceiver. First you will have to open the OpenDis.sln file in Microsoft Visual Studio. Then, from within Visual Studio, open a new file and select EspduReceiver.cs. Go to the main method at the bottom of the file. Set the mcastAddress to your desired IP address (ensure it matches the Sender’s!) and also set the ports. Finally, you can save and exit the file. Now that everything is configured, you can begin running the receiver.

To run the receiver, again open OpenDis.sln in Microsoft Visual Studio. Now, open a new file called EspduReceiver.exe, found via the path: open-dis-csharp\CsharpDis6\EspduReceiver\obj\x86\Debug. Once open, you will have to run the file using Debug -> Start Without Debugging. This will cause an “error” to occur, but that doesn’t actually affect how it runs in any way. Now a terminal window should open up saying it is waiting to receive packets. If you run the Crowd simulation in Unity at the same time as the EspduReceiver.exe file is running, you will see packets being received in said window.

Of course, you could try receiving these packets in numerous other ways, the most obvious probably being using Wireshark. The only reason we didn’t use Wireshark was that other applications in the network were causing Wireshark to be unable to read the packets from their destination port.

Contributing to the Project

As this is an open source project, to contribute to the code, you can simply fork the repo for yourself and play with it as you like. We will not be working further on it, but there is plenty more that can be done with it. Feel free to have fun!
