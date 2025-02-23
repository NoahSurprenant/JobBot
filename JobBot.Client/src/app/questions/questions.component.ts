import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, effect, OnInit, resource, signal } from '@angular/core';
import { ToastService } from '../toast.service';
import { ButtonComponent } from '../button/button.component';
import { InputComponent } from '../input/input.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { QuestionBase, QuestionControlService } from '../questionControl.service';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { CommonModule } from '@angular/common';
import { PaginatorComponent } from '../paginator/paginator.component';
import { finalize } from 'rxjs';

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
  ],
  templateUrl: './questions.component.html',
  styleUrl: './questions.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuestionsComponent implements OnInit {
  constructor(private http: HttpClient, private toastService: ToastService, private qcs: QuestionControlService) {
    effect(() => {
      const x = this.x.value();
      if (x)
        this.current.set(x);
    })
  }

  isEven(i: number) {
    return i % 2 == 0;
  }

  ngOnInit(): void {
    this.x.reload();
  }

  clicked(): void {
    this.x.reload();
  }

  reset(): void {
    this.form().reset();
  }

  save(): void {
    const dirtyControls: { [key: string]: string | null } = {};
    const dirtyDto: QuestionDto[] = [];

    Object.keys(this.form().controls).forEach(controlName => {
      const control = this.form().get(controlName);
      if (control && control.dirty) {
        dirtyControls[controlName] = control.value;
        let dto = this.current().results.find(x => x.guid == controlName);
        if (dto) {
          dto = {...dto};
          dto.value = control.value;
          dirtyDto.push(dto);
        }
      }
    });

    console.log(dirtyControls);
    console.log(dirtyDto);

    this.http.post('api/SaveQuestions', dirtyDto)
      .pipe(finalize(() => {
        this.x.reload();
      }))
      .subscribe({
        next: () => this.toastService.show('Success'),
        error: () => this.toastService.show('Error'),
      });
  }

  toQuestionBase(x: QuestionDto[]): QuestionBase<string>[] {
    return x.map(t => new QuestionBase<string>({
      value: t.value ?? undefined,
      key: t.guid,
      //label: t.label,
      //required: false,
      //controlType: (t.questionKind == 'SingleLine' || t.questionKind == 'AutoLine') ? 'textbox' : (t.questionKind == 'ComboBox' || t.questionKind == 'Radio') ? 'dropdown' : ''
    }));
  }

  pageSize = signal(10);
  pageNumber = signal(1);

  x = resource({
    request: () => ({ pageSize: this.pageSize(), pageNumber: this.pageNumber() }),
    loader: async ({request}) => {
      const params = new URLSearchParams();
      params.set('pageSize', request.pageSize.toString());
      params.set('pageNumber', request.pageNumber.toString());
      return await fetch(`api/questions?${params}`)
        .then(x => x.json() as Promise<PaginationResult<QuestionDto>>)
        .then(x => {
          x.results.forEach(x => x.guid = this.uuidv4());
          return x;
        });
    },
  });

  form = computed(() => {
    return this.qcs.toFormGroup(this.toQuestionBase(this.current().results));
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
  questionPage: 'ContactInfo' | 'AdditionalQuestions' | 'WorkAuthorization',
  questionKind: 'AutoLine' | 'ComboBox' | 'Radio' | 'SingleLine',
  options: string[] | null,
  inputType: "text" | "tel" | "url" | "number" | "email" | "password" | null,
  guid: string,
}


export interface PaginationResult<T>
{
  totalCount: number,
  results: T[],
}