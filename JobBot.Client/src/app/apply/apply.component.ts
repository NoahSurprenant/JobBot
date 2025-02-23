import { ChangeDetectionStrategy, Component, computed, Signal, signal } from '@angular/core';
import { InputComponent } from '../shared/input/input.component';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ButtonComponent } from '../shared/button/button.component';
import { toSignal } from '@angular/core/rxjs-interop';
import { HttpClient, HttpParams } from '@angular/common/http';
import { finalize } from 'rxjs';
import { ToastService } from '../toast.service';

@Component({
  selector: 'app-apply',
  imports: [
    InputComponent,
    ButtonComponent,
    ReactiveFormsModule,
  ],
  templateUrl: './apply.component.html',
  styleUrl: './apply.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ApplyComponent {
  form: FormGroup<MyFormGroup>;
  valueChanges: Signal<Partial<MyForm> | undefined>;

  constructor(private fb: FormBuilder, private http: HttpClient, private toastService: ToastService) {
    this.form = this.fb.group<MyFormGroup>({
      job: this.fb.control<string>('.net developer', Validators.required),
      location: this.fb.control<string>('Detroit Metropolitan Area', Validators.required),
    });
    this.valueChanges = toSignal(this.form.valueChanges);
  }

  clicked(): void {
    this.submitting.set(true);

    let params = new HttpParams()
      .set('job', this.form.value.job!)
      .set('location', this.form.value.location!);

    this.http.get('api/execute', { params: params })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (x) => {
          this.toastService.show('Success');
        },
        error: () => {
          this.toastService.show('Error');
        },
      })
  }

  submitting = signal<boolean>(false);

  disabled = computed<boolean>(() => {
    this.valueChanges();
    const s = this.submitting();
    const invalid = !this.form.valid;
    const final = s || invalid;
    return final;
  });
}

export interface MyFormGroup {
  job: FormControl<string | null>,
  location: FormControl<string | null>,
}

export interface MyForm {
  job: string | null,
  location: string | null,
}