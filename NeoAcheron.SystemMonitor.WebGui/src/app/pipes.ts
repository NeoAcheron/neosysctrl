import { Pipe, PipeTransform } from '@angular/core';
import { Sensor } from './models/sensor/sensor.service';

@Pipe({
  name: 'isPrimary',
  pure: false
})
export class MeasurementIsPrimaryPipe implements PipeTransform {
  transform(allMeasurements: Sensor[]) {
    return allMeasurements.filter(m => m.primary);
  }
}

@Pipe({
  name: 'notPrimary',
  pure: false
})
export class MeasurementNotPrimaryPipe implements PipeTransform {
  transform(allMeasurements: Sensor[]) {
    return allMeasurements.filter(m => !m.primary);
  }
}

@Pipe({
  name: 'adjusterNameConverter',
  pure: false
})
export class AdjusterNameConverterPipe implements PipeTransform {
  transform(internalName: string) {
    switch (internalName) {
      case "FixedAdjuster":
        return "Fixed";
      case "LinearAdjuster":
        return "Linear Control";
      default:
        return "System Default";
    }
  }
}