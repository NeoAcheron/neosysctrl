import { Injectable } from '@angular/core';
import { Model, ModelFactory } from '@angular-extensions/model';
import { Observable } from 'rxjs';

const initialData: Hardware = { prop: 'value' };


@Injectable({
    providedIn: 'root'
})
export class HardwareService {
  private model: Model<Hardware>;

  hardware$: Observable<Hardware>;

  constructor(private modelFactory: ModelFactory<Hardware>) {
    this.model = this.modelFactory.create(initialData);
    this.hardware$ = this.model.data$;
  }

  updateProp(newPropValue: string) {
    const hardware = this.model.get();

    hardware.prop = newPropValue;

    this.model.set(hardware);
  }
}

export interface Hardware {
  prop: string;
}
