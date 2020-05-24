import {
  Component,
  OnInit,
} from '@angular/core';
import { Hardware, SensorUnits } from '../models';

import { SensorService, Sensor } from '../models/sensor/sensor.service';
import * as _ from 'underscore';
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { strict } from 'assert';
import { MatDialog } from '@angular/material/dialog';
import { AdjusterComponent } from '../adjuster/adjuster.component';
import { ScrollStrategyOptions } from '@angular/cdk/overlay';

@Component({
  selector: 'app-monitor',
  templateUrl: './monitor.component.html',
  styleUrls: ['./monitor.component.scss'],
})
export class MonitorComponent implements OnInit {

  public allHardware: Hardware[] = [];

  constructor(private http: HttpClient, public dialog: MatDialog, public sensorService: SensorService) {
  }

  traverseHardware(hardware): void {
    var sensorUnits = new SensorUnits();

    switch (hardware.type.toLowerCase()) {
      case "cpu":
        if (hardware.path.indexOf("amdcpu") == 0) {
          hardware.image = "amd";
        }
        break;
      case "motherboard":
        hardware.image = "motherboard";
        break;
      case "memory":
        hardware.image = "ram";
        break;
      case "gpunvidia":
        hardware.image = "nvidia";
        break;
      case "liquidcooler":
        if (hardware.path.indexOf("nzxt") == 0) {
          hardware.image = "nzxt";
        }
        break;
      case "storage":
        hardware.image = "storage";
        break;
    }

    hardware.children.forEach(child => {
      this.traverseHardware(child);
    });

    hardware.sensorTypes.forEach(type => {

      var sensors = hardware.sensors[type];
      if (sensors == undefined) return;

      sensors.forEach(sensor => {
        sensor.unit = sensorUnits[sensor.type];
        sensor.parentName = hardware.name;
        this.sensorService.addSensor(sensor);
      });
    });
  }

  SetPrimary(sensor, primary): void {
    sensor.primary = primary;
    this.http.put("http://localhost:5000/api/hardware", sensor).subscribe((data: Sensor) => {
      sensor.primary = data.primary;
    });
  }

  ConfigureSensor(sensor: Sensor): void {
    const dialogRef = this.dialog.open(AdjusterComponent, {
      width: '1024px',
      height: '1024px',
      panelClass: 'noscroll-container',
      data: { targetSensor: sensor }
    });

    dialogRef.afterClosed().subscribe(result => {
      console.log(`Dialog result: ${result}`);
    });
  }

  ngOnInit(): void {
    this.http.get("http://localhost:5000/api/hardware").subscribe((data: Hardware[]) => {
      this.allHardware = data;

      data.forEach(hardware => {
        this.traverseHardware(hardware);
      });
    });
  }
}
