import { Injectable } from '@angular/core';
import { Model, ModelFactory } from '@angular-extensions/model';
import { Observable } from 'rxjs';

const initialData: any = {};

@Injectable({
  providedIn: 'root'
})
export class SensorService {
  private model: Model<any>;

  sensors$: Observable<any>;

  constructor(private modelFactory: ModelFactory<any>) {
    this.model = this.modelFactory.createMutable(initialData);
    this.sensors$ = this.model.data$;
  }

  addSensor(sensor: Sensor): void {
    const sensors = this.model.get();
    sensors[sensor.path] = sensor;
    this.model.set(sensors);
  }

  getSensor(path: string): Sensor {
    const sensors = this.model.get();
    return sensors[path];
  }

  getSensors(): Sensor[] {
    return Object.values(this.model.get());
  }
}

export class Sensor {
  name: string;
  path: string;
  type: string;
  value: number;
  primary: boolean;
  hidden: boolean;
  unit: string;
  controlPath: string;
  parentName: string;
}
