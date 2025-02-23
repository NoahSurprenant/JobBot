import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, OnInit, resource, signal } from '@angular/core';
import { ToastService } from '../toast.service';
import { ButtonComponent } from '../button/button.component';
import { InputComponent } from '../input/input.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { QuestionBase, QuestionControlService } from '../questionControl.service';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { CommonModule } from '@angular/common';
import { PaginatorComponent } from '../paginator/paginator.component';

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

  toQuestionBase(x: QuestionDto[]): QuestionBase<string>[] {
    return x.map(t => new QuestionBase<string>({
      value: t.value ?? undefined,
      key: t.label,//.replace(".", "_"),
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
      return fetch(`api/questions?${params}&foo=foo`).then(x => x.json() as Promise<PaginationResult<QuestionDto>>);
    },
  });

  pr = computed(() => {
    return this.x.value() ?? { totalCount: 0, results: []};
  });

  form = computed(() => {
    return this.qcs.toFormGroup(this.toQuestionBase(this.pr().results));
  });
}

export interface QuestionDto
{
  label: string,
  value: string | null,
  questionPage: 'ContactInfo' | 'AdditionalQuestions' | 'WorkAuthorization',
  questionKind: 'AutoLine' | 'ComboBox' | 'Radio' | 'SingleLine',
  options: string[] | null,
  inputType: "text" | "tel" | "url" | "number" | "email" | "password" | null,
}


export interface PaginationResult<T>
{
  totalCount: number,
  results: T[],
}