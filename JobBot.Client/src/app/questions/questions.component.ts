import { HttpClient, HttpParams } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, OnInit, signal } from '@angular/core';
import { ToastService } from '../toast.service';
import { combineLatest, distinctUntilChanged, finalize, Observable, switchMap, tap } from 'rxjs';
import { ButtonComponent } from '../button/button.component';
import { InputComponent } from '../input/input.component';
import { FormGroup, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { QuestionBase, QuestionControlService } from '../questionControl.service';
import { DropdownComponent } from '../dropdown/dropdown.component';
import { CommonModule } from '@angular/common';
import { PaginatorComponent } from '../paginator/paginator.component';
import { toObservable } from '@angular/core/rxjs-interop';

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
  loading = signal<boolean>(false);
  pr = signal<PaginationResult<QuestionDto>>({ totalCount: 0, results: []});
  questions = signal<QuestionDto[]>([]);
  form!: FormGroup;

  constructor(private http: HttpClient, private toastService: ToastService, private qcs: QuestionControlService) {
  }

  isEven(i: number) {
    return i % 2 == 0;
  }

  isOdd(i: number) {
    return !this.isEven(i);
  }

  ngOnInit(): void {
    combineLatest({pageSize: this.pageSize$.pipe(distinctUntilChanged()), pageNumber: this.pageNumber$.pipe(distinctUntilChanged())})
      .pipe(
        distinctUntilChanged(),
        switchMap((x) => this.load(x.pageSize, x.pageNumber))
      )
      .subscribe();
  }

  clicked(): void {
    this.load(this.pageSize(), this.pageNumber()).subscribe();
  }

  load(pageSize: number, pageNumber: number): Observable<PaginationResult<QuestionDto>> {
    this.loading.set(true);

    let params = new HttpParams().set('pageSize', pageSize).set('pageNumber', pageNumber);

    return this.http.get<PaginationResult<QuestionDto>>('api/questions', {params: params})
      .pipe(finalize(() => this.loading.set(false)))
      .pipe(tap((x) => {
        this.pr.set(x);
          this.questions.set(x.results);
          this.form = this.qcs.toFormGroup(this.toQuestionBase(x.results));
          this.toastService.show('Success');
      }));
      // .subscribe({
      //   next: (x) => {
      //     this.pr.set(x);
      //     this.questions.set(x.results);
      //     this.form = this.qcs.toFormGroup(this.toQuestionBase(x.results));
      //     this.toastService.show('Success');
      //   },
      //   error: () => {
      //     this.toastService.show('Error');
      //   },
      // })
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
  pageSize$ = toObservable(this.pageSize);
  pageNumber$ = toObservable(this.pageNumber);
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