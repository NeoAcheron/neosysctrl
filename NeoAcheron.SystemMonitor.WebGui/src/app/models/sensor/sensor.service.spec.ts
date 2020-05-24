import { TestBed, inject, async } from '@angular/core/testing';

import { SensorService } from './sensor.service';

describe('SensorService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created',
    inject([SensorService], (service: SensorService) => {
      expect(service).toBeTruthy();
    })
  );

  it('should add item',
    async(
      inject([SensorService], (service: SensorService) => {
        service.addSensor({ name: 'test', controlPath: '', parentName: '', path: '', primary: false, type: '', unit: '', value: 0 });
        service.sensors$.subscribe(sensors => expect(sensors.length).toBe(1));
      })
    )
  );
});
