import { ChangeDetectionStrategy, Component, forwardRef, Inject, Injector, input, OnInit } from '@angular/core';
import { ControlValueAccessor, FormControl, FormControlDirective, FormControlName, FormGroupDirective, NG_VALUE_ACCESSOR, NgControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';

@Component({
  selector: 'x-dropdown',
  imports: [
    ReactiveFormsModule,
  ],
  templateUrl: './dropdown.component.html',
  styleUrl: './dropdown.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
      {
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => DropdownComponent),
        multi: true
      }
    ]
})
export class DropdownComponent<T> implements ControlValueAccessor, OnInit {
  displayErrors = input<boolean>(true);
  options = input.required<T[]>();
  public control!: FormControl;
  
  constructor(@Inject(Injector) private injector: Injector) {
  }

  // Based on https://stackoverflow.com/questions/45755958/how-to-get-formcontrol-instance-from-controlvalueaccessor
  // and https://levelup.gitconnected.com/angular-get-control-in-controlvalueaccessor-b7f09a485fba
  ngOnInit(): void {
    const injectedControl = this.injector.get(NgControl);

    switch (injectedControl.constructor) {
      // case NgModel: {
      //   const { control, update } = injectedControl as NgModel;

      //   this.control = control;

      //   this.control.valueChanges
      //     .pipe(
      //       tap((value: T) => update.emit(value)),
      //       takeUntil(this.destroy),
      //     )
      //     .subscribe();
      //   break;
      // }
      case FormControlName: {
        this.control = this.injector.get(FormGroupDirective).getControl(injectedControl as FormControlName);
        break;
      }
      default: {
        this.control = (injectedControl as FormControlDirective).form as FormControl;
        break;
      }
    }
  }
  
  writeValue(obj: any): void {
    //this.control.patchValue(obj);
  }
  registerOnChange(fn: any): void {
    //this.control.valueChanges.subscribe(val => fn(val))
  }
  registerOnTouched(fn: any): void {
    //this.control.valueChanges.subscribe(val => fn(val))
  }

  errors(): string {
    if (!this.displayErrors())
      return '';
    //https://stackoverflow.com/questions/40680321/get-all-validation-errors-from-angular-2-formgroup
    const controlErrors: ValidationErrors | null = this.control.errors;
    let str = '';
    if (controlErrors != null) {
      Object.keys(controlErrors).forEach(keyError => {
        //console.log('keyError: ' + keyError + ', err value: ', controlErrors[keyError]);
        if (keyError == 'required') {
        str += 'This field is required. ';
        } else {
        str += keyError += '. ';
        }
      });
    }
    return str;
  }
}
