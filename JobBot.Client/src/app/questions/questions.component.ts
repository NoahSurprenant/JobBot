import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, effect, input, OnInit, resource, signal } from '@angular/core';
import { ToastService } from '../toast.service';
import { ButtonComponent } from '../shared/button/button.component';
import { InputComponent } from '../shared/input/input.component';
import { FormArray, FormBuilder, FormControl, FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { DropdownComponent } from '../shared/dropdown/dropdown.component';
import { CommonModule } from '@angular/common';
import { finalize } from 'rxjs';
import { PaginationResult } from '../paginationResult';
import { PaginatorComponent } from '../shared/paginator/paginator.component';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-questions',
  imports: [
    CommonModule,
    ButtonComponent,
    InputComponent,
    DropdownComponent,
    FormsModule,
    ReactiveFormsModule,
    PaginatorComponent,
    RouterModule,
  ],
  templateUrl: './questions.component.html',
  styleUrl: './questions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuestionsComponent implements OnInit {
  jobID = input<number>();

  formArray: FormArray<FormGroup<QuestionFormRow>>;

  constructor(private http: HttpClient,
    private toastService: ToastService,
    private fb: FormBuilder) {
      
    this.formArray = fb.nonNullable.array<FormGroup<QuestionFormRow>>([]);

    effect(() => {
      
      const x = this.x.value();
      if (x)
        this.current.set(x);
    })

    effect(() => {
      if (this.questionFilter().jobID == this.jobID())
        return;
      this.questionFilter.update(x => {
        x.jobID = this.jobID() ?? null;
        return {...x};
      });
    });

    effect(() => {
      const x = this.current();
      const map = x.results.map(x => {
        const newObj: QuestionFormRow = {
          dto: fb.nonNullable.control(x),
          control: fb.nonNullable.control(x.value),
        }
        return fb.nonNullable.group<QuestionFormRow>(newObj);
      });
      this.formArray = fb.nonNullable.array<FormGroup<QuestionFormRow>>(map);
    });
  }

  isEven(i: number) {
    return i % 2 == 0;
  }

  ngOnInit(): void {
    this.x.reload();

    this.valueControl.valueChanges.subscribe({
      next: (x) => {
        this.questionFilter.update(x => {
          if (x.value != null) {
            x.value.value = this.valueControl.value;
          }
          return {...x};
        });
      },
    });
  }

  questionFilter = signal<QuestionFilter>({value: null, jobID: this.jobID() ?? null});

  toggleValueFilter() {
    this.questionFilter.update(x => {
      if (x.value == null)
        x.value = { value: this.valueControl.value };
      else 
        x.value = null;
      return {...x};
    });
  }

  valueControl: FormControl<string | null> = new FormControl(null);

  clicked(): void {
    this.x.reload();
  }

  save(): void {
    const dirtyDto = this.formArray.controls.filter(x => x.dirty).map(x => {
      const dto = {...x.value.dto!};
      dto.value = x.value.control ?? null;
      return dto;
    });

    this.http.post('api/SaveQuestions', dirtyDto)
      .pipe(finalize(() => {
        this.x.reload();
      }))
      .subscribe({
        next: () => this.toastService.show('Success'),
        error: () => this.toastService.show('Error'),
      });
  }

  pageSize = signal(10);
  pageNumber = signal(1);

  x = resource({
    request: () => ({ pageSize: this.pageSize(), pageNumber: this.pageNumber(), questionFilter: this.questionFilter() }),
    loader: async ({request}) => {
      const params = new URLSearchParams();
      params.set('pageSize', request.pageSize.toString());
      params.set('pageNumber', request.pageNumber.toString());
      return await fetch(`api/questions?${params}`, {
          method: 'POST',
          body: JSON.stringify(request.questionFilter),
          headers: {
            "Content-Type": "application/json",
          },
        })
        .then(x => x.json() as Promise<PaginationResult<QuestionDto>>);
    },
  });

  current = signal<PaginationResult<QuestionDto>>({ totalCount: 0, results: []});

  uuidv4() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'
    .replace(/[xy]/g, function (c) {
        const r = Math.random() * 16 | 0, 
            v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
  }
}

export interface QuestionDto
{
  label: string,
  value: string | null,
  questionKind: 'AutoLine' | 'ComboBox' | 'Radio' | 'SingleLine',
  options: string[] | null,
  inputType: "text" | "tel" | "url" | "number" | "email" | "password" | null,
  attachedJobs: number,
}

export interface QuestionFormRow {
  dto: FormControl<QuestionDto>,
  control: FormControl<string | null>,
}

export interface QuestionFilter {
  value: PropertyFilter | null,
  jobID: number | null,
}

export interface PropertyFilter {
  value: string | null,
}
