# SNMP Power Monitor
## What is it?
A simple and quick .NET 6 app that pulls current Watt usage from an SNMP device and allows it to be used with Home Assistant.

## How does it work?
It uses .NET 6 and Lextm.SharpSnmpLib to call an SNMP device (APC PDU for me), pull a given record out (in my case, the current W usage), does some math to figure out the Watt Hours and writes it to a text file. It also logs the info. The text file can be served with NGinx (see docker-compose.yml file) and imported into Home Assistant.

## How do i get it?
I need to push the real docker instance somewhere, but you can check this repo out and build it locally. 

## How do i use it?

Till i get the full docker side of things working, the following steps are required:

* check out code
* cd into the folder and run `docker build . -t tiernano/snmppowermon`. 
* edit the docker-compose to set the port of nginx as required.
* rename the snmppowermon.env.example to snmppowermon.env and change the config as required
* `docker-compose up -d`
* in home assistant, edit the configuration.yml file and add the following:

```
sensor:
  - platform: rest
    resource: http://<ip>:<port>/total.txt
    unit_of_measurement: "Wh"
    device_class: energy
    state_class: total_increasing
```

* change the IP to the correct IP of your docker instance. 

* you may need a customize.yaml with the following:

```
sensor.rest_energy:
  device_class: energy
  state_class: measurement
```

* restart home assistant
* in home assistant, go to settings, dashboards, energy and click "Add Consumption". 
* you should see the Rest Sensor. Select it and, if you have a known static price, select "use a static price" and enter it.
* after an hour or so, you should start seeing data showing in there.
* to do a bit more digging, go to developer tools, statistics and search for rest. You should see the kWh and Price units in there in somewhat real time.

Any issues, open a ticket.



