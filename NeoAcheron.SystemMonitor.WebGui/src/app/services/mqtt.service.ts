import { Injectable } from '@angular/core';
import { Client, Message } from 'paho-mqtt';
import { SensorService } from '../models/sensor/sensor.service';

@Injectable({
  providedIn: 'root'
})
export class MqttService {

  private client: Client;

  constructor(private sensorService: SensorService) {
  }

  Start(): void {
    this.client = new Client('localhost', 5000, "sys-mon-" + Math.random());
    this.client.onConnectionLost = this.OnConnectionLost.bind(this);
    this.client.onMessageArrived = this.OnMessageArrived.bind(this);

    this.client.connect({ onSuccess: this.OnConnect.bind(this) });
  }

  OnConnect() {
    this.client.subscribe("#");
  }

  OnMessageArrived(message) {
    const sensor = this.sensorService.getSensor(message.topic)
    sensor.value = +message.payloadString;
  }

  OnConnectionLost(responseObject) {
    if (responseObject.errorCode !== 0) {
      console.log("onConnectionLost:" + responseObject.errorMessage);
      setTimeout(this.Start, 1000);      
    }
  }

}
