import { Component } from '@angular/core';
import { MqttService } from './services/mqtt.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'System Monitor';

  constructor(private mqttService: MqttService) {

    mqttService.Start();
  }
}
