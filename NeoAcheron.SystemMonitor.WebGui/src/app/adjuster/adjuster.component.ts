import { Component, OnInit, OnChanges, Inject, Input } from '@angular/core';
import { Adjuster } from '../models';
import { HttpClient } from '@angular/common/http';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Sensor } from '../models/sensor/sensor.service';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-adjuster',
  templateUrl: './adjuster.component.html',
  styleUrls: ['./adjuster.component.scss']
})
export class AdjusterComponent implements OnInit {

  public targetSensor: Sensor;

  public adjuster: any;

  targetSensorName: string;

  adjusterType: string;

  nameFormControl: FormControl = new FormControl('', Validators.required);

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, private http: HttpClient) {
    this.targetSensor = data.targetSensor;
    this.GetAdjuster();
  }

  ngOnInit(): void {
    this.targetSensorName = this.targetSensor.name;
    this.nameFormControl.registerOnChange(() => {
      this.targetSensor = this.nameFormControl.value;
    });
  }

  ngOnChanges(): void {
  }

  GetAdjuster(): void {
    var url = "http://localhost:5000/api/control/" + this.targetSensor.controlPath;
    this.http.get(url).subscribe((adjuster: Adjuster) => {
      this.adjuster = adjuster;
      this.adjusterType = adjuster.type;
    });
  }

  SaveAdjuster(adjuster: Adjuster): void {
    this.adjuster = adjuster;
    var url = "http://localhost:5000/api/control/" + this.targetSensor.controlPath;
    this.http.put(url, adjuster).subscribe((response) => {
      console.log("Success!");
    });
  }

  UpdateSensorName(name: string): void {
    this.targetSensor.name = name;
    this.http.put("http://localhost:5000/api/hardware", this.targetSensor).subscribe((data: Sensor) => {
      this.targetSensor.name = data.name;
    });
  }
}
