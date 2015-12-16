rnet
=========

Implements a .Net version of various utilities for working with Russound RNET devices.

RNet
---------

Provides a connection and protocol library for connecting to RNET devices and sending and receiving RNET messages.



Message I sent to Jörg Beck over email explaining the architecture. Should clean this up and make real documentation from it:

You can just unload the Android project. That doesn’t work anyways. I was beginning work on a quick Xamarin front end for it. It’s basically an empty project at this point.

I’ll explain the architecture a bit:

The main Rnet project is the core of the library. It contains all of the communication primitives, protocol handler, socket managers, and an abstraction for the Rnet messaging bus. Basically, Rnet is packet based. Packets have a source and destination. And there are a number of packet types. These are encapsulated in RnetMessage and the classes that descend from it.

There’s also an abstraction for device addresses (RnetDevice), path structures (RnetPath). And there’s a RnetMessageReader/Writer which parse byte streams into RnetMessage instances. “Zone” is a top-level concept in Rnet, so there is also a RnetZone class.

This is all exposed up in a class called RnetClient. An RnetClient takes an RnetConnection, and provides methods like Start/Stop. It raises events when a message arrives. And has a Send method for sending a message. A quick way to get started with the code is to create an RnetClient, hand it an RnetConnection (or a URL, there are special URL protocol handlers, like rnet.tcp://IP:PORT, which will discover an RnetConnection implementation). You can then call subscribe to message arrive events, and call Send to send messages.

You probably don’t want to do this, however. The RnetClient will receive all messages. No matter who they are targeted to. And messages are send to the connection, and routed by the CAV/M to their target device/zone/keypad. The Client will receive everything the CAV routes to you. This isn’t very useful.

At a higher level is an RnetBus. An RnetBus is a construct that wraps an RnetClient, maintains an identity as a Rnet device, and tracks the state of all of the other devices on the system. It maintains a tree of all of the known devices, and allows you to retrieve a device and send targeted messages to it. You can navigate through the bus, discover controllers, discover keypads, etc. It also handles reply/request logic: hooking up messages to later messages that serve as a reply to the first. Correlating.

It also allows a consumer to take out a RnetDataHandle on a specific Rnet path inside of a specific device, and be notified when the data at this path is changed.

Next. Above that is the Driver/Profile layer. Rnet doesn’t make any distinction between device types. You just send messages, and other devices also send messages. Internally there are no high level concepts like Volume and such. Instead, Volume might be represented as a directory structure exposed on a particular device, that you have to send SetData messages to in order to change. And messages are completely inconsistent. Some properties you need to send SetData to change. Some you have to send an Event to change. Some reply with SetData in order to signal a change. Some reply with Events to signal a change. It’s all over the map.

So, that’s where Drivers and Profiles come in. Profiles represent a certain TYPE of communication with a device. Think of them like USB profiles: you have the HID profile (human interface device), an Audio profile, etc. Some other Rnet devices (non-Russound) might have their own profiles.

So, Rnet.Profiles documents the profiles, and their associated names and interfaces. For instance, the media.audio.Equalization profile. This represents an interface that a device might support that provides properties like Volume, Bass, Treble, Balance, and a set of methods to change them: VolumeUp, VolumeDown, etc. A given Rnet device might support these, but implement them differently.

Rnet.Drivers.Russound then has a set of Drivers, in a discoverable DriverPackage, that detect whether they apply to a given RnetDevice, and if so, let the DriverManager know. The specific driver instance might support multiple profiles. In here are the implementations for Russound-brand devices.

The Rnet.Drivers.Russound.Controller represents a Russound RNET controller. The CAM66 and CAV66 drivers inherit from it, and apply when the model value advertised by a specific device starts with “CAM 6.6” and “CAV 6.6” respectively (this detection logic is in Rnet.Drivers.Russound.DriverPackage). Each of these implement the base ControllerDriver. Which expose the Controller and ControllerContainer profile. The implementation of these Profiles generate fake devices with supporting profiles to represent the 6 channels each controller has. So, you end up with a Controller device at the time, that implements a Container profile, inside of which are some Zones, which implement the generic core.IZone profile as well as the media.audio.Zone profile. Basically, now you have a navigable object tree, inside which you can find zones, which support Volume/Bass/Treble properties, and implement these by sending the appropriate messages to the appropriate controller, and receiving the appropriate responses, which cause the property values to change.

At the end of this you get an object tree, that you can navigate programmatically, with objects that implement .Net interfaces, that allow you to set Volume or whatever on particular objects.

This is all integrated in the Rnet.Service.Host project. This project exposes all of this on a HTTP endpoint, with messages in JSON. Now you can see this device tree, interrogate the properties of the various objects in it, and invoke commands to change volume, over HTTP.

That was complicated. Basically, I implemented this in my the same way the Linux or Windows device model is implemented. Core communication protocols talking over an abstract bus. A higher level layer which keeps track of devices. And a higher level ontop of that which allows you to plug in drivers that support various features. Much like how the Video driver for an AMD device magically allows other applications to use it as a graphics card, without knowing the details of how it’s implemented.

So now you can investigate the tree, and discover all the other Rnet devices which support volume, and change it, without knowing how.

The Rnet.Service project is a Console application which starts up the Rnet.Service.Host and points it to a bus at a URL endpoint, exposing it all over HTTP.

You should be able to use this project by simply changing the App.config file to add two things: the host URI, which is the local HTTP URL that it will listen on, and a bus connection URI, which is the URI of the Rnet bus to connect to.

If you’re running it on Windows, and you want it to listen on a port, you do have to configure your urllistener stuff properly.
