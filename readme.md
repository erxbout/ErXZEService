I worked on this project since I got my car (Renault ZOE R90).
It was always a side project, but I never took the time to refine it so others can also use it.

Maybe somebody finds this helpful or wants to build it themselfes..
ios was never tested since it is no focus because I do not have the hardware for that (u need a macbook and an iPhone)

## Hardware
![electronics_incar](https://github.com/erxbout/ErXZEService/assets/68945126/bf455d6b-feb2-43f6-9923-f3535da134d0)
The hardware is split in 2 layers
First layer is an Arduino UNO with an attached CAN Bus shield (Spark Fun). It only does sniffing on the CAN Bus. Saves data on SD Card
Second layer is an ESP8266. This device is responsible for communication with the outside world.
Communication can be established via
- Wifi (ESP Access Point)
- Wifi (External AP)
- Wifi (Hotspot from phone *not tested*)
- MQTT (Needs external AP with internet connection, I use an usb wifi internet stick)

## Android APP
The android app collects the data and saves historical information like trips and charges
It does not capture all the missed data and events, the data on the SD Card is the single source of truth for the full set of data!
App looks like this
![Android_App](https://github.com/erxbout/ErXZEService/assets/68945126/4e9d44d6-bdc1-47c5-873f-7ab021947608)
