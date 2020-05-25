import { Pipe, PipeTransform } from '@angular/core';
import { Sensor } from './models/sensor/sensor.service';

@Pipe({
  name: 'isPrimary',
  pure: false
})
export class MeasurementIsPrimaryPipe implements PipeTransform {
  transform(allMeasurements: Sensor[], showPrimary?: boolean) {
    return allMeasurements.filter(m => m.primary == showPrimary);
  }
}

@Pipe({
  name: 'isHidden',
  pure: false
})
export class MeasurementIsHiddenPipe implements PipeTransform {
  transform(allMeasurements: Sensor[], showHidden?: boolean) {
    return allMeasurements.filter(m => m.hidden == showHidden);
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