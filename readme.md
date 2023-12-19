I worked on this project since I got my car (Renault ZOE R90).
It was always a side project, but I never took the time to refine it so others can also use it.

Maybe somebody finds this helpful or wants to build it themselfes..
ios was never tested since it is no focus because I do not have the hardware for that (u need a macbook and an iPhone)

Feel free to create issues or prs, if enough people are interested I might be more motivated to add features or improve stuff down the road..

## Hardware
![electronics_incar_compressed](https://github.com/erxbout/ErXZEService/assets/68945126/bfd4d1d3-e320-4f6e-8f48-ceb64af5edaa)
The hardware is split in 2 layers
First layer is an Arduino UNO with an attached CAN Bus shield (Spark Fun). It only does sniffing on the CAN Bus. Saves data on SD Card and sends data via uart to the second layer..
<br>
Second layer is an ESP8266. This device is responsible for communication with the outside world.
<br>
Communication can be established via
- Wifi (ESP Access Point)
- Wifi (External AP)
- Wifi (Hotspot from phone *not tested*)
- MQTT (Needs external AP with internet connection, I use an usb wifi internet stick)

## Android APP
The android app collects the data and saves historical information like trips and charges
It does not capture all the missed data and events, the data on the SD Card is the single source of truth for the full set of data!
In the app are many tiny features that can not be accessed without understanding where to look, so its not very user friendly..

App looks like this

![Android cleaned_compressed](https://github.com/erxbout/ErXZEService/assets/68945126/714e6b5b-d468-41d7-ad94-1b5d207ed09e)
