import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FixedAdjuster, Adjuster } from 'src/app/models';
import { Sensor } from '../../models/sensor/sensor.service';

@Component({
  selector: 'app-adjuster-fixed',
  templateUrl: './fixed.component.html',
  styleUrls: ['./fixed.component.scss']
})
export class FixedComponent extends FixedAdjuster implements OnInit {
  
  @Input()
  targetSensor: Sensor;
  @Input()
  adjuster: FixedAdjuster = new FixedAdjuster();

  @Output()
  valueChange: EventEmitter<Adjuster> = new EventEmitter<Adjuster>();

  public PublishUpdate(): void {
    var adjuster = new FixedAdjuster();

    adjuster.fixedTarget = this.fixedTarget;
    adjuster.settingPath = this.settingPath;

    this.valueChange.emit(adjuster);
  }

  constructor() {
    super();
  }

  ngOnInit(): void {
    this.settingPath = this.targetSensor.controlPath;
    this.fixedTarget = this.adjuster.fixedTarget;

    if(!this.fixedTarget){
      this.fixedTarget = this.targetSensor.value;
    }
  }

}
