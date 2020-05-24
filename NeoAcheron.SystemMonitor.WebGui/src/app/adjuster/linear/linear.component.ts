import { Component, OnInit, OnDestroy, Input, Output, EventEmitter, AfterViewInit } from '@angular/core';
import { LinearAdjuster, Adjuster } from 'src/app/models';
import { SensorService, Sensor } from '../../models/sensor/sensor.service';
import * as Chart from 'chart.js';
import * as ChartAnnotation from 'chartjs-plugin-annotation';
import { Observable } from 'rxjs/internal/Observable';
import { BehaviorSubject } from 'rxjs/internal/BehaviorSubject';
import { FormControl, Validators } from '@angular/forms';

@Component({
  selector: 'app-adjuster-linear',
  templateUrl: './linear.component.html',
  styleUrls: ['./linear.component.scss']
})
export class LinearComponent extends LinearAdjuster implements OnInit, AfterViewInit, OnDestroy {

  @Input()
  targetSensor: Sensor;
  @Input()
  adjuster: LinearAdjuster;

  @Output()
  valueChange: EventEmitter<Adjuster> = new EventEmitter<Adjuster>();

  selectedMeasurementValue: number;
  selectedMeasurementUnit: string = "";
  updateInterval: NodeJS.Timeout;

  valueControl = new FormControl('', Validators.required);

  private currentValueAnnotationOptions: any;
  private currentTargetAnnotationOptions: any;

  canvas: any;
  ctx: any;
  chart: any;

  constructor(public sensorService: SensorService) {
    super();

  }

  public UpdateChart(): void {

    this.chart.data.datasets.forEach((dataset) => {
      dataset.data = [{
        x: 0,
        y: this.lowerTarget
      }, {
        x: this.lowerValue,
        y: this.lowerTarget
      }, {
        x: this.upperValue,
        y: this.upperTarget
      }, {
        x: 100,
        y: this.upperTarget
      }];
    });

    this.chart.options.annotation.annotations.forEach(annotation => {
      var id = annotation.id;
      var options = this.chart.annotation.elements[id].options;
      switch (id) {
        case "a-lowerValue":
          options.value = this.lowerValue;
          options.label.content = options.value + this.selectedMeasurementUnit;
          break;
        case "a-upperValue":
          options.value = this.upperValue;
          options.label.content = options.value + this.selectedMeasurementUnit;
          break;
        case "a-lowerTarget":
          options.value = this.lowerTarget;
          options.label.content = options.value + this.targetSensor.unit;
          break;
        case "a-upperTarget":
          options.value = this.upperTarget;
          options.label.content = options.value + this.targetSensor.unit;
          break;
      }
    });

    this.chart.update();
  }

  UpdateSelectedMeasurement(): void {
    this.measurementPath = this.valueControl.value;
    var measurementPath = this.measurementPath;

    var sensor = this.sensorService.getSensor(measurementPath);

    if (this.updateInterval !== undefined) {
      clearInterval(this.updateInterval);
      this.updateInterval = undefined;
    }

    if (sensor === undefined) return;
    this.selectedMeasurementUnit = sensor.unit;
    this.selectedMeasurementValue = sensor.value;

    var lastValue = 0;
    var lastTarget = 0;

    var updateFunction = function () {
      this.selectedMeasurementUnit = sensor.unit;
      this.selectedMeasurementValue = sensor.value;
      if (this.chart) {
        if (this.currentTargetAnnotationOptions) {
          this.currentTargetAnnotationOptions.value = this.targetSensor.value;
          this.currentTargetAnnotationOptions.label.content = `${this.targetSensor.value.toPrecision(3)}${this.targetSensor.unit}`;
        }
        if (this.currentValueAnnotationOptions) {
          this.currentValueAnnotationOptions.value = sensor.value;
          this.currentValueAnnotationOptions.label.content = `${sensor.value.toPrecision(3)}${this.selectedMeasurementUnit}`;
        }
        if (lastValue !== sensor.value || lastTarget !== this.targetSensor.value) {
          this.chart.update();
          lastValue = sensor.value;
          lastTarget = this.targetSensor.value;
        }
      }
    }.bind(this);

    this.PublishUpdate();
    updateFunction();
    this.updateInterval = setInterval(updateFunction, 500);
  }

  LockLowerValue(): void {
    this.upperValue = Math.max(this.upperValue, this.lowerValue);
  }

  LockUpperValue(): void {
    this.lowerValue = Math.min(this.upperValue, this.lowerValue);
  }

  LockLowerTarget(): void {
    this.upperTarget = Math.max(this.upperTarget, this.lowerTarget);
  }

  LockUpperTarget(): void {
    this.lowerTarget = Math.min(this.upperTarget, this.lowerTarget);
  }

  public PublishUpdate(): void {

    this.settingPath = this.targetSensor.controlPath;

    if (!this.measurementPath) return;

    var adjuster = new LinearAdjuster();
    adjuster.measurementPath = this.measurementPath;
    adjuster.settingPath = this.settingPath;
    adjuster.lowerTarget = this.lowerTarget;
    adjuster.upperTarget = this.upperTarget;
    adjuster.lowerValue = this.lowerValue;
    adjuster.upperValue = this.upperValue;

    this.valueChange.emit(adjuster);
  }

  ngOnDestroy(): void {
    if (this.updateInterval !== undefined) {
      clearInterval(this.updateInterval);
      this.updateInterval = undefined;
    }
    if (this.chart) {
      this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
      this.chart = null;
      this.canvas = null;
      this.ctx = null;
    }
  }

  ngAfterViewInit(): void {
    this.canvas = document.getElementById('myChart');
    this.ctx = this.canvas.getContext('2d');

    var chartOptions = {
      type: 'line',
      plugins: [ChartAnnotation],
      data: {
        datasets: [{
          data: [],
          borderColor: [
            'rgb(33,243,126)'
          ],
          fill: false,
          borderWidth: 3,
          lineTension: 0,
          pointStyle: "line"
        }]
      },
      options: {
        animation: {
          duration: 0
        },
        legend: {
          display: false
        },
        responsive: true,
        scales: {
          xAxes: [{
            type: "linear",
            bounds: "ticks",
            ticks: {
              callback: function (value, index, values) {
                if (this.selectedMeasurementUnit) {
                  return value + this.selectedMeasurementUnit;
                } else {
                  return value;
                }
              },
              fontColor: '#b7b7b7',
              fontSize: 14,
              beginAtZero: true
            },
            gridLines: {
              display: false
            }
          }],
          yAxes: [{
            type: 'linear',
            bounds: "ticks",
            ticks: {
              callback: function (value, index, values) {
                if (this.targetSensor) {
                  return value + this.targetSensor.unit;
                } else {
                  return value;
                }
              },
              beginAtZero: true,
              fontColor: '#b7b7b7',
              fontSize: 14,
              suggestedMin: 0,
              suggestedMax: 100
            },
            gridLines: {
              drawBorder: false,
            }
          }]
        },
        annotation: {
          annotations: [
            {
              id: "a-lowerValue",
              type: 'line',
              mode: 'vertical',
              scaleID: 'x-axis-0',
              value: 0,
              borderColor: 'rgba(0,0,0,0.2)',
              borderWidth: 3,
              label: {
                backgroundColor: 'rgba(0,0,0,0.5)',
                fontFamily: "sans-serif",
                fontSize: 12,
                fontStyle: "bold",
                fontColor: "#fff",
                xPadding: 6,
                yPadding: 6,
                cornerRadius: 6,
                position: "bottom",
                xAdjust: 0,
                yAdjust: 0,
                enabled: true,
                content: "",
                rotation: 0
              },
            },
            {
              id: "a-upperValue",
              type: 'line',
              mode: 'vertical',
              scaleID: 'x-axis-0',
              value: 100,
              borderColor: 'rgba(0,0,0,0.2)',
              borderWidth: 3,
              label: {
                backgroundColor: 'rgba(0,0,0,0.5)',
                fontFamily: "sans-serif",
                fontSize: 12,
                fontStyle: "bold",
                fontColor: "#fff",
                xPadding: 6,
                yPadding: 6,
                cornerRadius: 6,
                position: "top",
                xAdjust: 0,
                yAdjust: 0,
                enabled: true,
                content: "",
                rotation: 0
              }
            },
            {
              //drawTime: 'beforeDatasetsDraw',
              id: "a-lowerTarget",
              type: 'line',
              mode: 'horizontal',
              scaleID: 'y-axis-0',
              value: 70,
              borderColor: 'rgba(0,0,0,0)',
              borderWidth: 2,
              label: {
                backgroundColor: 'rgba(0,0,0,0.5)',
                fontFamily: "sans-serif",
                fontSize: 12,
                fontStyle: "bold",
                fontColor: "#fff",
                xPadding: 6,
                yPadding: 6,
                cornerRadius: 6,
                position: "left",
                xAdjust: 0,
                yAdjust: 0,
                enabled: true,
                content: "",
                rotation: 0
              }
            },
            {
              //drawTime: 'beforeDatasetsDraw',
              id: "a-upperTarget",
              type: 'line',
              mode: 'horizontal',
              scaleID: 'y-axis-0',
              value: 70,
              borderColor: 'rgba(0,0,0,0)',
              borderWidth: 1,
              label: {
                backgroundColor: 'rgba(0,0,0,0.5)',
                fontFamily: "sans-serif",
                fontSize: 12,
                fontStyle: "bold",
                fontColor: "#fff",
                xPadding: 6,
                yPadding: 6,
                cornerRadius: 6,
                position: "right",
                xAdjust: 0,
                yAdjust: 0,
                enabled: true,
                content: "",
                rotation: 0
              }
            },
            {
              drawTime: 'beforeDatasetsDraw',
              id: "a-currentValue",
              type: 'line',
              mode: 'vertical',
              scaleID: 'x-axis-0',
              value: 0,
              borderColor: 'rgba(255,255,255,1)',
              borderWidth: 1,
              borderDash: [2, 2],

              label: {
                backgroundColor: 'rgba(0,0,0,0.5)',
                fontFamily: "sans-serif",
                fontSize: 12,
                fontStyle: "bold",
                fontColor: "#fff",
                xPadding: 6,
                yPadding: 6,
                cornerRadius: 6,
                position: "left",
                xAdjust: 0,
                yAdjust: 0,
                enabled: false,
                content: "",
                rotation: 0
              }
            },
            {
              drawTime: 'beforeDatasetsDraw',
              id: "a-currentTarget",
              type: 'line',
              mode: 'horizontal',
              scaleID: 'y-axis-0',
              value: 0,
              borderColor: 'rgba(255,255,255,1)',
              borderWidth: 1,
              borderDash: [2, 2],

              label: {
                backgroundColor: 'rgba(0,0,0,0.5)',
                fontFamily: "sans-serif",
                fontSize: 12,
                fontStyle: "bold",
                fontColor: "#fff",
                xPadding: 6,
                yPadding: 6,
                cornerRadius: 6,
                position: "left",
                xAdjust: 0,
                yAdjust: 0,
                enabled: false,
                content: "",
                rotation: 0
              }
            }
          ]
        }
      }
    };
    this.chart = new Chart(this.ctx, chartOptions as Chart.ChartConfiguration);


    this.chart.options.annotation.annotations.forEach(annotation => {
      var id = annotation.id;
      var options = this.chart.annotation.elements[id].options;
      if (id === "a-currentValue") {
        this.currentValueAnnotationOptions = options;
      } else if (id === "a-currentTarget") {
        this.currentTargetAnnotationOptions = options;
      }
    });

    this.UpdateChart();
  }


  ngOnInit(): void {
    this.lowerValue = 30;
    this.upperValue = 60;

    this.lowerTarget = 25;
    this.upperTarget = 80;

    Object.keys(this.adjuster).forEach(key => {
      if (key !== "type") {
        return this[key] = this.adjuster[key];
      }
    });
    this.valueControl.patchValue(this.measurementPath);
    this.valueControl.markAsTouched();
    this.UpdateSelectedMeasurement();
  }

}
