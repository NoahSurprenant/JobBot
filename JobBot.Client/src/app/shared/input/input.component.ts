import { ChangeDetectionStrategy, ChangeDetectorRef, Component, forwardRef, Inject, Injector, input, OnInit } from '@angular/core';
import { ControlValueAccessor, FormControl, FormControlDirective, FormControlName, FormGroupDirective, NG_VALUE_ACCESSOR, NgControl, ReactiveFormsModule, ValidationErrors } from '@angular/forms';

@Component({
  selector: 'x-input',
  imports: [
    ReactiveFormsModule,
  ],
  templateUrl: './input.component.html',
  styleUrl: './input.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => InputComponent),
      multi: true
    }
  ]
})
export class InputComponent implements ControlValueAccessor, OnInit {
  type = input<"text" | "tel" | "url" | "number" | "email" | "password">("text");
  placeholder = input<string>('');
  required = input<boolean>(false);
  displayErrors = input<boolean>(true);
  convertEmptyToNull = input<boolean>(true);

  public control!: FormControl;
  protected onTouched: (() => void) | undefined;
  protected onChange: ((value: string) => void) | undefined;

  constructor(@Inject(Injector) private injector: Injector, private _cdr: ChangeDetectorRef) {
  }

  // Based on https://stackoverflow.com/questions/45755958/how-to-get-formcontrol-instance-from-controlvalueaccessor
  // and https://levelup.gitconnected.com/angular-get-control-in-controlvalueaccessor-b7f09a485fba
  ngOnInit(): void {
    this.setControl();
  }
  
  private setControl() {
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

    // TODO: fix this?
    // this.control.events.subscribe({
    //   next: (x) => {
    //     if (this.convertEmptyToNull() && x.source.value === '')
    //       this.control.patchValue(null, { emitEvent: false });
    //   },
    // })
  }

  writeValue(obj: string): void {
    this.setControl(); // TODO: Is this the right place to do this?
    this.setValue(obj, false);
    this._cdr.markForCheck();
  }

  protected setValue(value: string, emitEvent: boolean) {
    this.control.patchValue(value, {emitEvent: false});
    if (emitEvent && this.onChange) {
        this.onChange(value);
        if (this.onTouched)
          this.onTouched();
    }
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {
    this.onTouched = fn;
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
