import { Component, OnInit, Inject } from '@angular/core';
import { Adjuster, DefaultAdjuster } from '../models';
import { HttpClient } from '@angular/common/http';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Sensor } from '../models/sensor/sensor.service';
import { FormControl, Validators } from '@angular/forms';
import { MatSnackBar } from '@angular/material/snack-bar';

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

  private displayed: boolean = false;

  nameFormControl: FormControl = new FormControl('', Validators.required);

  constructor(@Inject(MAT_DIALOG_DATA) public data: any, private http: HttpClient, private snackBar: MatSnackBar) {
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

  SetDefault(): boolean {
    if (this.adjuster.type !== "DefaultAdjuster") {
      this.adjuster = new DefaultAdjuster();
      this.adjuster.settingPath = this.targetSensor.controlPath;
      this.SaveAdjuster(this.adjuster);
    }
    return true;
  }

  SaveAdjuster(adjuster: Adjuster): void {
    this.adjuster = adjuster;
    var url = "http://localhost:5000/api/control/" + this.targetSensor.controlPath;
    this.http.put(url, adjuster).subscribe((response) => {
      this.snackBar.open("Settings saved and applied", "", {
        duration: 2000,
      });
    });
  }

  UpdateSensor(successMessage: string): void {
    this.http.put("http://localhost:5000/api/hardware", this.targetSensor).subscribe((data: Sensor) => {
      this.targetSensor.name = data.name;
      this.targetSensor.primary = data.primary;
      this.targetSensor.hidden = data.hidden;
      this.snackBar.open(successMessage, "", {
        duration: 2000,
      });
    });
  }

  UpdateSensorName(name: string): void {
    this.targetSensor.name = name;
    this.UpdateSensor("Name updated successfully");
  }

  UpdateSensorPrimary(primary: boolean): void {
    this.targetSensor.primary = primary;
    this.UpdateSensor(primary ? "Primary value set" : "Primary value unset");
  }

  UpdateSensorHidden(hidden: boolean): void {
    this.targetSensor.hidden = hidden;
    this.UpdateSensor(hidden ? "Value hidden from summary" : "Value shown in summary");
  }
}
