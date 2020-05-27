import { EventEmitter } from '@angular/core';
import { Sensor } from './models/sensor/sensor.service'

export enum HardwareTypes {
  motherboard,
  gpu,
  nzxt,
  amdcpu
}

export class SensorUnits {
  voltage = " V"; // V
  clock = " MHz"; // MHz
  temperature = "°C"; // °C
  load = " %"; // %
  frequency = " Hz"; // Hz
  fan = " RPM"; // RPM
  flow = " L/h"; // L/h
  control = " %"; // %
  level = " %"; // %
  factor = ":1"; // 1
  power = " W"; // W
  data = " GB"; // GB = 2^30 Bytes
  smalldata = " MB"; // MB = 2^20 Bytes
  throughput = " B/s"; // B/s
}

export class SensorCollection {
  voltage: Sensor[] = [];
  clock: Sensor[] = [];
  temperature: Sensor[] = [];
  load: Sensor[] = [];
  frequency: Sensor[] = [];
  fan: Sensor[] = [];
  flow: Sensor[] = [];
  control: Sensor[] = [];
  level: Sensor[] = [];
  factor: Sensor[] = [];
  power: Sensor[] = [];
  data: Sensor[] = [];
  smallData: Sensor[] = [];
  throughput: Sensor[] = [];
}

export class Hardware {
  name: string;
  path: string;
  type: string;
  children: Hardware[];
  sensors: SensorCollection;
  sensorTypes: string[];
  image: string;
}

export class Adjuster {  
  type: string;
  controlledSettingPaths: string[];
  watchedMeasurementPaths: string[];  
}

export class DefaultAdjuster extends Adjuster {
  readonly type = "DefaultAdjuster";
  readonly name = "System Default";

  settingPath: string = "";
}

export class FixedAdjuster extends Adjuster {
  readonly type = "FixedAdjuster";
  readonly name = "Fixed";

  fixedTarget: number = 100;
  settingPath: string = "";
}

export class LinearAdjuster extends Adjuster {
  readonly type = "LinearAdjuster";
  readonly name = "Linear";

  public lowerValue: number = 0;
  public upperValue: number = 0;
  public lowerTarget: number = 100;
  public upperTarget: number = 100;

  public measurementPath: string = "";
  public settingPath: string = "";
}
