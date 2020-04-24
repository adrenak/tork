# Tork
An arcade vehicle system for Unity

# Intro
Tork is not meant to be a realistic vehicle physics solution. It is best deployed in games where driving realism, especially at high velocities, is secondary. For example, demolition derby style games or games where driving is a secondary gameplay element. You could probably get away using it in a parking game.

Tork has its own Wheel physics component simply called `TorkWheel` which provides a greatly simplified friction computation mechanism. There are only three tweakable parametes: `forward grip`, `sideways grip` and `rolling friction`. 

It provides spring customization which is quite similar to the inbuilt `WheelCollider` in Unity.

`TorkWheel` also provides recommended values and guidelines for tweaking values.

# Components

`Vehicle` is the central class. It stores instances of all the `Core` and `AddOn` components in the car and this provides a single point for accessing these components. `Vehicle` class doesn't have any features of it's own and only facilitates in access.

## Core
### TorkWheel
A simplified version of Unity's `Wheel Collider`. Much of the information of this is in the Intro section above.

### Ackermann
Tork uses Ackermann steering to turn the wheels in a realistic manner.

### Motor
Essentially the engine of the vehicle. Has a `maxTorque` field that determines the maximum torque it can deliver. It also distributes it among the wheels smartly using the `Ackermann` component of the vehicle. Currently it only supports `All Wheel Drive` with other modes coming soon.
  
`Value` is the normalized value of the engines output. So `.5` would mean the engine is creating `maxTorque/2` torque.
  
`maxReverseInput` is the highest negative `value` the engine will output. This is because engines usually reverse at and up to a lower speed.

### Brakes
Very similar to `Motor` except that it applies braking torque on the wheels. Please see source to see the similarities.

### Steering
A layer of added feature over `Ackermann`. It stores the maximum steer angle the `Ackermann` will be sent as well as the rate at which the steering happens.

## Add Ons
AddOns are scripts that can be used to add or modify the vehicles behaviour. These derive from `VehicleAddOn` so that `Vehicle` can store them in a list and provide `GetAddOn<T>()` a method that allows any consumer to access the add on by type.

The add ons shipped with Tork are

### CenterOfMassAssigner
Assigns the center of mass of the vehicle. It is to be attached on a GameObject that represents the center of mass. You'd pretty much always need this class. Use it to make sure your center of mass is lower else the vehicle will flip on sharp turns.

### AntiRoll
Vehicles in real life also have anti roll bars that try to keep the vehicle grounded based on the difference in compression of the horizontally opposite wheels. So when you're taking a sharp right turn, the anti roll bars help ensure you don't flip the car anti-clockwise (to the left). 
  
### DownForceVsSpeed
Applies a down force on the car to simulate better aerodynamics. You'd normally want a linear curve
  
### MidAirStabilization
A pretty arcadish feature that tries to ensure that the vehicle lands flat on the ground. When the vehicle is in the air it applies counter torque based on the orientation. 

### MidAirStabilization
Another arcasish feature that allows the vehicle to be steered in air.

### MaxSteerAndleVsSpeed
Used to mimic real life driving by changing the max angle by which the wheels will rotate. Unless you're a bad driver, you won't steer the wheels all the way at high speeds and flip it. This class simulates that.

### TorkWheelViewSync
Synchronises the wheel meshes with the rotation and position of their respective `TorkWheel` objects.

### Ideas
Many, many addons can be made. Some ideas I have: A `BrakeLights` that makes the tail lamps illuminate. `SteeringWheelSync` that rotates the wheel along with the steering input.

## Input
The way input is provided and handled by the code and add on scripts may change, especially because it's like that as more add ons accumulate, it'd be good to make it available for reading some a common place. For now `IVehicleDriver` interface is to be extended. `KeyboardVehicleDriver` is inbuilt in Tork. 

## Demo
A demo scene and script is provided inside `Tork/Samples/`

### Forks
For a DOTS implementation of Tork v1, check out [@PragneshRathod901's fork](https://github.com/PragneshRathod901/Tork)

### Contact
[@github](https://www.github.com/adrenak)  
[@www](http://www.vatsalambastha.com)  
[@npm](https://www.npmjs.com/~adrenak)  
